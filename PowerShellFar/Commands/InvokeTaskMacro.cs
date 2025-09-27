using FarNet;
using System.Management.Automation;

namespace PowerShellFar.Commands;

internal sealed class InvokeTaskMacro : PSCmdlet
{
	internal const string MyName = "Invoke-FarTaskMacro";

	[Parameter(Position = 0, Mandatory = true)]
	public string Macro { get; set; } = null!;

	protected override void BeginProcessing()
	{
		if (A.IsMainSession)
			throw new InvalidOperationException("Cannot run in main session.");

		Tasks.Macro(Macro).Await();
		FarNet.Works.Far2.Api.WaitSteps().Await();
	}
}
