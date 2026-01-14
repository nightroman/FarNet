using FarNet;
using FarNet.Works;
using System.Management.Automation;

namespace PowerShellFar.Commands;

[OutputType(nameof(PSObject))]
internal sealed class InvokeTaskJob : BaseTaskCmdlet
{
	internal const string MyName = "Invoke-FarTaskJob";

	private void InvokeScript()
	{
		A.SetVariableValue(StartFarTaskCommand.NameData, GetData());
		A.SetVariableValue(StartFarTaskCommand.NameVar, GetVars());
		var r = Script.Invoke();
		if (r.Count > 0)
			throw new InvalidOperationException($"Unexpected `job` output: '{r[0]?.BaseObject.GetType().FullName}'.");
	}

	protected override void BeginProcessing()
	{
		// do sync
		if (A.IsMainSession)
		{
			InvokeScript();
			return;
		}

		// post the job as task
		var task = Far.Api.PostJobAsync(InvokeScript);

		// await
		task.Await();
		Far2.Api.WaitSteps().Await();
	}
}
