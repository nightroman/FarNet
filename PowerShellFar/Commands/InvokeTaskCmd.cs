using FarNet;

namespace PowerShellFar.Commands;

internal sealed class InvokeTaskCmd : BaseTaskCmdlet
{
	internal const string MyName = "Invoke-FarTaskCmd";

	private Exception? InvokeScript()
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
	}

	protected override void BeginProcessing()
	{
		// do sync
		if (A.IsMainSession)
		{
			var ex1 = InvokeScript();
			if (ex1 is { })
				throw ex1;
			return;
		}

		// post the job as task
		var task = Tasks.Job(InvokeScript);

		// await
		var ex2 = task.AwaitResult();
		FarNet.Works.Far2.Api.WaitSteps().Await();
		if (ex2 is { })
			throw ex2;
	}
}
