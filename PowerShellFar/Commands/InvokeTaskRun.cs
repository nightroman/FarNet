using FarNet;

namespace PowerShellFar.Commands;

internal sealed class InvokeTaskRun : BaseTaskCmdlet
{
	internal const string MyName = "Invoke-FarTaskRun";

	protected override void BeginProcessing()
	{
		if (A.IsMainSession)
			throw new InvalidOperationException("Cannot run in main session.");

		// post the job as task
		var task = Tasks.Run(() =>
		{
			A.Engine.SessionState.PSVariable.Set(StartFarTaskCommand.NameData, GetData());
			A.Engine.SessionState.PSVariable.Set(StartFarTaskCommand.NameVar, GetVars());

			//!! 2025-05-18-1148 Test-CallStack.fas.ps1 -- nested pipeline issues if we use `Script.Invoke()` like `job`.
			using var ps = A.NewPowerShell();
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
