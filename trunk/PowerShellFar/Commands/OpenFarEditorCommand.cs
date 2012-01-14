
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	sealed class OpenFarEditorCommand : NewFarEditorCommand
	{
		[Parameter]
		public SwitchParameter Modal { get; set; }
		protected override void ProcessRecord()
		{
			IEditor editor = CreateEditor();
			if (Modal)
				editor.Open(OpenMode.Modal);
			else
				editor.Open();
		}
	}
}
