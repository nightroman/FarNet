
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;

namespace PowerShellFar.Commands
{
	[Cmdlet("Start", "FarTask", DefaultParameterSetName = "Script")]
	[OutputType(typeof(Task<object[]>))]
	sealed class StartFarTaskCommand : BaseCmdlet
	{
		// state exposed as $Data to async script and sync jobs
		readonly Hashtable _data = new Hashtable(StringComparer.OrdinalIgnoreCase);
		ScriptBlock _script;

		[Parameter(Position = 0, Mandatory = true, ParameterSetName = "Script")]
		public ScriptBlock Script { set { _script = value; } }

		[Parameter(Mandatory = true, ParameterSetName = "File")]
		public string File
		{
			set
			{
				var path = GetUnresolvedProviderPathFromPSPath(value);
				var info = (ExternalScriptInfo)SessionState.InvokeCommand.GetCommand(path, CommandTypes.ExternalScript);
				_script = info.ScriptBlock;
			}
		}

		[Parameter(Mandatory = true, ParameterSetName = "Code")]
		public string Code
		{
			set
			{
				_script = ScriptBlock.Create(value);
			}
		}

		[Parameter]
		public SwitchParameter AsTask { get; set; }

		const string _codeTask = @"
param(
	$Script,
	$Data
)

$ErrorActionPreference = 'Stop'

function Invoke-FarJob($Job) {
	$result = $StartFarTaskCommand.Job($Job)
	if ($result -is [System.Exception]) {
		throw $result
	}

	if ($result -is [System.Threading.Tasks.Task]) {
		$result.Wait()
		$result.Result
	}
	else {
		$result
	}
}

function Invoke-FarKeys($Keys) {
	$StartFarTaskCommand.Keys($Keys)
}

Set-Alias job Invoke-FarJob
Set-Alias keys Invoke-FarKeys

. $Script.GetNewClosure()
";

		const string _codeJob = @"
param(
	$Script,
	$Data
)

$ErrorActionPreference = 'Stop'

. $Script.GetNewClosure()
";

		// Called by task script.
		//! Return exceptions to avoid wrappers: CmdletInvocationException(MethodInvocationException).
		//! The calling script checks for an exception and throws it.
		public object Job(ScriptBlock job)
		{
			try
			{
				var task = Tasks.Job<object>(() =>
				{
					// invoke live script block syncronously in the main session
					var ps = PowerShell.Create();
					ps.Runspace = A.Psf.Runspace;
					ps.AddScript(_codeJob, true).AddArgument(job).AddArgument(_data);
					var jobResult = ps.Invoke();
					if (jobResult.Count == 1)
						return jobResult[0];
					else
						return jobResult;
				});

				task.Wait();
				return task.Result;
			}
			catch (Exception exn)
			{
				return UnwrapAggregateException(exn);
			}
		}

		// Called by task script.
		public void Keys(string keys)
		{
			var task = Tasks.Keys(keys);
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
			Far.Api.ShowError("Async error", exn);
		}

		protected override void BeginProcessing()
		{
			var iss = InitialSessionState.CreateDefault();

			// add variables
			iss.Variables.Add(new SessionStateVariableEntry("StartFarTaskCommand", this, string.Empty));
			iss.Variables.Add(new SessionStateVariableEntry("LogEngineLifeCycleEvent", false, string.Empty)); // whole log disabled
			iss.Variables.Add(new SessionStateVariableEntry("LogProviderLifeCycleEvent", false, string.Empty)); // start is still logged

			var rs = RunspaceFactory.CreateRunspace(iss);
			rs.Open();

			// make live script block to invoke asyncronously in the new session
			IAsyncResult asyncResult = null;
			var ps = PowerShell.Create();
			ps.Runspace = rs;
			ps.AddScript(_codeTask).AddArgument(_script).AddArgument(_data);

			// future completer
			var tcs = AsTask ? new TaskCompletionSource<object[]>() : null;
			ps.InvocationStateChanged += (object sender, PSInvocationStateChangedEventArgs e) =>
			{
				switch (e.InvocationStateInfo.State)
				{
					case PSInvocationState.Completed:
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
									tcs.SetException(exn);
								else
									ShowError(exn);
							}
							finally
							{
								done();
							}
						});
						break;

					case PSInvocationState.Failed:
						Far.Api.PostJob(() =>
						{
							if (AsTask)
								tcs.SetException(e.InvocationStateInfo.Reason);
							else
								ShowError(e.InvocationStateInfo.Reason);

							done();
						});
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
