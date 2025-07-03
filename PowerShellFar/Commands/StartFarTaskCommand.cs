
using FarNet;
using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;

namespace PowerShellFar.Commands;

[OutputType(typeof(Task<object[]>))]
sealed class StartFarTaskCommand : BaseCmdlet, IDynamicParameters
{
	internal const string
		NameData = "Data",
		NameVar = "Var",
		NameInvokeTaskJob = "Invoke-TaskJob",
		NameInvokeTaskCmd = "Invoke-TaskCmd",
		NameInvokeTaskRun = "Invoke-TaskRun",
		NameInvokeTaskKeys = "Invoke-TaskKeys",
		NameInvokeTaskMacro = "Invoke-TaskMacro",
		ScriptBlockFunction = "ScriptBlockFunction";

	// invokes task scripts
	const string CodeTask = "param($_) . $args[0] @_";

	// invokes run and ps: blocks
	internal const string CodeJob = "& $args[0]";

	// sets step breaks
	const string CodeStep = "Set-PSBreakpoint -Command job, ps:, run, keys, macro";

	// tasks initial state
	static readonly InitialSessionState _iss;

	// $Data for scripts
	readonly Hashtable _data = new(StringComparer.OrdinalIgnoreCase);

	// task script
	ScriptBlock _script = null!;
	RuntimeDefinedParameterDictionary? _dynamicParameters;
	Dictionary<string, ParameterMetadata>? _scriptParameters;
	static readonly string[] s_paramInvalid = [nameof(Script), nameof(Data), nameof(AsTask), nameof(AddDebugger), nameof(Step)];

