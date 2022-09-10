
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;

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
	readonly object _lock = new();
	List<FarFile>? _idleFiles;

	/// <summary>
	/// Gets the super explorer of this panel.
	/// </summary>
	public new SuperExplorer Explorer { get { return (SuperExplorer)base.Explorer; } }

	/// <summary>
	/// New super panel with a super explorer.
	/// </summary>
	/// <param name="explorer">The panel explorer.</param>
	public SuperPanel(SuperExplorer explorer)
		: base(explorer)
	{
		CurrentLocation = Name;
		Title = Name;

		var plan = new PanelPlan
		{
			Columns = new FarColumn[] { new SetColumn() { Kind = "N" }, new SetColumn() { Kind = "O" } }
		};
		SetPlan(PanelViewMode.AlternativeFull, plan);
		ViewMode = PanelViewMode.AlternativeFull;
		SortMode = PanelSortMode.Unsorted;
	}

	///
	public SuperPanel() : this(new SuperExplorer()) { }

	/// <inheritdoc/>
	public override void UICopyMove(bool move)
	{
		// target
		var that = TargetPanel;
		if (that == null)
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
		this.Explorer.CommitFiles(this, that, files, move);
	}

	/// <inheritdoc/>
	public override void UITimer()
	{
		// let event happens first
		base.UITimer();

		// no job
		if (_idleFiles == null)
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
			if (_idleFiles == null)
				_idleFiles = new List<FarFile>();

			_idleFiles.AddRange(files);
		}
	}

	/// <inheritdoc/>
	public override bool UIKeyPressed(KeyInfo key)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		switch (key.VirtualKeyCode)
		{
			case KeyCode.F7:

				if (key.Is())
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

				break;

			case KeyCode.PageUp:

				if (key.IsCtrl())
				{
					var efile = (SuperFile)CurrentFile;
					if (efile == null)
						break;

					var panel = efile.Explorer.CreatePanel();
					panel.PostFile(efile.File);
					panel.OpenChild(this);
					return true;
				}

				break;
		}

		return base.UIKeyPressed(key);
	}
}
