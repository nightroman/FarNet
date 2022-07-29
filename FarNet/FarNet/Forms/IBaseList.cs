
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;

namespace FarNet.Forms;

/// <summary>
/// Base interface for <see cref="IComboBox"/> and <see cref="IListBox"/>.
/// </summary>
public interface IBaseList : IControl
{
	/// <summary>
	/// Adds and returns a new item.
	/// </summary>
	/// <param name="text">Item text.</param>
	/// <remarks>
	/// This is the simplest way to setup items before opening a dialog.
	/// After opening it is better to create and add items directly to <see cref="Items"/>.
	/// </remarks>
	FarItem Add(string text);

	/// <summary>
	/// Gets or sets the selected item index.
	/// </summary>
	//! The name is not ideal but consistent with many others.
	int Selected { get; set; }

	/// <include file='doc.xml' path='doc/AutoAssignHotkeys/*'/>
	bool AutoAssignHotkeys { get; set; }

	/// <summary>
	/// Tells to not show ampersand symbols and use them as hotkey marks.
	/// </summary>
	bool NoAmpersands { get; set; }

	/// <include file='doc.xml' path='doc/WrapCursor/*'/>
	bool WrapCursor { get; set; }

	/// <include file='doc.xml' path='doc/NoFocus/*'/>
	bool NoFocus { get; set; }

	/// <summary>
	/// Tells to not close the dialog on item selection.
	/// </summary>
	bool NoClose { get; set; }

	/// <summary>
	/// Tells to select the last item if <see cref="Selected"/> is not set.
	/// </summary>
	bool SelectLast { get; set; }

	/// <summary>
	/// Attaches previously detached items.
	/// </summary>
	/// <seealso cref="Items"/>
	void AttachItems();

	/// <summary>
	/// Detaches the items before large changes for better performance.
	/// You have to call <see cref="AttachItems"/> when changes are done.
	/// <seealso cref="Items"/>
	/// </summary>
	void DetachItems();

	/// <include file='doc.xml' path='doc/BaseListItems/*'/>
	IList<FarItem> Items { get; }

	/// <summary>
	/// INTERNAL
	/// </summary>
	/// <param name="items">.</param>
	/// <param name="subset">.</param>
	void ReplaceItems(IList<FarItem> items, IList<int> subset);
}
