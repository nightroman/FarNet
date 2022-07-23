
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

namespace FarNet;

/// <summary>
/// Window kind constants.
/// </summary>
public enum WindowKind
{
	/// <summary>
	/// Unknown window.
	/// </summary>
	None = -1,

	/// <summary>
	/// Desktop window.
	/// </summary>
	Desktop = 0,

	/// <summary>
	/// File panels.
	/// </summary>
	Panels = 1,

	/// <summary>
	/// Internal viewer window.
	/// </summary>
	Viewer = 2,

	/// <summary>
	/// Internal editor window.
	/// </summary>
	Editor = 3,

	/// <summary>
	/// Dialog.
	/// </summary>
	Dialog = 4,

	/// <summary>
	/// Menu.
	/// </summary>
	Menu = 5,

	/// <summary>
	/// Help window.
	/// </summary>
	Help = 6,

	/// <summary>
	/// Combo box.
	/// </summary>
	ComboBox = 7
}
