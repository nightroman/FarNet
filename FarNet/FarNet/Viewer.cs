
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using System.Diagnostics;

namespace FarNet
{
	/// <summary>
	/// Common viewer events.
	/// </summary>
	public abstract class IViewerBase
	{
		/// <summary>
		/// Called when the viewer is closed.
		/// </summary>
		/// <remarks>
		/// This event is called once for the viewer instance, even if there were several files opened in it,
		/// e.g. on [Add], [Subtract] keys the <see cref="Opened"/> is called every time.
		/// <para>
		/// Don't operate on the viewer, it has really gone.
		/// </para>
		/// </remarks>
		public abstract event EventHandler Closed; // [_100117_101226]
		/// <summary>
		/// Called when a file is opened in the viewer.
		/// </summary>
		/// <remarks>
		/// This event can be called more than once for the same viewer instance,
		/// e.g. on [Add], [Subtract] keys.
		/// </remarks>
		public abstract event EventHandler Opened;
		/// <summary>
		/// Called when the viewer window has got focus.
		/// </summary>
		public abstract event EventHandler GotFocus;
		/// <summary>
		/// Called when the viewer window is losing focus.
		/// </summary>
		public abstract event EventHandler LosingFocus;
	}

	/// <summary>
	/// Any viewer operator.
	/// Exposed as <see cref="IFar.AnyViewer"/>.
	/// </summary>
	public abstract class IAnyViewer : IViewerBase
	{
		/// <summary>
		/// Opens a viewer to view some text.
		/// </summary>
		/// <param name="text">The text to view.</param>
		/// <param name="title">The viewer title.</param>
		/// <param name="mode">The open mode.</param>
		public abstract void ViewText(string text, string title, OpenMode mode);
	}

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
		public abstract string Title { get; set; }
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
		/// Makes the instance window active.
		/// </summary>
		/// <remarks>It may throw if the window cannot be activated.</remarks>
		public abstract void Activate();
	}

	/// <summary>
	/// Viewer modes.
	/// </summary>
	public enum ViewerViewMode
	{
		/// <summary>
		/// Text view mode.
		/// </summary>
		Text,
		/// <summary>
		/// Hex view mode.
		/// </summary>
		Hex,
		/// <summary>
		/// Dump view mode.
		/// </summary>
		Dump
	}

	/// <summary>
	/// Options for <see cref="IViewer.SetFrame"/>.
	/// </summary>
	[Flags]
	public enum ViewFrameOptions
	{
		///
		None,
		/// <summary>
		/// Don't redraw.
		/// </summary>
		NoRedraw = 1,
		/// <summary>
		/// Offset is defined in percents.
		/// </summary>
		Percent = 2,
		/// <summary>
		/// Offset is relative to the current (and can be negative).
		/// </summary>
		Relative = 4,
	}

	/// <summary>
	/// Open modes of editor and viewer.
	/// </summary>
	public enum OpenMode
	{
		/// <summary>
		/// Tells to open not modal editor or viewer and return immediately.
		/// </summary>
		None,
		/// <summary>
		/// Tells to open not modal editor or viewer and wait for exit.
		/// </summary>
		Wait,
		/// <summary>
		/// Tells to open modal editor or viewer.
		/// </summary>
		Modal,
	}

	/// <summary>
	/// Options to delete temporary source files and empty folders after use.
	/// </summary>
	public enum DeleteSource
	{
		/// <summary>
		/// Default action: do not delete a file.
		/// </summary>
		None,
		/// <summary>
		/// Try to delete a file always. It is not recommended if editor\viewer switching is enabled (F6).
		/// You may set it at any time, i.e. before or after opening.
		/// </summary>
		File,
		/// <summary>
		/// The same as <see cref="File"/> plus delete its folder if it is empty.
		/// You may set it at any time, i.e. before or after opening.
		/// </summary>
		Folder,
		/// <summary>
		/// Delete a file if it was not used. The file is used if:
		/// *) it was saved;
		/// *) there was editor\viewer switching (F6).
		/// *) it is opened in another editor or viewer.
		/// You should set it before opening.
		/// </summary>
		UnusedFile,
		/// <summary>
		/// The same as <see cref="UnusedFile"/> plus delete its folder if it is empty.
		/// You should set it before opening.
		/// </summary>
		UnusedFolder,
	}

	/// <summary>
	/// Viewer frame info.
	/// </summary>
	public struct ViewFrame
	{
		/// <param name="offset">See <see cref="Offset"/></param>
		/// <param name="column">See <see cref="Column"/></param>
		public ViewFrame(long offset, long column)
			: this()
		{
			Offset = offset;
			Column = column;
		}
		/// <summary>
		/// Offset in the file.
		/// </summary>
		public long Offset { get; set; }
		/// <summary>
		/// Leftmost visible column index.
		/// </summary>
		public long Column { get; set; }
		/// <include file='doc.xml' path='doc/OpEqual/*'/>
		public static bool operator ==(ViewFrame left, ViewFrame right)
		{
			return
				left.Offset == right.Offset &&
				left.Column == right.Column;
		}
		/// <include file='doc.xml' path='doc/OpNotEqual/*'/>
		public static bool operator !=(ViewFrame left, ViewFrame right)
		{
			return !(left == right);
		}
		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj != null && obj.GetType() == typeof(ViewFrame) && this == (ViewFrame)obj;
		}
		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return (int)Offset | ((int)Column << 16);
		}
		/// <summary>
		/// Returns the string "(Offset, Column)".
		/// </summary>
		public override string ToString()
		{
			return "(" + Offset + ", " + Column + ")";
		}
	}

}
