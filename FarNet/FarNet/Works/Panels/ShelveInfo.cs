
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FarNet.Works;
#pragma warning disable 1591

public abstract class ShelveInfo
{
	//! PSF test.
	public static Collection<ShelveInfo> Stack => new(_Stack);
	static readonly List<ShelveInfo> _Stack = [];

	//! PSF test.
	public string[]? GetSelectedNames() => _SelectedNames;
	string[]? _SelectedNames;

	//! PSF test.
	public int[]? GetSelectedIndexes() => _SelectedIndexes;
	int[]? _SelectedIndexes;

	public abstract bool CanRemove { get; }

	public abstract string Title { get; }

	public abstract void PopWork(bool active);

	public void Pop(bool active = true)
	{
		Far.Api.PostStep(() => PopWork(active)); //_201216_d3
	}

	protected void InitSelectedNames(IPanel panel)
	{
		ArgumentNullException.ThrowIfNull(panel);

		// nothing
		if (!panel.SelectionExists)
			return;

		// copy selected names
		var files = panel.SelectedFiles;
		_SelectedNames = new string[files.Count];
		for (int i = files.Count; --i >= 0;)
			_SelectedNames[i] = files[i].Name;
	}

	protected void InitSelectedIndexes(IPanel panel)
	{
		ArgumentNullException.ThrowIfNull(panel);

		// nothing
		if (!panel.SelectionExists)
			return;

		// keep selected indexes
		_SelectedIndexes = panel.SelectedIndexes();
	}
}
