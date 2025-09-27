using FarNet;
using System.Management.Automation;

namespace PowerShellFar.Commands;

internal sealed class InvokeTaskKeys : PSCmdlet
{
	internal const string MyName = "Invoke-FarTaskKeys";

	[Parameter(ValueFromRemainingArguments = true, Mandatory = true)]
	public string[] Keys { get; set; } = null!;

	protected override void BeginProcessing()
	{
		if (A.IsMainSession)
			throw new InvalidOperationException("Cannot run in main session.");

		var keys = string.Join(" ", Keys);
		Tasks.Keys(keys).Await();
		FarNet.Works.Far2.Api.WaitSteps().Await();
	}
}
