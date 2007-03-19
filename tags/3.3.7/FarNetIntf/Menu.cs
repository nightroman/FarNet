using System.Collections.Generic;
using System;

namespace FarManager
{
	/// <summary>
	/// Menu. Contains settings and menu item collection. It is created by <see cref="IFar.CreateMenu"/>.
	/// </summary>
	public interface IMenu : IDisposable
	{
		/// <summary>
		/// Event is raised when menu item is clicked.
		/// </summary>
		event EventHandler OnClick;
		/// <summary>
		/// X-position.
		/// </summary>
		int X { get; set; }
		/// <summary>
		/// Y-position.
		/// </summary>
		int Y { get; set; }
		/// <summary>
		/// Maximal height.
		/// </summary>
		int MaxHeight { get; set; }
		/// <summary>
		/// Title of menu.
		/// </summary>
		string Title { get; set; }
		/// <summary>
		/// Bottom line text.
		/// </summary>
		string Bottom { get; set; }
		/// <summary>
		/// List of virtual key codes that closes menu (see VK_* in Far API docs).
		/// </summary>
		IList<int> BreakKeys { get; }
		/// <summary>
		/// Menu items.
		/// </summary>
		IMenuItems Items { get; }
		/// <summary>
		/// Before <see cref="Show"/> tells to select this item;
		/// After <see cref="Show"/> returns the selected item index or -1 if nothing is selected.
		/// </summary>
		int Selected { get; set; }
		/// <summary>
		/// User data attached to the <see cref="Selected"/> menu item or null if nothing is selected.
		/// </summary>
		object SelectedData { get; }
		/// <summary>
		/// <see cref="BreakKeys"/> index of key interrupted menu.
		/// </summary>
		int BreakCode { get; }
		/// <summary>
		/// Show ampersands in menu items or used as accelerator character.
		/// </summary>
		bool ShowAmpersands { get; set; }
		/// <summary>
		/// Cursor will go to upper position if is is at downmost position and down is pressed.
		/// </summary>
		bool WrapCursor { get; set; }
		/// <summary>
		/// Assign hotkeys automatically.
		/// </summary>
		bool AutoAssignHotkeys { get; set; }
		/// <summary>
		/// Assign hotkeys automatically from bottom.
		/// </summary>
		bool ReverseAutoAssign { get; set; }
		/// <summary>
		/// Show menu.
		/// </summary>
		/// <returns>true if a menu item was selected, otherwise cancelled</returns>
		bool Show();
		/// <summary>
		/// Creates low level internal data of the menu from the current items.
		/// It is used for multiple calls of Show() with the same item set.
		/// <see cref="Unlock"/> has to be called to change items again.
		/// </summary>
		void Lock();
		/// <summary>
		/// Destroys internal data created by <see cref="Lock"/>.
		/// Items can be changed if the menu is still used.
		/// </summary>
		void Unlock();
		/// <include file='doc.xml' path='docs/pp[@name="HelpTopic"]/*'/>
		string HelpTopic { get; set; }
		/// <summary>
		/// Tells to select the last item on <see cref="Show()"/>.
		/// </summary>
		bool SelectLast { get; set; }
		/// <summary>
		/// Filter string.
		/// Format: [regex] or [*substring].
		/// It is used by filter input box enabled by <see cref="FilterKey"/>.
		/// If it is null and <see cref="FilterHistory"/> and <see cref="FilterRestore"/> are set it is restored from history.
		/// Don't use it if you use <see cref="Lock"/>.
		/// </summary>
		string Filter { get; set; }
		/// <summary>
		/// Filter history used by filter input box.
		/// Don't use it if you use <see cref="Lock"/>.
		/// </summary>
		string FilterHistory { get; set; }
		/// <summary>
		/// Restore filter from history if it is null and <see cref="FilterHistory"/> is set.
		/// Don't use it if you use <see cref="Lock"/>.
		/// </summary>
		bool FilterRestore { get; set; }
		/// <summary>
		/// Virtual key code of a key opening a filter input box.
		/// It is added to other break keys automatically.
		/// Don't use it if you use <see cref="Lock"/>.
		/// </summary>
		int FilterKey { get; set; }
	}

	/// <summary>
	/// Item of <see cref="IMenu.Items"/> in a menu.
	/// </summary>
	public interface IMenuItem
	{
		/// <summary>
		/// Text of menu item.
		/// </summary>
		string Text { get; set; }
		/// <summary>
		/// Is menu item checked?
		/// </summary>
		bool Checked { get; set; }
		/// <summary>
		/// Is menu item separator?
		/// </summary>
		bool IsSeparator { get; set; }
		/// <summary>
		/// Any user data attached to the menu item. Used by <see cref="IMenu.SelectedData"/>.
		/// </summary>
		object Data { get; set; }
		/// <summary>
		/// Event raised when menu item is clicked.
		/// </summary>
		event EventHandler OnClick;
		/// <summary>
		/// Fires <see cref="OnClick"/>.
		/// </summary>
		void FireOnClick();
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
}
