
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
		/// Panel type Id. See <see cref="Panel.TypeId"/>
		/// </summary>
		[Parameter(HelpMessage = "Panel type Id.")]
		public Guid TypeId
		{
			get { return _TypeId.GetValueOrDefault(); }
			set { _TypeId = value; }
		}
		Guid? _TypeId;

		/// <summary>
		/// Panel title. See <see cref="Panel.Title"/>.
		/// </summary>
		[Parameter(HelpMessage = "Panel title.")]
		public string Title { get; set; }

		/// <summary>
		/// Panel sort mode. See <see cref="IPanel.SortMode"/>.
		/// </summary>
		[Parameter(HelpMessage = "Panel sort mode.")]
		public PanelSortMode SortMode
		{
			get { return _SortMode.GetValueOrDefault(); }
			set { _SortMode = value; }
		}
		PanelSortMode? _SortMode;

		/// <summary>
		/// Panel view mode. See <see cref="IPanel.ViewMode"/>.
		/// </summary>
		[Parameter(HelpMessage = "Panel view mode.")]
		public PanelViewMode ViewMode
		{
			get { return _ViewMode.GetValueOrDefault(); }
			set { _ViewMode = value; }
		}
		PanelViewMode? _ViewMode;

		/// <summary>
		/// Tells to update data periodically when idle. See <see cref="Panel.IdleUpdate"/>.
		/// </summary>
		[Parameter(HelpMessage = "Tells to update data periodically when idle.")]
		public SwitchParameter IdleUpdate
		{
			get { return _IdleUpdate.GetValueOrDefault(); }
			set { _IdleUpdate = value; }
		}
		SwitchParameter? _IdleUpdate;

		/// <summary>
		/// Custom data ID getter to distinguish files by data.
		/// </summary>
		[Parameter(HelpMessage = "Custom data ID to distinguish files by data.")]
		public Meta DataId { get; set; }

		/// <summary>
		/// Attached user data. See <see cref="Panel.Data"/>.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		[Parameter(HelpMessage = "Attached user data.")]
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
