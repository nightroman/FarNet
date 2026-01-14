using FarNet;
using FarNet.Works;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace PowerShellFar.Commands;

[OutputType(nameof(PSObject))]
internal sealed class InvokeTaskFun : BaseTaskCmdlet
{
	internal const string MyName = "Invoke-FarTaskFun";

	private Collection<PSObject> InvokeScript()
	{
		A.SetVariableValue(StartFarTaskCommand.NameData, GetData());
		A.SetVariableValue(StartFarTaskCommand.NameVar, GetVars());
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

		// await result
		var result = task.AwaitResult();
		if (result.Count == 0)
			throw new InvalidOperationException("Missing expected output from `fun`.");

		// wait steps
		Far2.Api.WaitSteps().Await();

		//! Do not optimize for exact `Task`, it may be task machine state type.
		//! Do not output returned nulls, it's messy on 2+, and avoid such cases.

		// await tasks, return results
		foreach (var pso in result)
		{
			if (pso?.BaseObject is Task task2)
			{
				task2.Await();
				var result2 = task2.TryProperty("Result");
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
