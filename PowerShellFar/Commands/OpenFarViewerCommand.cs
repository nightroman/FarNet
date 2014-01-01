
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	sealed class OpenFarViewerCommand : NewFarViewerCommand
	{
		[Parameter]
		public SwitchParameter Modal { get; set; }
		protected override void ProcessRecord()
		{
			IViewer viewer = CreateViewer();
			if (Modal)
				viewer.Open(OpenMode.Modal);
			else
				viewer.Open();
		}
	}
}
