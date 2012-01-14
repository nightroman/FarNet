
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Common panel parameters.
	/// </summary>
	class BasePanelCmdlet : BaseCmdlet
	{
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Parameter]
		public Guid TypeId { get { return _TypeId.GetValueOrDefault(); } set { _TypeId = value; } }
		Guid? _TypeId;
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Parameter]
		public string Title { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Parameter]
		public PanelSortMode SortMode { get { return _SortMode.GetValueOrDefault(); } set { _SortMode = value; } }
		PanelSortMode? _SortMode;
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Parameter]
		public PanelViewMode ViewMode { get { return _ViewMode.GetValueOrDefault(); } set { _ViewMode = value; } }
		PanelViewMode? _ViewMode;
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Parameter]
		public SwitchParameter IdleUpdate { get { return _IdleUpdate.GetValueOrDefault(); } set { _IdleUpdate = value; } }
		SwitchParameter? _IdleUpdate;
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Parameter]
		public Meta DataId { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		[Parameter]
		public IDictionary Data { get; set; }
		internal void ApplyParameters(Panel panel)
		{
			// panel
			if (DataId != null) panel.Explorer.FileComparer = new FileMetaComparer(DataId);
			if (_IdleUpdate.HasValue && _IdleUpdate.Value) panel.IdleUpdate = true;
			if (Data != null)
				foreach (DictionaryEntry kv in Data)
					panel.Data.Add(kv.Key, kv.Value);

			// info
			if (_SortMode.HasValue) panel.SortMode = _SortMode.Value;
			if (_TypeId.HasValue) panel.TypeId = _TypeId.Value;
			if (_ViewMode.HasValue) panel.ViewMode = _ViewMode.Value;
			if (Title != null) panel.Title = Title;
		}
	}
}
