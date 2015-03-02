
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using FarNet;

namespace PowerShellFar
{
	class HelpMenuItems
	{
		public FarItem ApplyCommand { get; set; }
		public FarItem OpenFileAttributes { get; set; }
		public FarItem OpenFile { get; set; }
		public FarItem Copy { get; set; }
		public FarItem CopyHere { get; set; }
		public FarItem Move { get; set; }
		public FarItem Rename { get; set; }
		public FarItem Create { get; set; }
		public FarItem Delete { get; set; }
		public FarItem OpenFileMembers { get; set; }
		public FarItem Save { get; set; }
		public FarItem Exit { get; set; }
		public FarItem Help { get; set; }
	}

	/// <summary>
	/// Arguments of the <see cref="AnyPanel.MenuCreating"/> event.
	/// </summary>
	/// <remarks>
	/// Handlers should add their menu items to the panel menu.
	/// If needed, e.g. to decide to set items disabled or not include them at all,
	/// they should use ready <see cref="CurrentFile"/> and <see cref="SelectedList"/>
	/// instead of requesting them from the panel.
	/// </remarks>
	public class PanelMenuEventArgs : EventArgs
	{
		internal PanelMenuEventArgs(IMenu menu, FarFile currentFile, IList<FarFile> selectedList)
		{
			Menu = menu;
			CurrentFile = currentFile;
			SelectedList = selectedList;
		}

		/// <summary>
		/// Menu for adding items.
		/// </summary>
		public IMenu Menu { get; private set; }

		/// <summary>
		/// Facility: the current file.
		/// </summary>
		public FarFile CurrentFile { get; private set; }

		/// <summary>
		/// Facility: the selected file list.
		/// </summary>
		public IList<FarFile> SelectedList { get; private set; }
	}
}
