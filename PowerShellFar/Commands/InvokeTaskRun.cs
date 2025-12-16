/*
	Why `run` in main

		Case: An Invoke-Build task is invoked from editor. This ends with modal
		"pause" and there is no more code to run. So, say, to open a panel, do
		`run { op }` in the task code.

		`Redraw()` once works around "pause" not rendered during `Wait`, odd.
*/

using FarNet;
using System.Management.Automation;

namespace PowerShellFar.Commands;

internal sealed class InvokeTaskRun : BaseTaskCmdlet
{
	internal const string MyName = "Invoke-FarTaskRun";

	protected override ScriptBlock ConvertScript(ScriptBlock script)
	{
		return A.IsMainSession ? script.GetNewClosure() : base.ConvertScript(script);
	}

	protected override void BeginProcessing()
	{
		if (A.IsMainSession)
		{
			bool redraw = true;

			_ = Tasks.Wait(100, 0, () =>
			{
				if (A.IsRunning)
					return false;

				if (redraw)
				{
					redraw = false;
					Far.Api.UI.Redraw();
				}

				if (Far.Api.Window.IsModal)
					return false;

				Far.Api.PostJob(() => Script.Invoke());
				return true;
			});
			return;
		}

		// post the job as task
		var task = Tasks.Run(() =>
		{
			A.SetVariableValue(StartFarTaskCommand.NameData, GetData());
			A.SetVariableValue(StartFarTaskCommand.NameVar, GetVars());

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
