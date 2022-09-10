
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Arguments of menu item handlers, e.g. menu key handlers.
/// A menu key closes the menu and gets stored as <see cref="IAnyMenu.Key"/>.
/// Use <see cref="Ignore"/> or <see cref="Restart"/> for different actions.
/// </summary>
public class MenuEventArgs : EventArgs
{
	/// <param name="item">Current item.</param>
	public MenuEventArgs(FarItem? item)
	{
		Item = item;
	}

	/// <summary>
	/// Gets the current menu item if any.
	/// </summary>
	public FarItem? Item { get; }

	/// <summary>
	/// Tells to do nothing, a handler has processed everything.
	/// </summary>
	public bool Ignore { get; set; }

	/// <summary>
	/// Tells to restart the menu, normally when menu items have changed.
	/// In some cases you may set <see cref="IAnyMenu.Selected"/> to a new value or -1.
	/// E.g. you have recreated all items and tell the first or last to be the current.
	/// </summary>
	public bool Restart { get; set; }
}
