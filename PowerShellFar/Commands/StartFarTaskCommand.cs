
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
		// state exposed as $Data to async script and sync jobs
		readonly Hashtable _data = new Hashtable(StringComparer.OrdinalIgnoreCase);
		ScriptBlock _script;
		Dictionary<string, ParameterMetadata> _scriptParameters;
		Exception _scriptError;

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

				string path;
				if (text.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase) && File.Exists(path = GetUnresolvedProviderPathFromPSPath(text)))
				{
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
		public SwitchParameter AsTask { get; set; }

		[Parameter]
		public SwitchParameter Confirm { get; set; }

		const string _codeTask = @"
param(
	$Script,
	$Data,
	$Parameters
)

$ErrorActionPreference = 'Stop'

function Invoke-FarJob($Job) {
	$result = $StartFarTaskCommand.Job($Job)
	if ($result -is [System.Exception]) {
		throw $result
	}
	$result
}

function Invoke-FarKeys($Keys) {
	$StartFarTaskCommand.Keys($Keys)
}

function Invoke-FarMacro($Keys) {
	$StartFarTaskCommand.Macro($Keys)
}

Set-Alias job Invoke-FarJob
Set-Alias keys Invoke-FarKeys
Set-Alias macro Invoke-FarMacro

. $Script.GetNewClosure() @Parameters
";

		const string _codeJob = @"
param(
	$Script,
	$Data
)

$ErrorActionPreference = 'Stop'

. $Script.GetNewClosure()
";

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

		// Called by task scripts.
		//! Catch and return an exception to avoid noise CmdletInvocationException\MethodInvocationException\<ActualException>.
		//! The task script checks for an exception and throws it.
		public object Job(ScriptBlock job)
		{
			try
			{
				// post the job as task
				var task = Tasks.Job(() =>
				{
					if (Confirm)
					{
						if (!ShowConfirm("Job", $"{job.File}\r\n{job.ToString()}"))
							throw new PipelineStoppedException();
					}

					// invoke live script block syncronously in the main session
					var ps = PowerShell.Create();
					ps.Runspace = A.Psf.Runspace;
					ps.AddScript(_codeJob, true).AddArgument(job).AddArgument(_data);
					var output = ps.Invoke();

					//! Assert-Far may throw special PipelineStoppedException, propagate it
					if (ps.InvocationStateInfo.Reason != null)
						throw ps.InvocationStateInfo.Reason;

					return output;
				});

				// await
				task.Wait();
				var result = task.Result;

				//! if the job returns a task, await and return, replacing null with an empty array
				if (result.Count == 1 && result[0] != null && result[0].BaseObject is Task task2)
				{
					task2.Wait();

					var pi = task2.GetType().GetProperty("Result");
					if (pi == null)
						return new object[] { };

					var result2 = pi.GetValue(task2);
					return result2 ?? new object[] { };
				}
				else
				{
					return result;
				}
			}
			catch (Exception exn)
			{
				return UnwrapAggregateException(exn);
			}
		}

		// Called by task scripts.
		//! Confirm has issues, macros fails.
		public void Keys(string keys)
		{
			var task = Tasks.Keys(keys);
			task.Wait();
		}

		// Called by task scripts.
		//! Confirm has issues, macros fails.
		public void Macro(string macro)
		{
			var task = Tasks.Macro(macro);
			task.Wait();
		}

		static Exception UnwrapAggregateException(Exception exn)
		{
			if (exn is AggregateException aggregate && aggregate.InnerExceptions.Count == 1)
				return aggregate.InnerExceptions[0];
			else
				return exn;
		}

		static void ShowError(Exception exn)
		{
			exn = UnwrapAggregateException(exn);
			Far.Api.ShowError("FarTask error", exn);
		}

		static readonly string[] _paramExclude = new string[] {
			"Verbose", "Debug", "ErrorAction", "WarningAction", "ErrorVariable", "WarningVariable",
			"OutVariable", "OutBuffer", "PipelineVariable", "InformationAction", "InformationVariable" };
		static readonly string[] _paramInvalid = new string[] {
			"Script", "AsTask", "Confirm" };
		RuntimeDefinedParameterDictionary _paramDynamic;

		public object GetDynamicParameters()
		{
			//! throw later, avoid bad error info
			if (_scriptError != null)
				return null;

			// get yet missing script block parameters
			if (_scriptParameters == null)
			{
				var res = A.InvokeCode("$function:__GetScriptParameters = $args[0]; Get-Command __GetScriptParameters -Type Function", _script);
				var info = (CommandInfo)res[0].BaseObject;
				_scriptParameters = info.Parameters;
			}

			_paramDynamic = new RuntimeDefinedParameterDictionary();
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

		protected override void BeginProcessing()
		{
			if (_scriptError != null)
				throw new PSArgumentException($"Script error: {_scriptError.Message}", nameof(Script));

			var iss = InitialSessionState.CreateDefault();

			// add variables
			iss.Variables.Add(new SessionStateVariableEntry("StartFarTaskCommand", this, string.Empty));
			iss.Variables.Add(new SessionStateVariableEntry("LogEngineLifeCycleEvent", false, string.Empty)); // whole log disabled
			iss.Variables.Add(new SessionStateVariableEntry("LogProviderLifeCycleEvent", false, string.Empty)); // start is still logged

			var rs = RunspaceFactory.CreateRunspace(iss);
			rs.Open();

			// parameters
			var parameters = new Hashtable();
			foreach (var p in _paramDynamic.Values)
				if (p.IsSet)
					parameters[p.Name] = p.Value;

			// make live script block to invoke asyncronously in the new session
			IAsyncResult asyncResult = null;
			var ps = PowerShell.Create();
			ps.Runspace = rs;
			ps.AddScript(_codeTask).AddArgument(_script).AddArgument(_data).AddArgument(parameters);

			// future completer
			var tcs = AsTask ? new TaskCompletionSource<object[]>() : null;
			ps.InvocationStateChanged += (object sender, PSInvocationStateChangedEventArgs e) =>
			{
				switch (e.InvocationStateInfo.State)
				{
					case PSInvocationState.Completed:
						//! post, to call EndInvoke from the same thread
						Far.Api.PostJob(() =>
						{
							try
							{
								var result = ps.EndInvoke(asyncResult);
								if (AsTask)
									tcs.SetResult(A.UnwrapPSObject(result));
							}
							catch (Exception exn)
							{
								if (AsTask)
								{
									tcs.SetException(UnwrapAggregateException(exn));
								}
								else
								{
									ShowError(exn);
								}
							}
							finally
							{
								done();
							}
						});
						break;

					case PSInvocationState.Failed:
						try
						{
							if (AsTask)
							{
								tcs.SetException(UnwrapAggregateException(e.InvocationStateInfo.Reason));
							}
							else
							{
								Far.Api.PostJob(() =>
								{
									ShowError(e.InvocationStateInfo.Reason);
								});
							}
						}
						finally
						{
							done();
						}
						break;

					default:
						break;
				}

				void done()
				{
					ps.Dispose();
					rs.Dispose();
				}
			};

			// start
			asyncResult = ps.BeginInvoke();
			if (AsTask)
				WriteObject(tcs.Task);
		}
	}
}
