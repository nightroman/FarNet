
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System.Management.Automation;

namespace PowerShellFar.Commands;

sealed class OpenFarViewerCommand : NewFarViewerCommand
{
	[Parameter]
	public SwitchParameter Modal { get; set; }

	protected override void ProcessRecord()
	{
		var viewer = CreateViewer();
		if (Modal)
			viewer.Open(OpenMode.Modal);
		else
			viewer.Open();
	}
}
