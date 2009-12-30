/*
PowerShellFar plugin for Far Manager
Copyright (c) 2006 Roman Kuzmin
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
	/// <summary>
	/// ???
	/// </summary>
	class HelpMenuItems
	{
		/// <summary>
		/// ???
		/// </summary>
		public FarItem OpenFileAttributes { get; set; }

		/// <summary>
		/// ???
		/// </summary>
		public FarItem OpenFile { get; set; }

		/// <summary>
		/// ???
		/// </summary>
		public FarItem Copy { get; set; }

		/// <summary>
		/// ???
		/// </summary>
		public FarItem CopyHere { get; set; }

		/// <summary>
		/// ???
		/// </summary>
		public FarItem Move { get; set; }

		/// <summary>
		/// ???
		/// </summary>
		public FarItem Rename { get; set; }

		/// <summary>
		/// ???
		/// </summary>
		public FarItem Create { get; set; }

		/// <summary>
		/// ???
		/// </summary>
		public FarItem Delete { get; set; }

		/// <summary>
		/// ???
		/// </summary>
		public FarItem OpenFileMembers { get; set; }

		/// <summary>
		/// ???
		/// </summary>
		public FarItem Save { get; set; }

		/// <summary>
		/// ???
		/// </summary>
		public FarItem Exit { get; set; }

		/// <summary>
		/// ???
		/// </summary>
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
