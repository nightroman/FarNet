
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
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
	class BasePanelCmdlet : BaseCmdlet
	{
		[Parameter()]
		public Guid TypeId { get { return _TypeId.GetValueOrDefault(); } set { _TypeId = value; } }
		Guid? _TypeId;
		[Parameter()]
		public string Title { get; set; }
		[Parameter()]
		public PanelSortMode SortMode { get { return _SortMode.GetValueOrDefault(); } set { _SortMode = value; } }
		PanelSortMode? _SortMode;
		[Parameter()]
		public PanelViewMode ViewMode { get { return _ViewMode.GetValueOrDefault(); } set { _ViewMode = value; } }
		PanelViewMode? _ViewMode;
		[Parameter()]
		public SwitchParameter IdleUpdate { get { return _IdleUpdate.GetValueOrDefault(); } set { _IdleUpdate = value; } }
		SwitchParameter? _IdleUpdate;
		[Parameter()]
		public Meta DataId { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		[Parameter()]
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
