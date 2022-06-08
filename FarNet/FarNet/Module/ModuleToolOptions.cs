
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Module tool options, combination of flags.
/// </summary>
/// <remarks>
/// Choose the flags carefully, include areas where the tool really works.
/// Nobody wants to have their plugin menus polluted by not working items.
/// </remarks>
[Flags]
public enum ModuleToolOptions
{
	/// <summary>
	/// None.
	/// </summary>
	None,
	/// <summary>
	/// Show the item in the config menu and call it from other specified menus by [ShiftF9].
	/// </summary>
	Config = 1 << 0,
	/// <summary>
	/// Show the item in the disk menu.
	/// </summary>
	Disk = 1 << 1,
	/// <summary>
	/// Show the item in the editor plugin menu.
	/// </summary>
	Editor = 1 << 2,
	/// <summary>
	/// Show the item in the panel plugin menu.
	/// </summary>
	Panels = 1 << 3,
	/// <summary>
	/// Show the item in the viewer plugin menu.
	/// </summary>
	Viewer = 1 << 4,
	/// <summary>
	/// Show the item in the dialog plugin menu.
	/// </summary>
	Dialog = 1 << 5,
	/// <summary>
	/// Show the item in all [F11] menus (Panels | Editor | Viewer | Dialog).
	/// </summary>
	F11Menus = Panels | Editor | Viewer | Dialog,
	/// <summary>
	/// Show the item in [F11] menus and in the disk menu (F11Menus | Disk).
	/// </summary>
	AllMenus = F11Menus | Disk,
	/// <summary>
	/// Show the item in [F11] menus, the disk menu and the config menu (AllMenus | Config).
	/// </summary>
	AllAreas = AllMenus | Config
}
