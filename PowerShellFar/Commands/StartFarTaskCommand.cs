using FarNet;
using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PowerShellFar.Commands;

[OutputType(typeof(Task<object[]>))]
sealed class StartFarTaskCommand : BaseCmdlet, IDynamicParameters
{
	const string
		KeyData = "Data",
		ScriptBlockFunction = "ScriptBlockFunction";

	// invokes task scripts in a new session
	const string CodeTask = """
	param($Script, $Data, $Parameters)
	. $Script.GetNewClosure() @Parameters
	""";

	// invokes job scripts in the main session
	const string CodeJob = """
	param($Script, $Data, $Arguments)
	. $Script.GetNewClosure() @Arguments
	""";

	// sets step breaks
	const string CodeStep = """
	Set-PSBreakpoint -Command job, ps:, run, keys, macro
	""";

	// tasks initial state
	static readonly InitialSessionState _iss;

	// $Data for scripts
	readonly Hashtable _data = new(StringComparer.OrdinalIgnoreCase);

	// task script
	ScriptBlock _script = null!;
	bool _isScript;
	Dictionary<string, ParameterMetadata>? _scriptParameters;
	RuntimeDefinedParameterDictionary? _dynamicParameters;
	static readonly string[] _paramInvalid = [nameof(Script), nameof(Data), nameof(AsTask), nameof(AddDebugger), nameof(Step)];

	[Parameter(Position = 0, Mandatory = true)]
	public object Script
	{
		set
		{
			value = value.ToBaseObject();

			if (value is ScriptBlock block)
			{
				_script = block;
				_isScript = true;

				SessionState.PSVariable.Set($"function:{ScriptBlockFunction}", block);
				var info = (FunctionInfo)SessionState.InvokeCommand.GetCommand(ScriptBlockFunction, CommandTypes.Function);
				_scriptParameters = info.Parameters;

				return;
			}

			if (value is not string text)
				throw new PSArgumentException("Invalid script, expected script block or file name or script code.");

			if (text.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
			{
				//! do not resolve and test by File.Exists, use normal script resolution, including scripts in the path
				var info = (ExternalScriptInfo)SessionState.InvokeCommand.GetCommand(text, CommandTypes.ExternalScript)
					?? throw new PSArgumentException($"Cannot find the script '{text}'.");

				//! throws on syntax errors
				_script = info.ScriptBlock;
				_scriptParameters = info.Parameters;
			}
			else
			{
				//! raw text for interop like FSharpFar
				_script = ScriptBlock.Create(text);
			}
		}
	}

	[Parameter]
	public object[] Data
	{
		set
		{
			if (value is null)
				return;

			foreach (var item in value)
			{
				var nameOrData = item.ToBaseObject();
				if (nameOrData is string name)
				{
					var variable = SessionState.PSVariable.Get(name) ?? throw new PSArgumentException($"Variable {name} is not found.");
					_data.Add(variable.Name, variable.Value);
				}
				else if (nameOrData is IDictionary data)
				{
					foreach (DictionaryEntry kv in data)
						_data[kv.Key] = kv.Value;
				}
				else
				{
					throw new PSArgumentNullException($"Invalid Data item type: {nameOrData?.GetType()}.");
				}
			}
		}
	}

	[Parameter]
	public SwitchParameter AsTask { get; set; }

	[Parameter]
	public Hashtable? AddDebugger { get; set; }

	[Parameter]
	public SwitchParameter Step { get; set; }

	static void ShowError(Exception exception)
	{
		Far.Api.ShowError("FarTask error", exception);
	}

	public object? GetDynamicParameters()
	{
		if (_scriptParameters is null || _scriptParameters.Count == 0)
			return null;

		_dynamicParameters = [];
		var commonParameters = CommonParameters;
		foreach (var p in _scriptParameters.Values)
		{
			if (commonParameters.Contains(p.Name))
				continue;

			if (_paramInvalid.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
				throw new InvalidOperationException($"Task script cannot use parameter: {string.Join(", ", _paramInvalid)}");

			_dynamicParameters.Add(p.Name, new RuntimeDefinedParameter(p.Name, p.ParameterType, p.Attributes));
		}
		return _dynamicParameters;
	}

