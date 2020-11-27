
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Management.Automation;

namespace PowerShellFar.Commands
{
	sealed class InvokeFarStepperCommand : BaseCmdlet, IDisposable
	{
		[Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
		[Alias("PSPath", "FileName")]
		public string Path { get; set; }
		[Parameter()]
		public SwitchParameter Confirm { get; set; }
		readonly Stepper _stepper = new Stepper();
		public void Dispose()
		{
			_stepper.Dispose();
		}
		protected override void ProcessRecord()
		{
			_stepper.AddFile(Path);
		}
		protected override void EndProcessing()
		{
			_stepper.Ask = Confirm;
			_stepper.Go();
		}
	}
}
