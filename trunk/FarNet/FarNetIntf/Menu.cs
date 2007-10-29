/*
Far.NET plugin for Far Manager
Copyright (c) 2005-2007 Far.NET Team
*/

using FarManager.Forms;
using System.Collections.Generic;
using System.Collections;
using System;

namespace FarManager
{
	/// <summary>
	/// Used by <see cref="IAnyMenu.Items"/> in a menu or a menu-list and <see cref="IComboBox"/>, <see cref="IListBox"/>.
	/// </summary>
	public interface IMenuItem
	{
		/// <summary>
		/// Item is checked (i.e. ticked off in a menu or a list).
		/// </summary>
		bool Checked { get; set; }
		/// <summary>
		/// Item is disabled. It is used only in a list item.
		/// </summary>
		bool Disabled { get; set; }
		/// <summary>
		/// Any user data attached to the item.
		/// </summary>
		object Data { get; set; }
		/// <summary>
		/// Item text.
		/// </summary>
		string Text { get; set; }
		/// <summary>
		/// Item is separator. <see cref="Text"/> is used for a list item and ignored for a menu item.
		/// </summary>
		bool IsSeparator { get; set; }
		/// <summary>
		/// Event raised when an item is clicked.
		/// </summary>
		event EventHandler OnClick;
	}

	/// <summary>
	/// List of menu items <see cref="IMenuItem"/> in a menu.
	/// </summary>
	public interface IMenuItems : IList<IMenuItem>
	{
		/// <summary>
		/// Add menu item to list
		/// </summary>
		/// <param name="text"><see cref="IMenuItem.Text"/></param>
		/// <returns>new menu item</returns>
		IMenuItem Add(string text);
		/// <summary>
		/// Add menu item to list
		/// </summary>
		/// <param name="text"><see cref="IMenuItem.Text"/></param>
		/// <param name="isChecked"><see cref="IMenuItem.Checked"/></param>
		/// <returns>new menu item</returns>
		IMenuItem Add(string text, bool isChecked);
		/// <summary>
		/// Add menu item to list
		/// </summary>
		/// <param name="text"><see cref="IMenuItem.Text"/></param>
		/// <param name="isChecked"><see cref="IMenuItem.Checked"/></param>
		/// <param name="isSeparator"><see cref="IMenuItem.IsSeparator"/></param>
		/// <returns>new menu item</returns>
		IMenuItem Add(string text, bool isChecked, bool isSeparator);
		/// <summary>
		/// Add menu item to list
		/// </summary>
		/// <param name="text"><see cref="IMenuItem.Text"/></param>
		/// <param name="onClick"><see cref="IMenuItem.OnClick"/></param>
		/// <returns>new menu item</returns>
		IMenuItem Add(string text, EventHandler onClick);
	}

	/// <summary>
	/// Any menu interface.
	/// Contains common settings and menu item collection.
	/// </summary>
	public interface IAnyMenu
	{
		/// <summary>
		/// X-position. Default: -1 (to be calculated).
		/// </summary>
		int X { get; set; }
		/// <summary>
		/// Y-position. Default: -1 (to be calculated).
		/// </summary>
		int Y { get; set; }
		/// <summary>
		/// Maximal height (maximal number of visible items).
		/// </summary>
		int MaxHeight { get; set; }
		/// <summary>
		/// Title of the menu.
		/// </summary>
		string Title { get; set; }
		/// <summary>
		/// Bottom line text.
		/// </summary>
		string Bottom { get; set; }
		/// <summary>
		/// Menu items.
		/// </summary>
		IMenuItems Items { get; }
		/// <summary>
		/// Before <see cref="Show"/> tells to select the item by this index.
		/// After <see cref="Show"/> returns the selected item index or -1 if nothing is selected.
		/// </summary>
		int Selected { get; set; }
		/// <summary>
		/// User data attached to the <see cref="Selected"/> menu item or null if nothing is selected.
		/// </summary>
		object SelectedData { get; }
		/// <summary>
		/// Shows the menu.
		/// </summary>
		/// <returns>true if a menu item is selected.</returns>
		/// <remarks>
		/// If a menu item is selected then its <see cref="IMenuItem.OnClick"/> is fired.
		/// Index of the selected item is stored in <see cref="Selected"/>.
		/// </remarks>
		bool Show();
		/// <include file='doc.xml' path='docs/pp[@name="HelpTopic"]/*'/>
		string HelpTopic { get; set; }
		/// <summary>
		/// Tells to select the last item on <see cref="Show()"/>.
		/// </summary>
		bool SelectLast { get; set; }
		/// <summary>
		/// Filter string. Format: regex | *substring | ?prefix.
		/// It is also used by a filter input box (if it is enabled by <see cref="FilterKey"/>).
		/// If null, it is taken from history if
		/// <see cref="FilterHistory"/> and <see cref="FilterRestore"/> are set.
		/// </summary>
		string Filter { get; set; }
		/// <summary>
		/// Filter history used by the filter input box opened by <see cref="FilterKey"/>.
		/// </summary>
		string FilterHistory { get; set; }
		/// <summary>
		/// Tells to restore a filter from history if <see cref="Filter"/> is null
		/// and <see cref="FilterHistory"/> is set.
		/// </summary>
		bool FilterRestore { get; set; }
		/// <summary>
		/// Virtual key code (for <see cref="IMenu"/>, added to break keys automatically)
		/// or internal key code (for <see cref="IListMenu"/>) opening the filter input box.
		/// </summary>
		int FilterKey { get; set; }
		/// <summary>
		/// Sender passed in <see cref="IMenuItem.OnClick"/> event.
		/// </summary>
		/// <remarks>
		/// By default <see cref="IMenuItem"/> is a sender.
		/// Creator of a menu can provide another sender passed in events.
		/// </remarks>
		object Sender { get; set; }
		/// <summary>
		/// Show ampersands in menu items instead of using them for accelerator characters.
		/// </summary>
		bool ShowAmpersands { get; set; }
		/// <summary>
		/// Cursor will go to upper position if it is at downmost position and down is pressed.
		/// </summary>
		bool WrapCursor { get; set; }
		/// <summary>
		/// Assign hotkeys automatically.
		/// </summary>
		bool AutoAssignHotkeys { get; set; }
		/// <summary>
		/// For <see cref="IMenu"/>: <see cref="BreakKeys"/> index of a key interrupted the menu.
		/// For <see cref="IListMenu"/>: internal key code, see <see cref="KeyCode"/> helper.
		/// </summary>
		int BreakCode { get; }
		/// <summary>
		/// List of keys that closes menu.
		/// For <see cref="IMenu"/> they are virtual key code (VK_* in FAR API docs).
		/// For <see cref="IListMenu"/> they are internal key codes, see <see cref="KeyCode"/> helper.
		/// </summary>
		IList<int> BreakKeys { get; }
	}