	// jobs and macros base
	public class BaseCommand : PSCmdlet
	{
		protected Hashtable GetData()
		{
			return (Hashtable)GetVariableValue(KeyData);
		}
	}

	// jobs base
	public class BaseJob : BaseCommand
	{
		[Parameter(Position = 0)]
		public ScriptBlock Script { get; set; } = null!;

		[Parameter(Position = 1)]
		public object[] Arguments { get; set; } = null!;

		protected override void BeginProcessing()
		{
			if (Script == null)
				throw new PSArgumentNullException(nameof(Script));
		}
	}

	// job {...}
	public class InvokeTaskJob : BaseJob
	{
		protected override void BeginProcessing()
		{
			var data = GetData();

			// post the job as task
			var task = Tasks.Job(() =>
			{
				// invoke script block in the main session
				var ps = A.Psf.NewPowerShell();

				ps.AddScript(CodeJob, true).AddArgument(Script).AddArgument(data).AddArgument(Arguments);
				var output = ps.Invoke();

				//! Assert-Far may stop by PipelineStoppedException
				if (ps.InvocationStateInfo.Reason != null)
					throw ps.InvocationStateInfo.Reason;

				return output;
			});

			// await
			var result = task.AwaitResult();
			FarNet.Works.Far2.Api.WaitSteps().Await();

			//! if the job returns a task, await and return
			if (result.Count == 1 && result[0]?.BaseObject is Task task2)
			{
				task2.Await();

				var result2 = task2.GetType().GetProperty("Result")?.GetValue(task2);
				if (result2 != null)
					WriteObject(result2);
			}
			else
			{
				foreach (var it in result)
					WriteObject(it);
			}
		}
	}

	// ps: {...}
	public class InvokeTaskCmd : BaseJob
	{
		protected override void BeginProcessing()
		{
			var data = GetData();

			Exception? reason = null;

			// post the job as task
			var task = Tasks.Job(() =>
			{
				var args = new RunArgs(CodeJob)
				{
					Writer = new ConsoleOutputWriter(),
					NoOutReason = true,
					UseLocalScope = true,
					Arguments = [Script, data, Arguments]
				};
				A.Psf.Run(args);
				reason = args.Reason;
			});

			// await
			task.Await();
			FarNet.Works.Far2.Api.WaitSteps().Await();
			if (reason != null)
				throw reason;
		}
	}

	// run {...}
	public class InvokeTaskRun : BaseJob
	{
		protected override void BeginProcessing()
		{
			var data = GetData();

			// post the job as task
			var task = Tasks.Run(() =>
			{
				var ps = A.Psf.NewPowerShell();
				ps.AddScript(CodeJob, true).AddArgument(Script).AddArgument(data).AddArgument(Arguments);
				ps.Invoke();

				//! Assert-Far may stop by PipelineStoppedException
				if (ps.InvocationStateInfo.Reason != null)
					throw ps.InvocationStateInfo.Reason;
			});

			// await
			task.Await();
			FarNet.Works.Far2.Api.WaitSteps().Await();
		}
	}

	// keys ...
	public class InvokeTaskKeys : BaseCommand
	{
		[Parameter(ValueFromRemainingArguments = true)]
		public string[] Keys { get; set; } = null!;

		protected override void BeginProcessing()
		{
			if (Keys is null || Keys.Length == 0)
				throw new PSArgumentNullException(nameof(Keys));

			var keys = string.Join(" ", Keys);

			Tasks.Keys(keys).Await();
			FarNet.Works.Far2.Api.WaitSteps().Await();
		}
	}

	// macro ...
	public class InvokeTaskMacro : BaseCommand
	{
		[Parameter(Position = 0)]
		public string Macro { get; set; } = null!;

		protected override void BeginProcessing()
		{
			if (Macro == null)
				throw new PSArgumentNullException(nameof(Macro));

			Tasks.Macro(Macro).Await();
			FarNet.Works.Far2.Api.WaitSteps().Await();
		}
	}

	const string NameInvokeTaskJob = "Invoke-TaskJob";
	const string NameInvokeTaskCmd = "Invoke-TaskCmd";
	const string NameInvokeTaskRun = "Invoke-TaskRun";
	const string NameInvokeTaskKeys = "Invoke-TaskKeys";
	const string NameInvokeTaskMacro = "Invoke-TaskMacro";

