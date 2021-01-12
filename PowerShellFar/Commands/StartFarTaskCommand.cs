
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
		readonly Hashtable _data = new Hashtable(StringComparer.OrdinalIgnoreCase);

		// task script
		ScriptBlock _script;
		Exception _scriptError;
		Dictionary<string, ParameterMetadata> _scriptParameters;
		RuntimeDefinedParameterDictionary _paramDynamic;
		static readonly string[] _paramExclude = new string[] {
			"Verbose", "Debug", "ErrorAction", "WarningAction", "ErrorVariable", "WarningVariable",
			"OutVariable", "OutBuffer", "PipelineVariable", "InformationAction", "InformationVariable" };
		static readonly string[] _paramInvalid = new string[] {
			nameof(Script), nameof(Data), nameof(AsTask), nameof(Confirm) };

		// invokes task scripts in the new session
		const string _codeTask = @"
param($Script, $Data, $Parameters)
. $Script.GetNewClosure() @Parameters
";

		// invokes job scripts in the main session
		const string _codeJob = @"
param($Script, $Data, $Arguments)
. $Script.GetNewClosure() @Arguments
";

		[Parameter(Position = 0, Mandatory = true)]
		public object Script
		{
			set
			{
				if (value is PSObject ps)
					value = ps.BaseObject;

				if (value is ScriptBlock block)
				{
					_script = block;
					return;
				}

				if (!(value is string text))
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

		[Parameter]
		public SwitchParameter Confirm { get; set; }

		bool ShowConfirm(string title, string text)
		{
			var args = new MessageArgs()
			{
				Text = text,
				Caption = title,
				Options = MessageOptions.LeftAligned,
				Buttons = new string[] { "Step", "Continue", "Cancel" },
				Position = new Point(int.MaxValue, 1)
			};
			switch (Far.Api.Message(args))
			{
				case 0:
					return true;
				case 1:
					Confirm = false;
					return true;
				default:
					return false;
			}
		}

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
				try
				{
					// post the job as task
					var task = Tasks.Job(() =>
					{
						if (Self.Confirm)
						{
							if (!Self.ShowConfirm("job", $"{Path.GetFileName(Script.File)}\r\n{Script}"))
								throw new PipelineStoppedException();
						}

						// invoke script block in the main session
						var ps = A.Psf.NewPowerShell();
						ps.AddScript(_codeJob, true).AddArgument(Script).AddArgument(Self._data).AddArgument(Arguments);
						var output = ps.Invoke();

						//! Assert-Far may stop by PipelineStoppedException
						if (ps.InvocationStateInfo.Reason != null)
							throw ps.InvocationStateInfo.Reason;

						return output;
					});

					// await
					var result = task.Result;
					FarNet.Works.Far2.Api.WaitSteps().Wait();

					//! if the job returns a task, await and return
					if (result.Count == 1 && result[0] != null && result[0].BaseObject is Task task2)
					{
						task2.Wait();

						var pi = task2.GetType().GetProperty("Result");
						if (pi == null)
							return;

						var result2 = pi.GetValue(task2);
						if (result2 != null)
							WriteObject(result2);
					}
					else
					{
						foreach (var it in result)
							WriteObject(it);
					}
				}
				catch (Exception exn)
				{
					throw FarNet.Works.Kit.UnwrapAggregateException(exn);
				}
			}
		}

		// ps: {...}
		public class InvokeTaskCmd : BaseJob
		{
			protected override void BeginProcessing()
			{
				base.BeginProcessing();
				try
				{
					Exception reason = null;

					// post the job as task
					var task = Tasks.Job(() =>
					{
						if (Self.Confirm)
						{
							if (!Self.ShowConfirm("ps:", $"{Path.GetFileName(Script.File)}\r\n{Script}"))
								throw new PipelineStoppedException();
						}

						var args = new RunArgs(_codeJob)
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
					task.Wait();
					FarNet.Works.Far2.Api.WaitSteps().Wait();
					if (reason != null)
						throw reason;
				}
				catch (Exception exn)
				{
					throw FarNet.Works.Kit.UnwrapAggregateException(exn);
				}
			}
		}

		// run {...}
		public class InvokeTaskRun : BaseJob
		{
			protected override void BeginProcessing()
			{
				base.BeginProcessing();

				//! show before Tasks.Run or it completes on this dialog
				if (Self.Confirm)
				{
					var confirm = Tasks.Job(() => Self.ShowConfirm("run", $"{Script.File}\r\n{Script}"));
					if (!confirm.Result)
						throw new PipelineStoppedException();
				}

				try
				{
					// post the job as task
					var task = Tasks.Run(() =>
					{
						var ps = A.Psf.NewPowerShell();
						ps.AddScript(_codeJob, true).AddArgument(Script).AddArgument(Self._data).AddArgument(Arguments);
						ps.Invoke();

						//! Assert-Far may stop by PipelineStoppedException
						if (ps.InvocationStateInfo.Reason != null)
							throw ps.InvocationStateInfo.Reason;
					});

					// await
					task.Wait();
					FarNet.Works.Far2.Api.WaitSteps().Wait();
				}
				catch (Exception exn)
				{
					throw FarNet.Works.Kit.UnwrapAggregateException(exn);
				}
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
				if (Self.Confirm)
				{
					var confirm = Tasks.Job(() => Self.ShowConfirm("keys", keys));
					if (!confirm.Result)
						throw new PipelineStoppedException();
				}

				Tasks.Keys(keys).Wait();
				FarNet.Works.Far2.Api.WaitSteps().Wait();
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

				if (Self.Confirm)
				{
					var confirm = Tasks.Job(() => Self.ShowConfirm("macro", Macro));
					if (!confirm.Result)
						throw new PipelineStoppedException();
				}

				Tasks.Macro(Macro).Wait();
				FarNet.Works.Far2.Api.WaitSteps().Wait();
			}
		}

		static StartFarTaskCommand()
		{
			_iss = InitialSessionState.CreateDefault();

			// add variables
			_iss.Variables.Add(new SessionStateVariableEntry[] {
				new SessionStateVariableEntry("LogEngineLifeCycleEvent", false, string.Empty),
				new SessionStateVariableEntry("LogProviderLifeCycleEvent", false, string.Empty),
			});

			// add commands
			_iss.Commands.Add(new SessionStateCommandEntry[] {
				new SessionStateAliasEntry("job", "Invoke-TaskJob"),
				new SessionStateAliasEntry("ps:", "Invoke-TaskCmd"),
				new SessionStateAliasEntry("run", "Invoke-TaskRun"),
				new SessionStateAliasEntry("keys", "Invoke-TaskKeys"),
				new SessionStateAliasEntry("macro", "Invoke-TaskMacro"),
				new SessionStateCmdletEntry("Invoke-TaskJob", typeof(InvokeTaskJob), string.Empty),
				new SessionStateCmdletEntry("Invoke-TaskCmd", typeof(InvokeTaskCmd), string.Empty),
				new SessionStateCmdletEntry("Invoke-TaskRun", typeof(InvokeTaskRun), string.Empty),
				new SessionStateCmdletEntry("Invoke-TaskKeys", typeof(InvokeTaskKeys), string.Empty),
				new SessionStateCmdletEntry("Invoke-TaskMacro", typeof(InvokeTaskMacro), string.Empty),
			});
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
			ps.AddScript(_codeTask).AddArgument(_script).AddArgument(_data).AddArgument(parameters);

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
							task.SetResult(A.UnwrapPSObject(result));
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
