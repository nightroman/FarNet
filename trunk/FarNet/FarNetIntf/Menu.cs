/*
FAR.NET plugin for Far Manager
Copyright (c) 2005-2008 FAR.NET Team
*/

using FarManager.Forms;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System;

namespace FarManager
{
	/// <summary>
	/// Arguments of a menu key handler, see <see cref="IListMenu.AddKey(int,EventHandler&lt;MenuEventArgs&gt;)"/>.
	/// By default the key closes the menu and it is stored in <see cref="IAnyMenu.BreakKey"/>.
	/// Use <see cref="Ignore"/> or <see cref="Restart"/> to perform different actions.
	/// </summary>
	[DebuggerStepThroughAttribute]
	public class MenuEventArgs : EventArgs
	{
		/// <param name="item">Current item.</param>
		public MenuEventArgs(IMenuItem item)
		{
			_Item = item;
		}
		IMenuItem _Item;
		/// <summary>
		/// Current item.
		/// </summary>
		public IMenuItem Item
		{
			get { return _Item; }
		}
		bool _Ignore;
		/// <summary>
		/// Tells to do nothing, a handler has processed everything.
		/// </summary>
		public bool Ignore
		{
			get { return _Ignore; }
			set { _Ignore = value; }
		}
		bool _Restart;
		/// <summary>
		/// Tells to restart the menu, normally when items or properties are changed.
		/// In some cases you may want to set proper <see cref="IAnyMenu.Selected"/> or -1
		/// (e.g. you recreated all items and want the first or the last to be current after that).
		/// </summary>
		public bool Restart
		{
			get { return _Restart; }
			set { _Restart = value; }
		}
	}

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
		/// Item is disabled.
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
		/// Item is a separator. <see cref="Text"/>, if any, is shown center aligned.
		/// </summary>
		bool IsSeparator { get; set; }
		/// <summary>
		/// If this menu item is not disabled then these flags enable or disable it depending on where a menu is called from.
		/// </summary>
		ToolOptions From { get; set; }
		/// <summary>
		/// Event raised when a menu item is clicked.
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
		/// <param name="text">Item text.</param>
		/// <returns>new menu item</returns>
		IMenuItem Add(string text);
		/// <summary>
		/// Add menu item to list
		/// </summary>
		/// <param name="text">Item text.</param>
		/// <param name="isChecked">Item is checked.</param>
		/// <returns>new menu item</returns>
		IMenuItem Add(string text, bool isChecked);
		/// <summary>
		/// Add menu item to list
		/// </summary>
		/// <param name="text">Item text.</param>
		/// <param name="isChecked">Item is checked.</param>
		/// <param name="isSeparator">Item is a separator.</param>
		/// <returns>new menu item</returns>
		IMenuItem Add(string text, bool isChecked, bool isSeparator);
		/// <summary>
		/// Add menu item to list
		/// </summary>
		/// <param name="text">Item text.</param>
		/// <param name="onClick"><see cref="IMenuItem.OnClick"/>.</param>
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
		/// Menu title.
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
		/// User data attached to the <see cref="Selected"/> item or null if nothing is selected.
		/// </summary>
		object SelectedData { get; }
		/// <summary>
		/// Shows the menu.
		/// </summary>
		/// <returns>true if a menu item is selected.</returns>
		/// <remarks>
		/// If a menu item is selected then its <see cref="IMenuItem.OnClick"/> is fired.
		/// Index of the selected item is stored in <see cref="Selected"/>.
		/// (if the menu is shown again it is used for the current item).
		/// </remarks>
		bool Show();
		/// <include file='doc.xml' path='docs/pp[@name="HelpTopic"]/*'/>
		string HelpTopic { get; set; }
		/// <summary>
		/// Tells to select the last item on <see cref="Show()"/> if <see cref="Selected"/> is not set.
		/// </summary>
		bool SelectLast { get; set; }
		/// <summary>
		/// Sender passed in <see cref="IMenuItem.OnClick"/> event.
		/// </summary>
		/// <remarks>
		/// By default <see cref="IMenuItem"/> is a sender. You can provide another sender passed in.
		/// </remarks>
		object Sender { get; set; }
		/// <summary>
		/// Show ampersands in items instead of using them for accelerator characters.
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
		/// A key that has closed the menu; it is virtual <see cref="VKeyCode"/> for <see cref="IMenu"/> and internal <see cref="KeyCode"/> for <see cref="IListMenu"/>.
		/// </summary>
		int BreakKey { get; }
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
		/// List of <see cref="VKeyCode"/> codes that close the menu. See VK_* in FAR API.
		/// </summary>
		IList<int> BreakKeys { get; }
		/// <summary>
		/// Creates low level internal data of the menu from the current items. Normally you have to call <see cref="Unlock"/> after use.
		/// </summary>
		/// <remarks>
		/// Used for better performance when you call <see cref="IAnyMenu.Show"/> repeatedly
		/// with an item set that never changes (e.g. a plugin menu with fixed command set:
		/// it can be created once on <see cref="BasePlugin.Connect"/> and locked forever -
		/// in this particular case you don't even have to call <see cref="Unlock"/>).
		/// <para>
		/// Don't change the menu or item set before <see cref="Unlock"/>.
		/// You still can change item properties except <see cref="IMenuItem.Text"/>.
		/// </para>
		/// </remarks>
		void Lock();
		/// <summary>
		/// Destroys internal data created by <see cref="Lock"/>.
		/// Menu and items can be changed again if the menu is still in use.
		/// </summary>
		void Unlock();
	}

	/// <summary>
	/// Filter pattern options.
	/// All combinations are allowed though normally you have to set one and only of
	/// <see cref="Regex"/>, <see cref="Prefix"/> or <see cref="Substring"/>.
	/// </summary>
	[Flags]
	public enum PatternOptions
	{
		/// <summary>
		/// None. Usually it means that filter is not enabled.
		/// </summary>
		None,
		/// <summary>
		/// Regular expression with forms: <c>standard</c> | <c>?prefix</c> | <c>*substring</c>.
		/// In prefix and substring forms * and ? are wildcards if <see cref="Literal"/> is not set,
		/// otherwise prefix and substring are exact string parts.
		/// </summary>
		Regex = 1,
		/// <summary>
		/// Prefix pattern, * and ? are wildcards if <see cref="Literal"/> is not set.
		/// </summary>
		Prefix = 2,
		/// <summary>
		/// Substring pattern, * and ? are wildcards if <see cref="Literal"/> is not set.
		/// </summary>
		Substring = 4,
		/// <summary>
		/// All filter symbols including * and ? are literal.
		/// Should be used with one of <see cref="Regex"/>, <see cref="Prefix"/> or <see cref="Substring"/>.
		/// </summary>
		Literal = 8
	}

	/// <summary>
	/// Menu implemented as a dialog with a list box.
	/// It is created by <see cref="IFar.CreateListMenu"/>.
	/// </summary>
	/// <remarks>
	/// This kind of a menu is more suitable for a list of objects than a set of commands.
	/// It provides two kinds of filters: permanent and incremental, both with many options, both can be used together.
	/// <para>
	/// Keys: [CtrlDown] - default key to open a permanent filter input box;
	/// [Backspace] - removes the last symbol from the incremental filter string (until the initial part is reached, if any);
	/// [ShiftBackspace] - removes the incremental filter string completely, even initial part (rarely needed, but there are some cases).
	/// </para>
	/// </remarks>
	public interface IListMenu : IAnyMenu
	{
		/// <summary>
		/// Enables permanent filter and defines its type.
		/// </summary>
		PatternOptions FilterOptions { get; set; }
		/// <summary>
		/// Permanent filter pattern.
		/// It does not enable filter itself, you have to set <see cref="FilterOptions"/>.
		/// If it is empty, it is taken from history if <see cref="FilterHistory"/> and <see cref="FilterRestore"/> are set.
		/// </summary>
		string Filter { get; set; }
		/// <summary>
		/// Permanent filter history used by the filter input box opened by <see cref="FilterKey"/>.
		/// </summary>
		string FilterHistory { get; set; }
		/// <summary>
		/// Tells to restore permanent filter pattern from history
		/// if <see cref="Filter"/> is empty and <see cref="FilterHistory"/> is set.
		/// </summary>
		bool FilterRestore { get; set; }
		/// <summary>
		/// Internal key code that opens a permanent filter input box.
		/// Default: CtrlDown, i.e. <c>KeyCode.Ctrl | KeyCode.Down</c>
		/// </summary>
		int FilterKey { get; set; }
		/// <summary>
		/// Enables specified incremental filter and related options
		/// and disables hotkey highlighting and related options.
		/// </summary>
		PatternOptions IncrementalOptions { get; set; }
		/// <summary>
		/// Predefined incremental filter pattern used to continue typing.
		/// It is not used to filter the initial list, initial list contains all items.
		/// It does not enable filter itself, you have to set <see cref="IncrementalOptions"/>.
		/// </summary>
		string Incremental { get; set; }
		/// <summary>
		/// Tells to select a single item or nothing automatically on less than two items.
		/// </summary>
		bool AutoSelect { get; set; }
		/// <summary>
		/// Disables the dialog shadow.
		/// </summary>
		bool NoShadow { get; set; }
		/// <summary>
		/// Screen margin size.
		/// </summary>
		int ScreenMargin { get; set; }
		/// <summary>
		/// Tells to use usual FAR menu margins.
		/// </summary>
		bool UsualMargins { get; set; }
		/// <summary>
		/// Adds an internal key code that closes the menu.
		/// </summary>
		/// <param name="key">Internal key code, see <see cref="KeyCode"/> helper.</param>
		void AddKey(int key);
		/// <summary>
		/// Adds an internal key code with associated handler.
		/// </summary>
		/// <param name="key">Internal key code, see <see cref="KeyCode"/> helper.</param>
		/// <param name="handler">Key handler triggered on the key pressed.</param>
		void AddKey(int key, EventHandler<MenuEventArgs> handler);
	}

}
