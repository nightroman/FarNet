
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;

namespace FarNet;

/// <summary>
/// Menu base interface.
/// Contains common settings and item collection.
/// </summary>
public interface IAnyMenu
{
	/// <summary>
	/// Gets or sets the X-position. Default: -1 (to be calculated).
	/// </summary>
	int X { get; set; }

	/// <summary>
	/// Gets or sets the Y-position. Default: -1 (to be calculated).
	/// </summary>
	int Y { get; set; }

	/// <summary>
	/// Gets or sets the max height (max number of visible items).
	/// </summary>
	int MaxHeight { get; set; }

	/// <summary>
	/// Gets or sets the title line text.
	/// </summary>
	string Title { get; set; }

	/// <summary>
	/// Gets or sets the bottom line text.
	/// </summary>
	string Bottom { get; set; }

	/// <summary>
	/// Gets the menu item list.
	/// </summary>
	/// <remarks>
	/// You should add your items to this list.
	/// </remarks>
	IList<FarItem> Items { get; }

	/// <summary>
	/// Gets or sets the selected item index.
	/// </summary>
	/// <remarks>
	/// Before and after <see cref="Show"/>:
	/// before: selects the item by this index;
	/// after: gets the selected item index or -1 on cancel.
	/// </remarks>
	//! The name is not ideal but consistent with many others.
	int Selected { get; set; }

	/// <summary>
	/// Gets user data attached to the <see cref="Selected"/> item or null on cancel.
	/// </summary>
	object SelectedData { get; }

	/// <summary>
	/// Shows the menu.
	/// </summary>
	/// <returns>
	/// True if any menu item is selected, false otherwise including the case of a break key hit in an empty menu.
	/// </returns>
	/// <remarks>
	/// If a menu item is selected and there is no break key hit then its <see cref="FarItem.Click"/> is called.
	/// Break key cases should be processed by a caller.
	/// <para>
	/// Index of the selected item is kept in <see cref="Selected"/>. It is reused if the menu is shown again:
	/// this is normally useful; if it is not then this value should be reset by a caller.
	/// </para>
	/// </remarks>
	bool Show();

	/// <include file='doc.xml' path='doc/HelpTopic/*'/>
	string HelpTopic { get; set; }

	/// <summary>
	/// Tells to select the last item on <see cref="Show()"/> if <see cref="Selected"/> is not set.
	/// </summary>
	bool SelectLast { get; set; }

	/// <summary>
	/// Gets or sets a sender to be passed in <see cref="FarItem.Click"/> event handlers.
	/// </summary>
	/// <remarks>
	/// By default <see cref="FarItem"/> is a sender. You can provide another sender passed in.
	/// </remarks>
	object Sender { get; set; }

	/// <summary>
	/// Tells to show ampersands in items instead of using them as hotkey marks.
	/// </summary>
	bool ShowAmpersands { get; set; }

	/// <include file='doc.xml' path='doc/WrapCursor/*'/>
	bool WrapCursor { get; set; }

	/// <include file='doc.xml' path='doc/AutoAssignHotkeys/*'/>
	bool AutoAssignHotkeys { get; set; }

	/// <summary>
	/// Tells to not show the dialog shadow.
	/// </summary>
	bool NoShadow { get; set; }

	/// <summary>
	/// Adds a new item to <see cref="Items"/> and returns it.
	/// </summary>
	/// <param name="text">Item text.</param>
	/// <returns>New menu item. You may set more properties.</returns>
	FarItem Add(string text);

	/// <summary>
	/// Adds a new item to <see cref="Items"/> and returns it.
	/// </summary>
	/// <param name="text">Item text.</param>
	/// <param name="click">Handler to be called on <see cref="FarItem.Click"/>.</param>
	/// <returns>New menu item. You may set more properties.</returns>
	FarItem Add(string text, EventHandler<MenuEventArgs> click);

	/// <summary>
	/// Gets a key that has closed the menu or an empty key.
	/// </summary>
	/// <remarks>
	/// Keys that close the menu are added before showing the menu.
	/// </remarks>
	KeyData Key { get; }

	/// <summary>
	/// Adds a key code that closes the menu.
	/// </summary>
	/// <param name="virtualKeyCode">Key code, for example <see cref="KeyCode"/>.</param>
	void AddKey(int virtualKeyCode);

	/// <summary>
	/// Adds a key code that closes the menu.
	/// </summary>
	/// <param name="virtualKeyCode">Key code, for example <see cref="KeyCode"/>.</param>
	/// <param name="controlKeyState">Key states.</param>
	void AddKey(int virtualKeyCode, ControlKeyStates controlKeyState);

	/// <summary>
	/// Adds a key code that closes the menu and invokes a handler.
	/// </summary>
	/// <param name="virtualKeyCode">Key code, for example <see cref="KeyCode"/>.</param>
	/// <param name="controlKeyState">Key states.</param>
	/// <param name="handler">Key handler triggered on the key pressed.</param>
	void AddKey(int virtualKeyCode, ControlKeyStates controlKeyState, EventHandler<MenuEventArgs> handler);
}