	/// <summary>
	/// Standard FAR menu.
	/// It is created by <see cref="IFar.CreateMenu"/>.
	/// </summary>
	public interface IMenu : IAnyMenu, IDisposable
	{
		/// <summary>
		/// Assign hotkeys automatically from bottom.
		/// </summary>
		bool ReverseAutoAssign { get; set; }
		/// <summary>
		/// Creates low level internal data of the menu from the current items.
		/// DON'T use filters or change menu items before <see cref="Unlock"/>
		/// which also disposes internal resources.
		/// </summary>
		/// <remarks>
		/// Used for better performance when you call <see cref="IAnyMenu.Show"/> repeatedly
		/// with an item set that never changes (e.g. a plugin menu with fixed command set:
		/// it can be created once on <see cref="BasePlugin.Connect"/> and locked forever;
		/// in this particular case you don't even have to unlock).
		/// </remarks>
		void Lock();
		/// <summary>
		/// Destroys internal data created by <see cref="Lock"/>.
		/// Menu and items can be changed again if the menu is still in use.
		/// </summary>
		void Unlock();
	}

	/// <summary>
	/// Filter options. By default * and ? are wildcard symbols.
	/// See <see cref="IListMenu.Incremental"/>.
	/// </summary>
	[Flags]
	public enum FilterOptions
	{
		/// <summary>
		/// No filter.
		/// </summary>
		None,
		/// <summary>
		/// Filter by prefix. 
		/// </summary>
		Prefix = 1,
		/// <summary>
		/// Filter by substring.
		/// </summary>
		Substring = 2,
		/// <summary>
		/// All filter symbols including * and ? are literal.
		/// </summary>
		Literal = 4
	}

	/// <summary>
	/// Menu implemented as a dialog with a list box.
	/// It is created by <see cref="IFar.CreateListMenu"/>.
	/// </summary>
	/// <remarks>
	/// This kind of a menu is more suitable for selecting an item from a list.
	/// It provides extra features for incremental filtering by typed substring or prefix.
	/// </remarks>
	public interface IListMenu : IAnyMenu
	{
		/// <summary>
		/// Predefined incremental filter string used at start.
		/// It does not enables filtering itself, you have to set <see cref="Incremental"/>.
		/// </summary>
		string IncrementalFilter { get; set; }
		/// <summary>
		/// Enables specified incremental filter and related options
		/// and disables hotkey highlighting and related options.
		/// </summary>
		FilterOptions Incremental { get; set; }
		/// <summary>
		/// Tells to select a single item or nothing automatically on less than two items.
		/// </summary>
		bool AutoSelect { get; set; }
		/// <summary>
		/// Disables menu shadow.
		/// </summary>
		bool NoShadow { get; set; }
		/// <summary>
		/// Raised before show.
		/// </summary>
		event EventHandler Showing;
		/// <summary>
		/// Underlying list box.
		/// Use it instantly in <see cref="Showing"/> to subscribe to its events.
		/// </summary>
		IListBox ListBox { get; }
		/// <summary>
		/// Screen margin size.
		/// </summary>
		int ScreenMargin { get; set; }
	}
}
