
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Standard Far menu.
/// It is created by <see cref="IFar.CreateMenu"/>.
/// </summary>
public interface IMenu : IAnyMenu, IDisposable
{
	/// <summary>
	/// Tells to assign hotkeys automatically from bottom.
	/// </summary>
	bool ReverseAutoAssign { get; set; }

	/// <summary>
	/// Tells to set the console title to the menu title.
	/// </summary>
	bool ChangeConsoleTitle { get; set; }

	/// <summary>
	/// Tells to show the menu with no box. Options <see cref="NoMargin"/> and <see cref="SingleBox"/> are not used.
	/// </summary>
	bool NoBox { get; set; }

	/// <summary>
	/// Tells to show the menu with no margin.
	/// </summary>
	bool NoMargin { get; set; }

	/// <summary>
	/// Tells to show the menu with single box.
	/// </summary>
	bool SingleBox { get; set; }

	/// <summary>
	/// Creates low level internal data of the menu from the current items.
	/// Normally you have to call <see cref="Unlock"/> after use.
	/// </summary>
	/// <remarks>
	/// Used for better performance when you call <see cref="IAnyMenu.Show"/> repeatedly
	/// with an item set that never changes (e.g. a module menu with fixed command set:
	/// it can be created once on <see cref="ModuleHost.Connect"/> and locked forever -
	/// in this particular case you don't even have to call <see cref="Unlock"/>).
	/// <para>
	/// Don't change the menu or item set before <see cref="Unlock"/>.
	/// You still can change item properties except <see cref="FarItem.Text"/>.
	/// </para>
	/// </remarks>
	void Lock();

	/// <summary>
	/// Destroys internal data created by <see cref="Lock"/>.
	/// Menu and items can be changed again if the menu is still in use.
	/// </summary>
	void Unlock();
}
