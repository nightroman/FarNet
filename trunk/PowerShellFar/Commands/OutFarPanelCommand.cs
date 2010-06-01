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
	/// Out-FarPanel command.
	/// Sends output to a new or appends to the active object panel.
	/// </summary>
	[Description("Sends output to a new or appends to the active object panel.")]
	public sealed class OutFarPanelCommand : BasePanelCmdlet
	{
		ObjectPanel _panel;

		/// <summary>
		/// Custom columns (names or special hashtables).
		/// </summary>
		/// <remarks>
		/// Use property names to specify columns or hashtables to describe columns in details,
		/// see <see cref="Meta"/> about hashtables and <see cref="PanelModeInfo.Columns"/> about column types.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[Parameter(HelpMessage = "Sets Columns property.", Position = 0)]
		public object[] Columns { get; set; }

		///
		[Parameter(HelpMessage = "Object(s) to be sent to an object panel.", ValueFromPipeline = true)]
		public PSObject InputObject { get; set; }

		///
		[Parameter(HelpMessage = "Regular expression pattern of members to be excluded in a child list panel.")]
		public string ExcludeMemberPattern { get; set; }

		///
		[Parameter(HelpMessage = "Regular expression pattern of members to be hidden in a child list panel.")]
		public string HideMemberPattern { get; set; }

		///
		[Parameter(HelpMessage = "Tells to append objects to the active object panel. All others options are ignored.")]
		public SwitchParameter Append { get; set; }

		///
		protected override void BeginProcessing()
		{
			if (Append)
			{
				AnyPanel panel = AnyPanel.GetPanel(true);
				if (panel == null || panel.GetType() != typeof(ObjectPanel))
					throw new InvalidOperationException("There is no panel able to append objects.");

				_panel = (ObjectPanel)panel;
			}
			else
			{
				_panel = new ObjectPanel();

				// common parameters
				ApplyParameters(_panel.Panel);

				// more parameters
				_panel.Columns = Columns;
				_panel.ExcludeMemberPattern = ExcludeMemberPattern;
				_panel.HideMemberPattern = HideMemberPattern;

				// and title, if not yet
				if (string.IsNullOrEmpty(_panel.Panel.Info.Title) && !string.IsNullOrEmpty(A.Psf._myCommand))
					_panel.Panel.Info.Title = A.Psf._myCommand;
			}
		}

		///
		protected override void EndProcessing()
		{
			if (Append)
				_panel.UpdateRedraw(true);
			else
				_panel.Show();
		}

		///
		protected override void ProcessRecord()
		{
			//! Skip null. 'Out-FarPanel' should open a panel with no items.
			if (InputObject == null)
				return;

			// add object(s)
			if (InputObject.BaseObject is object[]) //???
				_panel.AddObjects(InputObject);
			else
				_panel.AddObject(InputObject);
		}
	}
}
