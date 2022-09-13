
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections.Generic;

namespace PowerShellFar;

class HelpMenuItems
{
	public FarItem? ApplyCommand { get; set; }
	public FarItem? OpenFileAttributes { get; set; }
	public FarItem? OpenFile { get; set; }
	public FarItem? Copy { get; set; }
	public FarItem? CopyHere { get; set; }
	public FarItem? Move { get; set; }
	public FarItem? Rename { get; set; }
	public FarItem? Create { get; set; }
	public FarItem? Delete { get; set; }
	public FarItem? OpenFileMembers { get; set; }
	public FarItem? Save { get; set; }
	public FarItem? Exit { get; set; }
	public FarItem? Help { get; set; }
}

/// <summary>
/// Arguments of the <see cref="AnyPanel.MenuCreating"/> event.
/// </summary>
/// <remarks>
/// Handlers should add their menu items to the panel menu.
/// If needed, e.g. to decide to set items disabled or not include them at all,
/// they should use ready <see cref="CurrentFile"/> and <see cref="SelectedFiles"/>
/// instead of requesting them from the panel.
/// </remarks>
public class PanelMenuEventArgs : EventArgs
{
	internal PanelMenuEventArgs(IMenu menu, FarFile? currentFile, IList<FarFile> selectedFiles)
	{
		Menu = menu;
		CurrentFile = currentFile;
		SelectedFiles = selectedFiles;
	}

	/// <summary>
	/// Menu for adding items.
	/// </summary>
	public IMenu Menu { get; }

	/// <summary>
	/// Helper: the current file.
	/// </summary>
	public FarFile? CurrentFile { get; }

	/// <summary>
	/// Helper: the selected file list.
	/// </summary>
	public IList<FarFile> SelectedFiles { get; }
}
