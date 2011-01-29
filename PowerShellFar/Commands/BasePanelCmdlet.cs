/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Common panel parameters.
	/// </summary>
	public class BasePanelCmdlet : BaseCmdlet
	{
		/// <summary>
		/// Panel type Id. See <see cref="IPanel.TypeId"/>
		/// </summary>
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

		/// <summary>
		/// Panel title. See <see cref="IPanelInfo.Title"/>.
		/// </summary>
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

		/// <summary>
		/// Panel sort mode. See <see cref="IAnyPanel.SortMode"/>.
		/// </summary>
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

		/// <summary>
		/// Tells to reverse the sort order. See <see cref="IAnyPanel.ReverseSortOrder"/>.
		/// </summary>
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

		/// <summary>
		/// Panel view mode. See <see cref="IAnyPanel.ViewMode"/>.
		/// </summary>
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

		/// <summary>
		/// Tells to update data periodically when idle. See <see cref="IPanel.IdleUpdate"/>.
		/// </summary>
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

		/// <summary>
		/// Custom data ID to distinguish between objects. See <see cref="IPanel.DataId"/>.
		/// </summary>
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

		/// <summary>
		/// Attached user data. See <see cref="IPanel.Data"/>.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		[Parameter(HelpMessage = "Attached user data.")]
		public IDictionary Data { get; set; }

		internal void ApplyParameters(IPanel panel)
		{
			// panel
			if (_setDataId) panel.DataId = _DataId;
			if (_setIdleUpdate) panel.IdleUpdate = _IdleUpdate;
			if (Data != null)
				foreach(DictionaryEntry kv in Data)
					panel.Data.Add(kv.Key, kv.Value);

			// info
			if (_setSortMode) panel.Info.StartSortMode = _SortMode;
			if (_setReverseSortOrder) panel.Info.StartReverseSortOrder = _ReverseSortOrder;
			if (_setTitle) panel.Info.Title = _Title;
			if (_setTypeId) panel.TypeId = _TypeId;
			if (_setViewMode) panel.Info.StartViewMode = _ViewMode;
		}

	}
}
