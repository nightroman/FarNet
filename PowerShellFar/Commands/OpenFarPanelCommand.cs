
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.ComponentModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Open-FarPanel command.
	/// Opens the panel.
	/// </summary>
	/// <remarks>
	/// The panel is opened only when the core gets control.
	/// </remarks>
	[Description("Opens the panel.")]
	public sealed class OpenFarPanelCommand : BasePanelCmdlet
	{
		Panel _Panel;
		/// <summary>
		/// A panel or explorer to open or an object to show members.
		/// </summary>
		[Parameter(HelpMessage = "A panel or explorer to open or an object to show members.", Position = 0, Mandatory = true, ValueFromPipeline = true)]
		public PSObject InputObject { get; set; }
		/// <summary>
		/// Tells to open the panel as a child of the current.
		/// </summary>
		[Parameter(HelpMessage = "Tells to open the panel as a child of the current.")]
		public SwitchParameter AsChild
		{
			get { return _AsChild; }
			set { _AsChild = value; }
		}
		SwitchParameter _AsChild;
		///
		protected override void ProcessRecord()
		{
			// ignore empty or the rest of input
			if (InputObject == null || _Panel != null)
				return;
			
			// get the panel or a new member panel
			var explorer = InputObject.BaseObject as Explorer;
			if (explorer == null)
				_Panel = (InputObject.BaseObject as Panel) ?? new MemberPanel(new MemberExplorer(InputObject));
			else
				_Panel = explorer.CreatePanel();

			// setup and show
			ApplyParameters(_Panel);
			_Panel.Open(_AsChild);
		}
	}
}
