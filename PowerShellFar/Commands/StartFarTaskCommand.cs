
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;

namespace PowerShellFar.Commands
{
	[OutputType(typeof(Task<object[]>))]
	sealed class StartFarTaskCommand : BaseCmdlet, IDynamicParameters
	{
		// tasks initial state
		static readonly InitialSessionState _iss;

		// $Data for scripts
		readonly Hashtable _data = new(StringComparer.OrdinalIgnoreCase);

		// task script
		ScriptBlock _script;
		Exception _scriptError;
		Dictionary<string, ParameterMetadata> _scriptParameters;
		RuntimeDefinedParameterDictionary _paramDynamic;
		static readonly string[] _paramExclude = new string[] {
			"Verbose", "Debug", "ErrorAction", "WarningAction", "ErrorVariable", "WarningVariable",
			"OutVariable", "OutBuffer", "PipelineVariable", "InformationAction", "InformationVariable" };
		static readonly string[] _paramInvalid = new string[] {
			nameof(Script), nameof(Data), nameof(AsTask), nameof(AddDebugger), nameof(Step) };

		// just adds the debugger
		const string CodeBreak = @"
param($Parameters)
Add-Debugger.ps1 @Parameters
";
		// adds debugger and step breaks
		const string CodeStep = @"
param($Parameters)
Add-Debugger.ps1 @Parameters
Set-PSBreakpoint -Command job, ps:, run, keys, macro
";

		// invokes task scripts in the new session
		const string CodeTask = @"
param($Script, $Data, $Parameters)
. $Script.GetNewClosure() @Parameters
";

		// invokes job scripts in the main session
		const string CodeJob = @"
param($Script, $Data, $Arguments)
. $Script.GetNewClosure() @Arguments
";

		[Parameter(Position = 0, Mandatory = true)]
		public object Script
		{
			set
			{
				value = PS2.BaseObject(value);

				if (value is ScriptBlock block)
				{
					_script = block;
					return;
				}

				if (value is not string text)
					throw new PSArgumentException("Invalid script type.");

				//! used to `&& File.Exists` -- bad, on missing file it is treated as code -- not clear errors
				if (text.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
				{
					var path = GetUnresolvedProviderPathFromPSPath(text);
					if (!File.Exists(path))
						throw new PSArgumentException($"Missing script file '{path}'.");

					try
					{
						var info = (ExternalScriptInfo)SessionState.InvokeCommand.GetCommand(path, CommandTypes.ExternalScript);
						//! throws on syntax errors
						_script = info.ScriptBlock;
						_scriptParameters = info.Parameters;
					}
					catch (Exception exn)
					{
						//! throw later, avoid bad error info
						_scriptError = exn;
					}
				}
				else
				{
					_script = ScriptBlock.Create(text);
				}
			}
		}

		[Parameter]
		[ValidateNotNull]
		public string[] Data
		{
			set
			{
				foreach (var name in value)
				{
					var variable = SessionState.PSVariable.Get(name);
					if (variable == null)
						throw new PSArgumentException($"Variable {name} is not found.");
					_data.Add(variable.Name, variable.Value);
				}
			}
		}

		[Parameter]
		public SwitchParameter AsTask { get; set; }

		[Parameter(ParameterSetName = "AddDebugger", Mandatory = true)]
		[ValidateNotNull]
		public IDictionary AddDebugger { get; set; }

		[Parameter(ParameterSetName = "AddDebugger")]
		public SwitchParameter Step { get; set; }

		static void ShowError(Exception exn)
		{
			Far.Api.ShowError("FarTask error", exn);
		}

		public object GetDynamicParameters()
		{
			//! throw later, avoid bad error info
			if (_scriptError != null)
				return null;

			_paramDynamic = new RuntimeDefinedParameterDictionary();
			if (_scriptParameters == null)
				return _paramDynamic;

