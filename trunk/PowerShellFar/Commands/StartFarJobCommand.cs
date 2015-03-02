
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System.Management.Automation;

namespace PowerShellFar.Commands
{
	sealed class StartFarJobCommand : BaseCmdlet
	{
		public StartFarJobCommand()
		{
			KeepSeconds = int.MaxValue;
		}
		[Parameter(Position = 0, Mandatory = true)]
		public JobCommand Command { get; set; }
		[Parameter(Position = 1)]
		public PSObject Parameters { get; set; }
		[Parameter]
		public string Name { get; set; }
		[Parameter]
		public SwitchParameter Output { get; set; }
		[Parameter]
		public SwitchParameter Return { get; set; }
		[Parameter]
		public SwitchParameter Hidden { get; set; }
		[Parameter]
		public int KeepSeconds { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		protected override void BeginProcessing()
		{
			if (Hidden)
			{
				Output = Return = false;
				KeepSeconds = 0;
			}
			else if (Return)
			{
				Output = true;
				KeepSeconds = int.MaxValue;
			}
			else if (Output)
			{
				KeepSeconds = int.MaxValue;
			}

			// new
			Job job = new Job(
				Command,
				Parameters == null ? null : Parameters.BaseObject,
				Name,
				!Hidden && !Output,
				KeepSeconds);

			// start
			if (!Return)
				job.StartJob();

			// write
			if (Output)
				WriteObject(job);
		}
	}
}
