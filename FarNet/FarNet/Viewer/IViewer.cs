
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet;

/// <summary>
/// Viewer operator. Exposed as <see cref="IFar.Viewer"/>. Created by <see cref="IFar.CreateViewer"/>.
/// </summary>
/// <remarks>
/// Normally this object should be created or requested, used instantly and never kept for future use.
/// When you need the current viewer operator next time call <see cref="IFar.Viewer"/> again to get it.
/// </remarks>
public abstract class IViewer : IViewerBase
{
	/// <summary>
	/// Gets the internal identifier.
	/// </summary>
	public abstract IntPtr Id { get; }

	/// <summary>
	/// Gets or sets the option to delete the source file on exit.
	/// </summary>
	public abstract DeleteSource DeleteSource { get; set; }

	/// <summary>
	/// Tells how editor\viewer switching should work on [F6].
	/// Set it before opening.
	/// </summary>
	public abstract Switching Switching { get; set; }

	/// <summary>
	/// Tells to not use history.
	/// Set it before opening.
	/// </summary>
	public abstract bool DisableHistory { get; set; }

	/// <summary>
	/// Name of a file being viewed. Set it before opening.
	/// On opening it can be corrected, e.g. converted into full path.
	/// </summary>
	public abstract string FileName { get; set; }

	/// <summary>
	/// Gets or sets the code page identifier.
	/// </summary>
	/// <remarks>
	/// Before opening it sets encoding for reading a file.
	/// After opening it only gets the current encoding.
	/// </remarks>
	public abstract int CodePage { get; set; }

	/// <summary>
	/// Gets or sets the start window place.
	/// Set it before opening.
	/// </summary>
	public abstract Place Window { get; set; }

	/// <summary>
	/// Gets the current window size.
	/// </summary>
	public abstract Point WindowSize { get; }

	/// <summary>
	/// Gets or sets the window title.
	/// Set it before opening.
	/// </summary>
	public abstract string? Title { get; set; }

	/// <summary>
	/// Opens the viewer.
	/// </summary>
	/// <remarks>
	/// It is the same as <see cref="Open(OpenMode)"/> with open mode <see cref="OpenMode.None"/>.
	/// See remarks there.
	/// </remarks>
	public void Open()
	{
		Open(OpenMode.None);
	}

	/// <summary>
	/// Opens the viewer.
	/// </summary>
	/// <param name="mode">The open mode.</param>
	/// <remarks>
	/// To open a viewer you should create a viewer operator by <see cref="IFar.CreateViewer"/>,
	/// set at least its <see cref="FileName"/> and optionally: <see cref="DeleteSource"/>,
	/// <see cref="DisableHistory"/>, <see cref="Switching"/>, <see cref="Title"/>, and
	/// <see cref="Window"/>. Then this method is called.
	/// </remarks>
	public abstract void Open(OpenMode mode);

	/// <summary>
	/// Gets true if the viewer is opened.
	/// </summary>
	public abstract bool IsOpened { get; }

	/// <summary>
	/// Gets the current file size, in symbols, not in bytes.
	/// </summary>
	public abstract long FileSize { get; }

	/// <summary>
	/// Gets the current view frame.
	/// </summary>
	public abstract ViewFrame Frame { get; set; }

	/// <summary>
	/// Sets the current view frame.
	/// </summary>
	/// <param name="offset">New file position (depends on options).</param>
	/// <param name="column">New left position.</param>
	/// <param name="options">Options.</param>
	/// <returns>New actual position.</returns>
	public abstract long SetFrame(long offset, int column, ViewFrameOptions options);

	/// <summary>
	/// Closes the current viewer window.
	/// </summary>
	public abstract void Close();

	/// <summary>
	/// Redraws the current viewer window.
	/// </summary>
	public abstract void Redraw();

	/// <summary>
	/// Selects the block of text in the current viewer.
	/// </summary>
	/// <param name="symbolStart">Selection start in charactes, not in bytes.</param>
	/// <param name="symbolCount">Selected character count.</param>
	public abstract void SelectText(long symbolStart, int symbolCount);

	/// <summary>
	/// Gets or sets the view mode in the current viewer (~ [F4]).
	/// </summary>
	public abstract ViewerViewMode ViewMode { get; set; }

	/// <summary>
	/// Gets or sets the wrap mode in the current editor (~ [F2]).
	/// </summary>
	public abstract bool WrapMode { get; set; }

	/// <summary>
	/// Gets or sets the word wrap mode in the current editor (~ [ShiftF2]).
	/// </summary>
	public abstract bool WordWrapMode { get; set; }

	/// <summary>
	/// Gets the opening time of the instance.
	/// </summary>
	public abstract DateTime TimeOfOpen { get; }

	/// <summary>
	/// Makes the window current.
	/// </summary>
	public void Activate()
	{
		var myId = Id;
		for (int i = Far.Api.Window.Count - 1; i >= 0; i--)
		{
			if (Far.Api.Window.GetIdAt(i) == myId && Far.Api.Window.GetKindAt(i) == WindowKind.Viewer)
			{
				Far.Api.Window.SetCurrentAt(i);
				return;
			}
		}
	}
}
