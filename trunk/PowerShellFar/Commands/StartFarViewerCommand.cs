/*
PowerShellFar plugin for Far Manager
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
		public SwitchParameter Modal
		{
			get { return _Modal; }
			set { _Modal = value; }
		}
		SwitchParameter _Modal;

		///
		protected override void ProcessRecord()
		{
			if (Stop())
				return;
			IViewer viewer = CreateViewer();
			if (_Modal.IsPresent)
				viewer.Open(OpenMode.Modal);
			else
				viewer.Open();
		}
	}
}
