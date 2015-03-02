
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FarNet
{
	/// <summary>
	/// Arguments of a menu key handler.
	/// By default the key closes the menu and it is stored in <see cref="IAnyMenu.Key"/>.
	/// Use <see cref="Ignore"/> or <see cref="Restart"/> to perform different actions.
	/// </summary>
	public class MenuEventArgs : EventArgs
	{
		/// <param name="item">Current item.</param>
		public MenuEventArgs(FarItem item)
		{
			_Item = item;
		}
		FarItem _Item;
		/// <summary>
		/// Current item.
		/// </summary>
		public FarItem Item
		{
			get { return _Item; }
		}
		/// <summary>
		/// Tells to do nothing, a handler has processed everything.
		/// </summary>
		public bool Ignore { get; set; }
		/// <summary>
		/// Tells to restart the menu, normally when items or properties are changed.
		/// In some cases you may want to set proper <see cref="IAnyMenu.Selected"/> or -1
		/// (e.g. you recreated all items and want the first or the last to be current after that).
		/// </summary>
		public bool Restart { get; set; }
	}

	/// <summary>
	/// Item of a menu, a list menu or one of list dialog controls.
	/// </summary>
	/// <seealso cref="IMenu"/>
	/// <seealso cref="IListMenu"/>
	/// <seealso cref="Forms.IListBox"/>
	/// <seealso cref="Forms.IComboBox"/>
	public abstract class FarItem
	{
		/// <summary>
		/// Item text.
		/// </summary>
		public abstract string Text { get; set; }
		/// <summary>
		/// Item is checked.
		/// </summary>
		public virtual bool Checked { get { return false; } set { throw new NotImplementedException(); } }
		/// <summary>
		/// Item is disabled. It cannot be selected.
		/// </summary>
		public virtual bool Disabled { get { return false; } set { throw new NotImplementedException(); } }
		/// <summary>
		/// Item is shown, but cannot be selected.
		/// </summary>
		public virtual bool Grayed { get { return false; } set { throw new NotImplementedException(); } }
		/// <summary>
		/// Item is hidden. It cannot be selected.
		/// </summary>
		public virtual bool Hidden { get { return false; } set { throw new NotImplementedException(); } }
		/// <summary>
		/// Item is a separator. <see cref="Text"/>, if any, is shown center aligned.
		/// </summary>
		public virtual bool IsSeparator { get { return false; } set { throw new NotImplementedException(); } }
		/// <summary>
		/// Any user data attached to the item.
		/// </summary>
		public virtual object Data { get { return null; } set { throw new NotImplementedException(); } }
		/// <summary>
		/// Called when a menu item is selected.
		/// </summary>
		public virtual EventHandler<MenuEventArgs> Click { get { return null; } set { throw new NotImplementedException(); } }
	}

	/// <summary>
	/// Item of a menu, a list menu or one of list dialog controls.
	/// </summary>
	/// <seealso cref="IMenu"/>
	/// <seealso cref="IListMenu"/>
	/// <seealso cref="Forms.IListBox"/>
	/// <seealso cref="Forms.IComboBox"/>
	public sealed class SetItem : FarItem
	{
		/// <summary>
		/// Item text.
		/// </summary>
		public override string Text { get; set; }
		/// <summary>
		/// Item is checked.
		/// </summary>
		public override bool Checked { get; set; }
		/// <summary>
		/// Item is disabled. It cannot be selected.
		/// </summary>
		public override bool Disabled { get; set; }
		/// <summary>
		/// Item is shown, but cannot be selected.
		/// </summary>
		public override bool Grayed { get; set; }
		/// <summary>
		/// Item is hidden. It cannot be selected.
		/// </summary>
		public override bool Hidden { get; set; }
		/// <summary>
		/// Item is a separator. <see cref="Text"/>, if any, is shown center aligned.
		/// </summary>
		public override bool IsSeparator { get; set; }
		/// <summary>
		/// Any user data attached to the item.
		/// </summary>
		public override object Data { get; set; }
		/// <summary>
		/// Called when a menu item is clicked.
		/// </summary>
		public override EventHandler<MenuEventArgs> Click { get; set; }
	}

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
	/// It provides incremental filtering with various options.
	/// <para>
	/// Keys:<br/>
	/// [CtrlC], [CtrlIns] - copy text of the current item to the clipboard.<br/>
	/// [CtrlDown] - this is a default key to open a permanent filter input box.<br/>
	/// [Backspace] - remove the last symbol from the incremental filter string (until the initial part is reached, if any).<br/>
	/// [ShiftBackspace] - remove the incremental filter string completely, even initial part (rarely needed, but there are some cases).<br/>
	/// </para>
	/// </remarks>
	public interface IListMenu : IAnyMenu
	{
		/// <summary>
		/// Gets or sets the incremental filter and related options.
		/// </summary>
		/// <remarks>
		/// Incremental filter mode disables hotkey highlighting and all related options.
		/// </remarks>
		PatternOptions IncrementalOptions { get; set; }
		/// <summary>
		/// Gets or sets the predefined incremental filter pattern used to continue typing.
		/// </summary>
		/// <remarks>
		/// It is not used to filter the initial list, initial list contains all items.
		/// <para>
		/// It does not enable filter itself, you have to set <see cref="IncrementalOptions"/>.
		/// </para>
		/// <para>
		/// In 'prefix' mode it is sometimes iseful to add '*' to the end of the initial pattern,
		/// as if it is already typed to filter with wildcard (it can be 'undone' by backspace).
		/// </para>
		/// </remarks>
		string Incremental { get; set; }
		/// <summary>
		/// Tells to select a single item or nothing automatically on less than two items.
		/// </summary>
		bool AutoSelect { get; set; }
		/// <summary>
		/// Tells to not show item count information at the bottom line.
		/// </summary>
		bool NoInfo { get; set; }
		/// <summary>
		/// Tells to not show the dialog shadow.
		/// </summary>
		bool NoShadow { get; set; }
		/// <summary>
		/// Gets or sets the screen margin size.
		/// </summary>
		int ScreenMargin { get; set; }
		/// <summary>
		/// Tells to use usual Far menu margins.
		/// </summary>
		bool UsualMargins { get; set; }
	}
}
