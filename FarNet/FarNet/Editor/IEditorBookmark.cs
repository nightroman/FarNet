
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;

namespace FarNet;

/// <summary>
/// Editor bookmark operator.
/// </summary>
/// <remarks>
/// It is exposed as <see cref="IEditor.Bookmark"/>.
/// It operates on standard (permanent) and stack (temporary) bookmarks in the current editor.
/// </remarks>
public abstract class IEditorBookmark
{
	/// <summary>
	/// Gets permanent bookmarks in the current editor.
	/// </summary>
	/// <remarks>
	/// Bookmarks are defined as <see cref="TextFrame"/>.
	/// Negative <see cref="TextFrame.CaretLine"/> means undefined bookmark.
	/// To go to a bookmark set the editor <see cref="IEditor.Frame"/>.
	/// </remarks>
	public abstract ICollection<TextFrame> Bookmarks();

	/// <summary>
	/// Gets session bookmarks in the current editor.
	/// </summary>
	/// <remarks>
	/// Bookmarks are defined as <see cref="TextFrame"/>.
	/// To go to a bookmark set the editor <see cref="IEditor.Frame"/>.
	/// </remarks>
	public abstract ICollection<TextFrame> SessionBookmarks();

	/// <summary>
	/// Adds a new stack bookmark at the current bookmark stack position.
	/// </summary>
	/// <remarks>
	/// Bookmarks after the current position, if any, are removed.
	/// </remarks>
	public abstract void AddSessionBookmark();

	/// <summary>
	/// Clears the bookmark stack.
	/// </summary>
	public abstract void ClearSessionBookmarks();

	/// <summary>
	/// Removes the specified stack bookmark.
	/// </summary>
	/// <param name="index">Bookmark index or -1 for the current stack position.</param>
	public abstract void RemoveSessionBookmarkAt(int index);

	/// <summary>
	/// Navigates to the next stack bookmark, if any.
	/// </summary>
	public abstract void GoToNextSessionBookmark();

	/// <summary>
	/// Navigates to the previous stack bookmark, if any.
	/// </summary>
	public abstract void GoToPreviousSessionBookmark();
}
