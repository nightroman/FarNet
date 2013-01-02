
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2001 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
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
		public SwitchParameter Append { get; set; }
		protected override void BeginProcessing()
		{
			if (Append)
			{
				var panel = Far.Api.Panel as Panel;
				if (panel == null || panel.GetType() != typeof(ObjectPanel))
					throw new InvalidOperationException("There is no panel able to append objects.");

				_panel = (ObjectPanel)panel;
			}
			else
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
		}
		// Use collector to control count of finaly added to the panel.
		List<object> _Collector = new List<object>();
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

			if (Append)
				_panel.UpdateRedraw(true);
			else
				_panel.OpenChild(null);
		}
	}
}
