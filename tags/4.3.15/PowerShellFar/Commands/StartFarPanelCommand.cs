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
	/// Start-FarPanel command.
	/// Opens the panel.
	/// </summary>
	/// <remarks>
	/// The panel is opened only when Far gets control.
	/// Use <see cref="Stepper"/> for more complex scenarios.
	/// </remarks>
	[Description("Opens the panel.")]
	public sealed class StartFarPanelCommand : BasePanelCmdlet
	{
		///
		[Parameter(HelpMessage = "Panel object or any object which members to be shown.", Position = 0, Mandatory = true, ValueFromPipeline = true)]
		public PSObject InputObject { get; set; }

		///
		[Parameter(HelpMessage = "Start the panel as child of the current panel.")]
		public SwitchParameter AsChild
		{
			get { return _AsChild; }
			set { _AsChild = value; }
		}
		SwitchParameter _AsChild;

		bool _done;
		///
		protected override void ProcessRecord()
		{
			// done or noop?
			if (_done || InputObject == null)
				return;

			// done
			_done = true;

			// what panel?
			AnyPanel anyPanel = InputObject.BaseObject as AnyPanel;
			if (anyPanel == null)
			{
				// net panel?
				IPanel netPanel = InputObject.BaseObject as IPanel;
				if (netPanel != null)
				{
					ApplyParameters(netPanel);
					netPanel.Open();
					return;
				}
				
				// member panel
				anyPanel = new MemberPanel(InputObject);
			}

			// setup and show
			ApplyParameters(anyPanel.Panel);
			anyPanel.Show(_AsChild);
		}
	}
}