			foreach (var p in _scriptParameters.Values)
			{
				if (!_paramExclude.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
				{
					if (_paramInvalid.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
						throw new InvalidOperationException($"Task script cannot use parameter: {string.Join(", ", _paramInvalid)}");

					_paramDynamic.Add(p.Name, new RuntimeDefinedParameter(p.Name, p.ParameterType, p.Attributes));
				}
			}
			return _paramDynamic;
		}

		// jobs and macros base
		public class BaseCommand : PSCmdlet
		{
			protected StartFarTaskCommand Self { get; private set; }

			protected override void BeginProcessing()
			{
				Self = (StartFarTaskCommand)GetVariableValue(nameof(StartFarTaskCommand));
			}
		}

		// jobs base
		public class BaseJob : BaseCommand
		{
			[Parameter(Position = 0)]
			public ScriptBlock Script { get; set; }

			[Parameter(Position = 1)]
			public object[] Arguments { get; set; }

			protected override void BeginProcessing()
			{
				base.BeginProcessing();
				if (Script == null)
					throw new PSArgumentNullException(nameof(Script));
			}
		}

		// job {...}
		public class InvokeTaskJob : BaseJob
		{
			protected override void BeginProcessing()
			{
				base.BeginProcessing();

				// post the job as task
				var task = Tasks.Job(() =>
				{
					// invoke script block in the main session
					var ps = A.Psf.NewPowerShell();

					ps.AddScript(CodeJob, true).AddArgument(Script).AddArgument(Self._data).AddArgument(Arguments);
					var output = ps.Invoke();

					//! Assert-Far may stop by PipelineStoppedException
					if (ps.InvocationStateInfo.Reason != null)
						throw ps.InvocationStateInfo.Reason;

					return output;
				});

				// await
				var result = task.GetAwaiter().GetResult();
				FarNet.Works.Far2.Api.WaitSteps().GetAwaiter().GetResult();

				//! if the job returns a task, await and return
				if (result.Count == 1 && result[0]?.BaseObject is Task task2)
				{
					task2.GetAwaiter().GetResult();

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
				base.BeginProcessing();

				Exception reason = null;

				// post the job as task
				var task = Tasks.Job(() =>
				{
					var args = new RunArgs(CodeJob)
					{
						Writer = new ConsoleOutputWriter(),
						NoOutReason = true,
						UseLocalScope = true,
						Arguments = new object[] { Script, Self._data, Arguments }
					};
					A.Psf.Run(args);
					reason = args.Reason;
				});

				// await
				task.GetAwaiter().GetResult();
				FarNet.Works.Far2.Api.WaitSteps().GetAwaiter().GetResult();
				if (reason != null)
					throw reason;
			}
		}

		// run {...}
		public class InvokeTaskRun : BaseJob
		{
			protected override void BeginProcessing()
			{
				base.BeginProcessing();

				// post the job as task
				var task = Tasks.Run(() =>
				{
					var ps = A.Psf.NewPowerShell();
					ps.AddScript(CodeJob, true).AddArgument(Script).AddArgument(Self._data).AddArgument(Arguments);
					ps.Invoke();

					//! Assert-Far may stop by PipelineStoppedException
					if (ps.InvocationStateInfo.Reason != null)
						throw ps.InvocationStateInfo.Reason;
				});

				// await
				task.GetAwaiter().GetResult();
				FarNet.Works.Far2.Api.WaitSteps().GetAwaiter().GetResult();
			}
		}

		// keys ...
		public class InvokeTaskKeys : BaseCommand
		{
			[Parameter(ValueFromRemainingArguments = true)]
			public string[] Keys { get; set; }

			protected override void BeginProcessing()
			{
				base.BeginProcessing();

				if (Keys == null || Keys.Length == 0)
					throw new PSArgumentNullException(nameof(Keys));

				var keys = string.Join(" ", Keys);

				Tasks.Keys(keys).GetAwaiter().GetResult();
				FarNet.Works.Far2.Api.WaitSteps().GetAwaiter().GetResult();
			}
		}

		// macro ...
		public class InvokeTaskMacro : BaseCommand
		{
			[Parameter(Position = 0)]
			public string Macro { get; set; }

			protected override void BeginProcessing()
			{
				base.BeginProcessing();

				if (Macro == null)
					throw new PSArgumentNullException(nameof(Macro));

				Tasks.Macro(Macro).GetAwaiter().GetResult();
				FarNet.Works.Far2.Api.WaitSteps().GetAwaiter().GetResult();
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

			// add commands
			_iss.Commands.Add(new SessionStateCommandEntry[] {
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
			});
		}

		void ValidateAddDebugger()
		{
			if (0 == A.InvokeCode("Get-Command Add-Debugger.ps1 -Type ExternalScript -ErrorAction 0").Count)
				throw new PSArgumentException(
					"Cannot find the required script Add-Debugger.ps1.\nInstall from PSGallery -- https://www.powershellgallery.com/packages/Add-Debugger");

			foreach (DictionaryEntry kv in AddDebugger)
				if (string.Equals("Path", kv.Key?.ToString(), StringComparison.OrdinalIgnoreCase))
					return;

			throw new PSArgumentException("AddDebugger parameters dictionary must contain 'Path'.");
		}

		protected override void BeginProcessing()
		{
			if (_scriptError != null)
				throw new PSArgumentException($"Script error: {_scriptError.Message}", nameof(Script));

			// open session and set extra variables
			var rs = RunspaceFactory.CreateRunspace(_iss);
			rs.Open();
			rs.SessionStateProxy.PSVariable.Set(nameof(StartFarTaskCommand), this);
			rs.SessionStateProxy.PSVariable.Set("ErrorActionPreference", ActionPreference.Stop);

			// parameters
			var parameters = new Hashtable();
			foreach (var p in _paramDynamic.Values)
				if (p.IsSet)
					parameters[p.Name] = p.Value;

			// make live script block to invoke asyncronously in the new session
			var ps = PowerShell.Create();
			ps.Runspace = rs;

			// debugging
			if (AddDebugger is not null)
			{
				ValidateAddDebugger();

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

				ps
					.AddScript(Step ? CodeStep : CodeBreak)
					.AddArgument(AddDebugger)
					.Invoke();

				ps.Commands.Clear();
			}

			// add task script
			ps.AddScript(CodeTask).AddArgument(_script).AddArgument(_data).AddArgument(parameters);

			// start
			var task = AsTask ? new TaskCompletionSource<object[]>() : null;
			ps.BeginInvoke<object>(null, null, asyncCallback, null);
			if (AsTask)
				WriteObject(task.Task);

			void done()
			{
				ps.Dispose();
				rs.Dispose();
			}

			void asyncCallback(IAsyncResult asyncResult)
			{
				var reason = ps.InvocationStateInfo.Reason;
				if (reason != null)
				{
					if (AsTask)
					{
						task.SetException(FarNet.Works.Kit.UnwrapAggregateException(reason));
					}
					else
					{
						Far.Api.PostJob(() =>
						{
							ShowError(reason);
						});
					}
					done();
					return;
				}

				//! post, to EndInvoke in the same thread
				Far.Api.PostJob(() =>
				{
					try
					{
						var result = ps.EndInvoke(asyncResult);
						if (AsTask)
							task.SetResult(PS2.UnwrapPSObject(result));
					}
					catch (Exception exn)
					{
						if (AsTask)
							task.SetException(FarNet.Works.Kit.UnwrapAggregateException(exn));
						else
							ShowError(exn);
					}
					finally
					{
						done();
					}
				});
			}
		}
	}
}
