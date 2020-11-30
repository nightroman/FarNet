
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	[OutputType(typeof(Panel))]
	sealed class OutFarPanelCommand : BasePanelCmdlet
	{
		ObjectPanel _panel;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[Parameter(Position = 0)]
		public object[] Columns { get; set; }

		[Parameter(ValueFromPipeline = true)]
		public PSObject InputObject { get; set; }

		[Parameter]
		public string ExcludeMemberPattern { get; set; }

		[Parameter]
		public string HideMemberPattern { get; set; }

		[Parameter]
		public SwitchParameter Return { get; set; }

		protected override void BeginProcessing()
		{
			_panel = new ObjectPanel();

			// common parameters
			ApplyParameters(_panel);

			// more parameters
			_panel.Columns = Columns;
			_panel.ExcludeMemberPattern = ExcludeMemberPattern;
			_panel.HideMemberPattern = HideMemberPattern;

			// and title, if not yet
			if (string.IsNullOrEmpty(_panel.Title) && !string.IsNullOrEmpty(A.Psf._myCommand))
				_panel.Title = A.Psf._myCommand;
		}

		// Use collector to control count of finaly added to the panel.
		readonly List<object> _Collector = new List<object>();
		protected override void ProcessRecord()
		{
			//! Skip null. 'Out-FarPanel' should open a panel with no items.
			if (InputObject == null)
				return;

			// add object(s)
			if (InputObject.BaseObject is object[])
				_Collector.AddRange(InputObject.BaseObject as object[]);
			else
				_Collector.Add(InputObject);
		}
		protected override void EndProcessing()
		{
			if (_Collector.Count == 1)
				_panel.AddObject(_Collector[0]);
			else
				_panel.AddObjects(_Collector);

			if (Return)
				WriteObject(_panel);
			else
				_panel.OpenChild(null);
		}
	}
}
