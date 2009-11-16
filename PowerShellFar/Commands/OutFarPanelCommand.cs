/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System.ComponentModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Out-FarPanel command.
	/// Sends output to a new or the current object panel.
	/// </summary>
	/// <seealso cref="NewFarObjectPanelCommand"/>
	/// <seealso cref="StartFarPanelCommand"/>
	[Description("Sends output to a new or the current object panel.")]
	public sealed class OutFarPanelCommand : BaseCmdlet
	{
		bool _isNewPanel;
		ObjectPanel _panel;

		/// <summary>
		/// Custom columns (names or special hashtables).
		/// </summary>
		/// <remarks>
		/// Use property names to specify columns or hashtables to describe columns in details,
		/// see <see cref="Meta"/> about hashtables and <see cref="PanelModeInfo.Columns"/> about column types.
		/// </remarks>
		[Parameter(HelpMessage = "Sets Columns property.", Position = 0)]
		public object[] Columns { get; set; }

		///
		[Parameter(HelpMessage = "Object(s) to be sent to a panel.", ValueFromPipeline = true)]
		public PSObject InputObject { get; set; }

		///
		[Parameter(HelpMessage = "Append objects to the active opened object panel, if any.")]
		public SwitchParameter Append { get; set; }

		/// <summary>
		/// See <see cref="FormatPanel.FarName"/>.
		/// </summary>
		[Parameter(HelpMessage = "Sets FarName property.")]
		public Meta FarName { get; set; }

		/// <summary>
		/// See <see cref="FormatPanel.ExcludeColumns"/>.
		/// </summary>
		[Parameter(HelpMessage = "Sets ExcludeColumns property.")]
		public string[] ExcludeColumns { get; set; }

		/// <summary>
		/// See <see cref="FormatPanel.AutoSize"/>.
		/// </summary>
		[Parameter(HelpMessage = "Sets AutoSize property.")]
		public SwitchParameter AutoSize { get; set; }

		/// <summary>
		/// Panel title.
		/// </summary>
		[Parameter(HelpMessage = "Panel title.")]
		public string Title { get; set; }

		///
		protected override void BeginProcessing()
		{
			if (Append)
				_panel = AnyPanel.GetPanel(true) as ObjectPanel;

			if (_panel == null)
			{
				_isNewPanel = true;
				_panel = new ObjectPanel();
				if (!string.IsNullOrEmpty(Title))
					_panel.Panel.Info.Title = Title;
				else if (!string.IsNullOrEmpty(A.Psf._myCommand))
					_panel.Panel.Info.Title = A.Psf._myCommand;

				_panel.Columns = Columns;
				_panel.FarName = FarName;
				_panel.ExcludeColumns = ExcludeColumns;
				_panel.AutoSize = AutoSize;
			}
		}

		///
		protected override void EndProcessing()
		{
			if (_isNewPanel)
				_panel.Show();
			else
				_panel.UpdateRedraw(true);
		}

		///
		protected override void ProcessRecord()
		{
			if (Stop())
				return;

			//! Skip null. 'Out-FarPanel' should open a panel with no items.
			if (InputObject == null)
				return;

			// add object(s)
			if (InputObject.BaseObject is object[])
				_panel.AddObjectsWorker(InputObject);
			else
				_panel.AddObject(InputObject);
		}
	}
}
