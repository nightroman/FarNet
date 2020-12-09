
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System.Management.Automation;

namespace PowerShellFar.Commands
{
	sealed class InvokeFarStepperCommand : BaseCmdlet
	{
		readonly Stepper _stepper = new Stepper();

		[Parameter(Position = 0, Mandatory = true)]
		public string Path { get; set; }

		[Parameter]
		public SwitchParameter AsTask { get; set; }

		[Parameter]
		public SwitchParameter Confirm { get; set; }

		protected override void BeginProcessing()
		{
			Path = GetUnresolvedProviderPathFromPSPath(Path);
			_stepper.AddFile(Path);
			_stepper.Ask = Confirm;

			if (AsTask)
				WriteObject(_stepper.GoAsync());
			else
				_stepper.Go();
		}
	}
}