	static StartFarTaskCommand()
	{
		_iss = InitialSessionState.CreateDefault();

		_iss.Variables.Add([
			new("ErrorActionPreference", ActionPreference.Stop, string.Empty)
		]);

		_iss.Commands.Add([
			new SessionStateAliasEntry("job", NameInvokeTaskJob),
			new SessionStateAliasEntry("ps:", NameInvokeTaskCmd),
			new SessionStateAliasEntry("run", NameInvokeTaskRun),
			new SessionStateAliasEntry("keys", NameInvokeTaskKeys),
			new SessionStateAliasEntry("macro", NameInvokeTaskMacro),
			new SessionStateCmdletEntry(NameInvokeTaskJob, typeof(InvokeTaskJob), string.Empty),
			new SessionStateCmdletEntry(NameInvokeTaskCmd, typeof(InvokeTaskCmd), string.Empty),
			new SessionStateCmdletEntry(NameInvokeTaskRun, typeof(InvokeTaskRun), string.Empty),
			new SessionStateCmdletEntry(NameInvokeTaskKeys, typeof(InvokeTaskKeys), string.Empty),
			new SessionStateCmdletEntry(NameInvokeTaskMacro, typeof(InvokeTaskMacro), string.Empty),
		]);
	}

	protected override void BeginProcessing()
	{
		// parameters
		var parameters = new Hashtable();
		if (_dynamicParameters is { })
		{
			foreach (var parameter in _dynamicParameters.Values)
			{
				if (parameter.IsSet)
				{
					parameters[parameter.Name] = parameter.Value;
					_data[parameter.Name] = parameter.Value;
				}
				else if (_isScript)
				{
					var variable = SessionState.PSVariable.Get(parameter.Name) ??
						throw new PSArgumentException($"Parameter '{parameter.Name}' should be specified or variable '{parameter.Name}' should exist.");

					_data[parameter.Name] = variable.Value;
				}
			}
		}

		// open new session
		var rs = RunspaceFactory.CreateRunspace(_iss);
		rs.Open();

		// sync file system location
		rs.SessionStateProxy.Path.SetLocation(SessionState.Path.CurrentFileSystemLocation.Path);

		// PowerShell invoker
		var ps = PowerShell.Create(rs);

		// debugging
		if (Step || AddDebugger is { })
		{
			AddDebuggerKit.ValidateAvailable();

			// import breakpoints
			foreach (var bp in A.Psf.Runspace.Debugger.GetBreakpoints())
			{
				if (bp is CommandBreakpoint cbp)
				{
					rs.Debugger.SetCommandBreakpoint(cbp.Command, cbp.Action, cbp.Script);
					continue;
				}

				if (bp is LineBreakpoint lbp)
				{
					rs.Debugger.SetLineBreakpoint(lbp.Script, lbp.Line, lbp.Column, lbp.Action);
					continue;
				}

				if (bp is VariableBreakpoint vbp)
				{
					rs.Debugger.SetVariableBreakpoint(vbp.Variable, vbp.AccessMode, vbp.Action, vbp.Script);
					continue;
				}
			}

			// load debugger assets
			AddDebuggerKit.AddDebugger(ps, AddDebugger);
			if (Step)
			{
				ps.AddScript(CodeStep).Invoke();
				ps.Commands.Clear();
			}
		}

		// add task script
		ps.AddScript(CodeTask).AddArgument(_script).AddArgument(_data).AddArgument(parameters);

		// start
		var tcs = AsTask ? new TaskCompletionSource<object[]>() : null;
		ps.BeginInvoke<object>(null, null, Callback, null);
		if (AsTask)
			WriteObject(tcs!.Task);

		void Callback(IAsyncResult asyncResult)
		{
			try
			{
				if (ps.InvocationStateInfo.Reason is { } ex)
				{
					if (AsTask)
					{
						tcs!.SetException(FarNet.Works.Kit.UnwrapAggregateException(ex));
					}
					else
					{
						Far.Api.PostJob(() =>
						{
							ShowError(ex);
						});
					}
				}
				else
				{
					var result = ps.EndInvoke(asyncResult);
					if (AsTask)
						tcs!.SetResult(PS2.UnwrapPSObject(result));
				}
			}
			finally
			{
				ps.Dispose();
				rs.Dispose();
			}
		}
	}
}
