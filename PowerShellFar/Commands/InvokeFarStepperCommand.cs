
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System.Management.Automation;

namespace PowerShellFar.Commands
{
	sealed class InvokeFarStepperCommand : BaseCmdlet
	{
		[Parameter(Position = 0, Mandatory = true)]
		public string Path { get; set; }

		[Parameter]
		public SwitchParameter AsTask { get; set; }

		[Parameter]
		public SwitchParameter Confirm { get; set; }

		protected override void BeginProcessing()
		{
			Path = GetUnresolvedProviderPathFromPSPath(Path);
			var args = new Stepper.Args(Path) { Confirm = Confirm };
			if (AsTask)
				WriteObject(Stepper.RunAsync(args));
			else
				Stepper.Run(args);
		}
	}
}
