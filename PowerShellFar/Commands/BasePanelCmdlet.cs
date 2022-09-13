
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections;
using System.Management.Automation;

namespace PowerShellFar.Commands;

/// <summary>
/// Common panel parameters.
/// </summary>
//! Do not use default parameter values (not set parameters),
//! override existing panel properties only with set values.
class BasePanelCmdlet : BaseCmdlet
{
	[Parameter]
	public Guid TypeId { set { _TypeId = value; } }
	Guid? _TypeId;

	[Parameter]
	public string? Title { get; set; }

	[Parameter]
	public PanelSortMode SortMode { set { _SortMode = value; } }
	PanelSortMode? _SortMode;

	[Parameter]
	public PanelViewMode ViewMode { set { _ViewMode = value; } }
	PanelViewMode? _ViewMode;

	[Parameter]
	public int TimerUpdate { set { _TimerUpdate = value; } }
	int? _TimerUpdate;

	[Parameter]
	public Meta? DataId { get; set; }

	[Parameter]
	public IDictionary? Data { get; set; }

	internal void ApplyParameters(Panel panel)
	{
		// panel
		if (_TimerUpdate.HasValue && _TimerUpdate.Value > 0)
		{
			panel.IsTimerUpdate = true;
			panel.TimerInterval = _TimerUpdate.Value;
		}
		if (DataId != null)
			panel.Explorer.FileComparer = new FileMetaComparer(DataId);
		if (Data != null)
			foreach (DictionaryEntry kv in Data)
				panel.Data.Add(kv.Key, kv.Value);

		// info
		if (_SortMode.HasValue)
			panel.SortMode = _SortMode.Value;
		if (_TypeId.HasValue)
			panel.TypeId = _TypeId.Value;
		if (_ViewMode.HasValue)
			panel.ViewMode = _ViewMode.Value;
		if (Title != null)
			panel.Title = Title;
	}
}
