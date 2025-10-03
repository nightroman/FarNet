using FarNet;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace PowerShellFar.Commands;

[OutputType(nameof(PSObject))]
internal sealed class InvokeTaskJob : BaseTaskCmdlet
{
	internal const string MyName = "Invoke-FarTaskJob";

	private Collection<PSObject> InvokeScript()
	{
		A.Engine.SessionState.PSVariable.Set(StartFarTaskCommand.NameData, GetData());
		A.Engine.SessionState.PSVariable.Set(StartFarTaskCommand.NameVar, GetVars());
		return Script.Invoke();
	}

	protected override void BeginProcessing()
	{
		// do sync
		if (A.IsMainSession)
		{
			WriteObject(InvokeScript(), true);
			return;
		}

		// post the job as task
		var task = Tasks.Job(InvokeScript);

		// await
		var result = task.AwaitResult();
		FarNet.Works.Far2.Api.WaitSteps().Await();

		// await tasks, return results
		foreach (var pso in result)
		{
			if (pso?.BaseObject is Task task2)
			{
				task2.Await();
				var taskType = task2.GetType();
				var result2 = taskType.GetProperty("Result")?.GetValue(task2);
				if (result2 is { } && result2.GetType().Name != "VoidTaskResult")
					WriteObject(result2);
			}
			else
			{
				WriteObject(pso);
			}
		}
	}
}
