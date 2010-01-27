/*
PowerShellFar plugin for Far Manager
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
	/// Normally it should be the last script command.
	/// Use <see cref="Stepper"/> for more complex scenarios.
	/// </remarks>
	[Description("Opens the panel.")]
	public sealed class StartFarPanelCommand : BaseCmdlet
	{
		///
		[Parameter(HelpMessage = "Panel object or any object which members to be shown.", Position = 0, Mandatory = true, ValueFromPipeline = true)]
		public PSObject InputObject { get; set; }

		///
		[Parameter(HelpMessage = "Panel type Id.")]
		public Guid TypeId
		{
			get { return _TypeId; }
			set
			{
				_setTypeId = true;
				_TypeId = value;
			}
		}
		Guid _TypeId;
		bool _setTypeId;

		///
		[Parameter(HelpMessage = "Panel title.")]
		public string Title
		{
			get { return _Title; }
			set
			{
				_setTitle = true;
				_Title = value;
			}
		}
		string _Title;
		bool _setTitle;

		///
		[Parameter(HelpMessage = "Any user data attached to the panel.")]
		public PSObject Data
		{
			get { return _Data; }
			set
			{
				_setData = true;
				_Data = value;
			}
		}
		PSObject _Data;
		bool _setData;

		///
		[Parameter(HelpMessage = "Panel sort mode.")]
		public PanelSortMode OrderBy
		{
			get { return _OrderBy; }
			set
			{
				_setOrderBy = true;
				_OrderBy = value;
			}
		}
		PanelSortMode _OrderBy;
		bool _setOrderBy;

		///
		[Parameter(HelpMessage = "Sort in descending order.")]
		public SwitchParameter Descending
		{
			get { return _Descending; }
			set
			{
				_setDescending = true;
				_Descending = value;
			}
		}
		SwitchParameter _Descending;
		bool _setDescending;

		///
		[Parameter(HelpMessage = "Panel view mode.")]
		public PanelViewMode View
		{
			get { return _View; }
			set
			{
				_setViewMode = true;
				_View = value;
			}
		}
		PanelViewMode _View;
		bool _setViewMode;

		///
		[Parameter(HelpMessage = "Tells to update data periodically when idle.")]
		public SwitchParameter IdleUpdate
		{
			get { return _IdleUpdate; }
			set
			{
				_setIdleUpdate = true;
				_IdleUpdate = value;
			}
		}
		SwitchParameter _IdleUpdate;
		bool _setIdleUpdate;

		///
		[Parameter(HelpMessage = "Custom data ID to distinguish between objects.")]
		public Meta DataId
		{
			get { return _DataId; }
			set
			{
				_setDataId = true;
				_DataId = value;
			}
		}
		Meta _DataId;
		bool _setDataId;

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

			// object or panel?
			AnyPanel panel = InputObject.BaseObject as AnyPanel;
			if (panel == null)
				panel = new MemberPanel(InputObject);

			if (_setData) panel.Data = _Data;
			if (_setDataId) panel.Panel.DataId = _DataId;
			if (_setDescending) panel.Panel.Info.StartSortDesc = _Descending;
			if (_setTypeId) panel.Panel.TypeId = _TypeId;
			if (_setIdleUpdate) panel.Panel.IdleUpdate = _IdleUpdate;
			if (_setOrderBy) panel.Panel.Info.StartSortMode = _OrderBy;
			if (_setTitle) panel.Panel.Info.Title = _Title;
			if (_setViewMode) panel.Panel.Info.StartViewMode = _View;

			// go
			panel.Show(_AsChild);
		}
	}
}
