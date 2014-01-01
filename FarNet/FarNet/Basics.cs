
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

namespace FarNet
{
	/// <summary>
	/// Macro areas.
	/// </summary>
	/// <remarks>
	/// Names are mostly the same as in macros (and as the registry key names).
	/// Positive values are real areas, the others are for internal use.
	/// </remarks>
	public enum MacroArea
	{
		/// <summary>No area.</summary>
		None,
		/// <summary>Screen grabbing mode ([AltIns]).</summary>
		Other,
		/// <summary>File panels.</summary>
		Shell,
		/// <summary>File viewer.</summary>
		Viewer,
		/// <summary>File editor.</summary>
		Editor,
		/// <summary>Dialog window.</summary>
		Dialog,
		/// <summary>Quick file search ([AltLetter]).</summary>
		Search,
		/// <summary>Drive menu.</summary>
		Disks,
		/// <summary>Main menu.</summary>
		MainMenu,
		/// <summary>Other menus.</summary>
		Menu,
		/// <summary>Help window.</summary>
		Help,
		/// <summary>Information panel.</summary>
		Info,
		/// <summary>Quick view panel.</summary>
		QView,
		/// <summary>Folder tree panel.</summary>
		Tree,
		/// <summary>Folder search panel.</summary>
		FindFolder,
		/// <summary>User menu.</summary>
		UserMenu,
		/// <summary>Auto completion menu.</summary>
		ShellAutoCompletion,
		/// <summary>Auto completion menu.</summary>
		DialogAutoCompletion,
	}
}
