/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Common panel parameters.
	/// </summary>
	public class BasePanelCmdlet : BaseCmdlet
	{
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
		public PanelSortMode SortMode
		{
			get { return _SortMode; }
			set
			{
				_setSortMode = true;
				_SortMode = value;
			}
		}
		PanelSortMode _SortMode;
		bool _setSortMode;

		///
		[Parameter(HelpMessage = "Tells to reverse the sort order.")]
		public SwitchParameter ReverseSortOrder
		{
			get { return _ReverseSortOrder; }
			set
			{
				_setReverseSortOrder = true;
				_ReverseSortOrder = value;
			}
		}
		SwitchParameter _ReverseSortOrder;
		bool _setReverseSortOrder;

		///
		[Parameter(HelpMessage = "Panel view mode.")]
		public PanelViewMode ViewMode
		{
			get { return _ViewMode; }
			set
			{
				_setViewMode = true;
				_ViewMode = value;
			}
		}
		PanelViewMode _ViewMode;
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

		internal void ApplyParameters(IPanel panel)
		{
			// panel
			if (_setData) panel.Data = _Data;
			if (_setDataId) panel.DataId = _DataId;
			if (_setIdleUpdate) panel.IdleUpdate = _IdleUpdate;

			// info
			if (_setSortMode) panel.Info.StartSortMode = _SortMode;
			if (_setReverseSortOrder) panel.Info.StartReverseSortOrder = _ReverseSortOrder;
			if (_setTitle) panel.Info.Title = _Title;
			if (_setTypeId) panel.TypeId = _TypeId;
			if (_setViewMode) panel.Info.StartViewMode = _ViewMode;
		}
		
	}
}