	[Parameter(Position = 0, Mandatory = true)]
	public object Script
	{
		set
		{
			value = value.ToBaseObject();

			if (value is string text)
			{
				if (text.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
				{
					//! do not resolve and test by File.Exists, use normal script resolution, including scripts in the path
					var scriptInfo = (ExternalScriptInfo)SessionState.InvokeCommand.GetCommand(text, CommandTypes.ExternalScript)
						?? throw new PSArgumentException($"Cannot find the script '{text}'.");

					//! throws on syntax errors
					_script = scriptInfo.ScriptBlock;
					_scriptParameters = scriptInfo.Parameters;

					return;
				}

				//! code for interop like FSharpFar
				//! script from text is unbound
				_script = ScriptBlock.Create(text);
			}
			else if (value is ScriptBlock block)
			{
				//! ensure unbound script, to use task session
				_script = ((ScriptBlockAst)block.Ast).GetScriptBlock();
			}
			else
			{
				throw new PSArgumentException("Invalid script, expected script block or file name or script code.");
			}

			SessionState.PSVariable.Set($"function:{ScriptBlockFunction}", _script);
			var functionInfo = (FunctionInfo)SessionState.InvokeCommand.GetCommand(ScriptBlockFunction, CommandTypes.Function);
			_scriptParameters = functionInfo.Parameters;
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
				else if (nameOrData is Hashtable data)
				{
					foreach (DictionaryEntry kv in data)
						_data[kv.Key] = kv.Value;
				}
				else
				{
					throw new PSArgumentNullException($"Invalid Data item type: {nameOrData?.GetType()}. Expected String or Hashtable.");
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

	static StartFarTaskCommand()
	{
		_iss = InitialSessionState.CreateDefault();

		_iss.Variables.Add([
			new("ErrorActionPreference", ActionPreference.Stop, string.Empty)
		]);

		_iss.Commands.Add([
			new SessionStateAliasEntry("job", NameInvokeTaskJob),
			new SessionStateAliasEntry("run", NameInvokeTaskRun),
			new SessionStateAliasEntry("ps:", NameInvokeTaskCmd),
			new SessionStateAliasEntry("keys", NameInvokeTaskKeys),
			new SessionStateAliasEntry("macro", NameInvokeTaskMacro),
			new SessionStateCmdletEntry(NameInvokeTaskJob, typeof(InvokeTaskJob), string.Empty),
			new SessionStateCmdletEntry(NameInvokeTaskCmd, typeof(InvokeTaskCmd), string.Empty),
			new SessionStateCmdletEntry(NameInvokeTaskRun, typeof(InvokeTaskRun), string.Empty),
			new SessionStateCmdletEntry(NameInvokeTaskKeys, typeof(InvokeTaskKeys), string.Empty),
			new SessionStateCmdletEntry(NameInvokeTaskMacro, typeof(InvokeTaskMacro), string.Empty),
		]);
	}

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

			if (s_paramInvalid.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
				throw new InvalidOperationException($"Task script cannot use parameters: {string.Join(", ", s_paramInvalid)}");

			_dynamicParameters.Add(p.Name, new RuntimeDefinedParameter(p.Name, p.ParameterType, p.Attributes));
		}
		return _dynamicParameters;
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
			}
		}

		// open new session
		var rs = RunspaceFactory.CreateRunspace(_iss);
		rs.Open();

		// $Data for scripts
		rs.SessionStateProxy.SetVariable(NameData, _data);

		// sync file system location
		rs.SessionStateProxy.Path.SetLocation(SessionState.Path.CurrentFileSystemLocation.Path);

		// PowerShell invoker
		var ps = PowerShell.Create(rs);

		// debugging
		if (Step || AddDebugger is { })
		{
			DebuggerKit.ValidateAvailable();

			// import breakpoints
			foreach (var bp in A.Psf.Runspace.Debugger.GetBreakpoints())
			{
				switch (bp)
				{
					case CommandBreakpoint cbp:
						rs.Debugger.SetCommandBreakpoint(cbp.Command, cbp.Action, cbp.Script);
						break;
					case LineBreakpoint lbp:
						rs.Debugger.SetLineBreakpoint(lbp.Script, lbp.Line, lbp.Column, lbp.Action);
						break;
					case VariableBreakpoint vbp:
						rs.Debugger.SetVariableBreakpoint(vbp.Variable, vbp.AccessMode, vbp.Action, vbp.Script);
						break;
				}
			}

			// load debugger assets
			DebuggerKit.AddDebugger(ps, AddDebugger);
			if (Step)
			{
				ps.AddScript(CodeStep, true).Invoke();
				ps.Commands.Clear();
			}
		}

		// add task script
		ps.AddScript(CodeTask, true).AddArgument(parameters).AddArgument(_script);

		// start
		var tcs = AsTask ? Tasks.CreateAsyncTaskCompletionSource<object[]>() : null;
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

file class BaseJob : PSCmdlet
{
	[Parameter(Position = 0, Mandatory = true)]
	public ScriptBlock Script
	{
		//! make unbound script
		set => _Script = ((ScriptBlockAst)value.Ast).GetScriptBlock();
		get => _Script;
	}
	ScriptBlock _Script = null!;

	protected Hashtable GetData() => (Hashtable)GetVariableValue(StartFarTaskCommand.NameData);

	protected VarDictionary GetVars() => new(SessionState.PSVariable);
}

file class InvokeTaskJob : BaseJob
{
	protected override void BeginProcessing()
	{
		// post the job as task
		var task = Tasks.Job(() =>
		{
			A.Psf.Engine.SessionState.PSVariable.Set(StartFarTaskCommand.NameData, GetData());
			A.Psf.Engine.SessionState.PSVariable.Set(StartFarTaskCommand.NameVar, GetVars());
			var output = Script.Invoke();
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
			if (result2 is { })
				WriteObject(result2);
		}
		else
		{
			foreach (var it in result)
				WriteObject(it);
		}
	}
}

file class InvokeTaskRun : BaseJob
{
	protected override void BeginProcessing()
	{
		// post the job as task
		var task = Tasks.Run(() =>
		{
			A.Psf.Engine.SessionState.PSVariable.Set(StartFarTaskCommand.NameData, GetData());
			A.Psf.Engine.SessionState.PSVariable.Set(StartFarTaskCommand.NameVar, GetVars());

			//!! 2025-05-18-1148 Test-CallStack.fas.ps1 -- nested pipeline issues if we use `Script.Invoke()` like `job`.
			using var ps = A.Psf.NewPowerShell();
			ps.AddScript(StartFarTaskCommand.CodeJob, false).AddArgument(Script);
			ps.Invoke();

			//! Assert-Far may stop by PipelineStoppedException
			if (ps.InvocationStateInfo.Reason is { })
				throw ps.InvocationStateInfo.Reason;
		});

		// await
		task.Await();
		FarNet.Works.Far2.Api.WaitSteps().Await();
	}
}

file class InvokeTaskCmd : BaseJob
{
	protected override void BeginProcessing()
	{
		// post the job as task
		var task = Tasks.Job(() =>
		{
			A.Psf.Engine.SessionState.PSVariable.Set(StartFarTaskCommand.NameData, GetData());
			A.Psf.Engine.SessionState.PSVariable.Set(StartFarTaskCommand.NameVar, GetVars());

			var args = new RunArgs(StartFarTaskCommand.CodeJob)
			{
				Writer = new ConsoleOutputWriter(),
				NoOutReason = true,
				UseLocalScope = false,
				Arguments = [Script]
			};
			A.Psf.Run(args);
			return args.Reason;
		});

		// await
		var reason = task.AwaitResult();
		FarNet.Works.Far2.Api.WaitSteps().Await();
		if (reason is { })
			throw reason;
	}
}

file class InvokeTaskKeys : PSCmdlet
{
	[Parameter(ValueFromRemainingArguments = true, Mandatory = true)]
	public string[] Keys { get; set; } = null!;

	protected override void BeginProcessing()
	{
		var keys = string.Join(" ", Keys);
		Tasks.Keys(keys).Await();
		FarNet.Works.Far2.Api.WaitSteps().Await();
	}
}

file class InvokeTaskMacro : PSCmdlet
{
	[Parameter(Position = 0, Mandatory = true)]
	public string Macro { get; set; } = null!;

	protected override void BeginProcessing()
	{
		Tasks.Macro(Macro).Await();
		FarNet.Works.Far2.Api.WaitSteps().Await();
	}
}
