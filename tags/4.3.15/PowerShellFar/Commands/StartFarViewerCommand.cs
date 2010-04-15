/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.ComponentModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Start-FarViewer command.
	/// Creates and opens a viewer.
	/// </summary>
	/// <seealso cref="NewFarViewerCommand"/>
	[Description("Creates and opens a viewer.")]
	public sealed class StartFarViewerCommand : NewFarViewerCommand
	{
		///
		[Parameter(HelpMessage = _helpModal)]
		public SwitchParameter Modal { get; set; }

		///
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
