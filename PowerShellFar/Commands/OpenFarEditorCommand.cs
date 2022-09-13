
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System.Management.Automation;

namespace PowerShellFar.Commands;

sealed class OpenFarEditorCommand : NewFarEditorCommand
{
	[Parameter]
	public SwitchParameter Modal { get; set; }

	protected override void ProcessRecord()
	{
		var editor = CreateEditor();
		if (Modal)
			editor.Open(OpenMode.Modal);
		else
			editor.Open();
	}
}
