﻿
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;
using System.Threading;

namespace FarNet.Tools;

/// <summary>
/// Panel of other explorer files, analogue of the temporary panel.
/// </summary>
/// <remarks>
/// Unlike the temporary panel the super panel deals with files from module panels, including mixed.
/// It knows how to view / edit / copy / move / delete and etc. its files because any super file
/// keeps the reference to its source explorer that actually performs that operations on files,
/// the super panel simply dispatches operations and files.
/// </remarks>
public class SuperPanel : Panel
{
	const string Name = "Super Panel";
	readonly Lock _lock = new();
	List<FarFile>? _idleFiles;

	/// <summary>
	/// Gets the super explorer of this panel.
	/// </summary>
	public new SuperExplorer Explorer => (SuperExplorer)base.Explorer;

	/// <summary>
	/// New super panel with a super explorer.
	/// </summary>
	/// <param name="explorer">The panel explorer.</param>
	public SuperPanel(SuperExplorer explorer) : base(explorer)
	{
		CurrentLocation = Name;
		Title = Name;

		var plan = new PanelPlan
		{
			Columns =
			[
				new SetColumn { Kind = "N" },
				new SetColumn { Kind = "O" },
			]
		};
		SetPlan(PanelViewMode.AlternativeFull, plan);
		ViewMode = PanelViewMode.AlternativeFull;
		SortMode = PanelSortMode.Unsorted;
	}

	///
	public SuperPanel() : this(new SuperExplorer())
	{
	}

	/// <inheritdoc/>
	public override void UICopyMove(bool move)
	{
		// target
		var that = TargetPanel;
		if (that is null)
		{
			base.UICopyMove(move);
			return;
		}

		// can?
		if (!that.Explorer.CanAcceptFiles)
			return;

		// files
		var files = GetSelectedFiles();
		if (files.Length == 0)
			return;

		// call
		Explorer.CommitFiles(this, that, files, move);
	}

	/// <inheritdoc/>
	public override void UITimer()
	{
		// let event happens first
		base.UITimer();

		// no job
		if (_idleFiles is null)
			return;

		// add files
		lock (_lock)
		{
			Explorer.AddFiles(_idleFiles);
			_idleFiles = null;
		}

		// show
		Update(true);
		Redraw();
	}

	/// <summary>
	/// Adds <see cref="SuperFile"/> files asynchronously.
	/// </summary>
	/// <param name="files">The files to add.</param>
	/// <remarks>
	/// It is thread safe and can be called from background threads.
	/// The added files will be shown later when the panel is idle.
	/// </remarks>
	public void AddFilesAsync(IEnumerable<FarFile> files)
	{
		lock (_lock)
		{
			_idleFiles ??= [];
			_idleFiles.AddRange(files);
		}
	}

	/// <inheritdoc/>
	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			case KeyCode.F7 when key.Is():
				{
					var files = GetSelectedFiles();
					if (files.Length > 0)
					{
						Explorer.RemoveFiles(files);
						Update(false);
						Redraw();
					}
					return true;
				}
			case KeyCode.PageUp when key.IsCtrl():
				{
					var efile = (SuperFile?)CurrentFile;
					if (efile is null)
						break;

					var panel = efile.Explorer.CreatePanel();
					panel.PostFile(efile.File);
					panel.OpenChild(this);
					return true;
				}
		}
		return base.UIKeyPressed(key);
	}
}
