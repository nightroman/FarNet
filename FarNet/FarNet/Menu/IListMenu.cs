
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet
{
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
		/// Gets or sets the screen margin size.
		/// </summary>
		int ScreenMargin { get; set; }

		/// <summary>
		/// Tells to use usual Far menu margins.
		/// </summary>
		bool UsualMargins { get; set; }

		/// <summary>
		/// Gets or sets the dialog type ID.
		/// </summary>
		Guid TypeId { get; set; }
	}
}
