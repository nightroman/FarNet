/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Diagnostics;

namespace FarNet
{
	/// <summary>
	/// Any viewer operator. Exposed as <see cref="IFar.AnyViewer"/>.
	/// </summary>
	public interface IAnyViewer
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
		event EventHandler Closed; // [_100117_101226]
		/// <summary>
		/// Called when a file is opened in the viewer.
		/// </summary>
		/// <remarks>
		/// This event can be called more than once for the same viewer instance,
		/// e.g. on [Add], [Subtract] keys.
		/// </remarks>
		event EventHandler Opened;
		/// <summary>
		/// Called when the viewer window has got focus.
		/// </summary>
		event EventHandler GotFocus;
		/// <summary>
		/// Called when the viewer window is losing focus.
		/// </summary>
		event EventHandler LosingFocus;
		/// <summary>
		/// Opens a viewer to view some text.
		/// </summary>
		void ViewText(string text, string title, OpenMode mode);
	}

	/// <summary>
	/// Viewer operator. Exposed as <see cref="IFar.Viewer"/>. Created by <see cref="IFar.CreateViewer"/>.
	/// </summary>
	/// <remarks>
	/// Normally this object should be created or requested, used instantly and never kept for future use.
	/// When you need the current viewer operator next time call <see cref="IFar.Viewer"/> again to get it.
	/// </remarks>
	public interface IViewer : IAnyViewer
	{
		/// <summary>
		/// Gets the internal identifier.
		/// </summary>
		int Id { get; }
		/// <summary>
		/// Gets or sets the option to delete the source file on exit.
		/// </summary>
		DeleteSource DeleteSource { get; set; }
		/// <summary>
		/// Tells how editor\viewer switching should work on [F6].
		/// Set it before opening.
		/// </summary>
		Switching Switching { get; set; }
		/// <summary>
		/// Tells to not use history.
		/// Set it before opening.
		/// </summary>
		bool DisableHistory { get; set; }
		/// <summary>
		/// Name of a file being viewed. Set it before opening.
		/// On opening it can be corrected, e.g. converted into full path.
		/// </summary>
		string FileName { get; set; }
		/// <summary>
		/// Gets or sets the code page identifier.
		/// </summary>
		/// <remarks>
		/// Before opening it sets encoding for reading a file.
		/// After opening it only gets the current encoding.
		/// </remarks>
		int CodePage { get; set; }
		/// <summary>
		/// Gets or sets the start window place.
		/// Set it before opening.
		/// </summary>
		Place Window { get; set; }
		/// <summary>
		/// Gets the current window size.
		/// </summary>
		Point WindowSize { get; }
		/// <summary>
		/// Gets or sets the window title.
		/// Set it before opening.
		/// </summary>
		string Title { get; set; }
		/// <summary>
		/// Opens the viewer.
		/// </summary>
		/// <remarks>
		/// It is the same as <see cref="Open(OpenMode)"/> with open mode <see cref="OpenMode.None"/>.
		/// See remarks there.
		/// </remarks>
		void Open();
		/// <summary>
		/// Opens the viewer.
		/// </summary>
		/// <remarks>
		/// To open a viewer you should create a viewer operator by <see cref="IFar.CreateViewer"/>,
		/// set at least its <see cref="FileName"/> and optionally: <see cref="DeleteSource"/>,
		/// <see cref="DisableHistory"/>, <see cref="Switching"/>, <see cref="Title"/>, and
		/// <see cref="Window"/>. Then this method is called.
		/// </remarks>
		void Open(OpenMode mode);
		/// <summary>
		/// Gets the current file size, in symbols, not in bytes.
		/// </summary>
		long FileSize { get; }
		/// <summary>
		/// Gets the current view frame.
		/// </summary>
		ViewFrame Frame { get; set; }
		/// <summary>
		/// Sets the current view frame.
		/// </summary>
		/// <param name="pos">New file position (depends on options).</param>
		/// <param name="left">New left position.</param>
		/// <param name="options">Options.</param>
		/// <returns>New actual position.</returns>
		long SetFrame(long pos, int left, ViewFrameOptions options);
		/// <summary>
		/// Closes the current viewer window.
		/// </summary>
		void Close();
		/// <summary>
		/// Redraws the current viewer window.
		/// </summary>
		void Redraw();
		/// <summary>
		/// Selects the block in the current viewer.
		/// </summary>
		/// <param name="symbolStart">Selection start in charactes, not in bytes.</param>
		/// <param name="symbolCount">Selected character count.</param>
		void Select(long symbolStart, int symbolCount);
		/// <summary>
		/// Gets or sets the hexadecimal mode in the current viewer (~ [F4]).
		/// </summary>
		bool HexMode { get; set; }
		/// <summary>
		/// Gets or sets the wrap mode in the current editor (~ [F2]).
		/// </summary>
		bool WrapMode { get; set; }
		/// <summary>
		/// Gets or sets the word wrap mode in the current editor (~ [ShiftF2]).
		/// </summary>
		bool WordWrapMode { get; set; }
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
		/// Position is defined in percents.
		/// </summary>
		Percent = 2,
		/// <summary>
		/// Position is relative to the current (and can be negative).
		/// </summary>
		Relative = 4
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
		Modal
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
		UnusedFolder
	}

	/// <summary>
	/// Viewer frame info.
	/// </summary>
	public struct ViewFrame
	{
		///
		public ViewFrame(long pos, long left)
		{
			_pos = pos;
			_leftPos = left;
		}
		/// <summary>
		/// Position in the file.
		/// </summary>
		public long Pos { get { return _pos; } set { _pos = value; } }
		long _pos;
		/// <summary>
		/// Leftmost visible position of the text on the screen.
		/// </summary>
		public long LeftPos { get { return _leftPos; } set { _leftPos = value; } }
		long _leftPos;
		///
		public static bool operator ==(ViewFrame left, ViewFrame right)
		{
			return
				left._pos == right._pos &&
				left._leftPos == right._leftPos;
		}
		///
		public static bool operator !=(ViewFrame left, ViewFrame right)
		{
			return !(left == right);
		}
		///
		public override bool Equals(Object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;
			ViewFrame that = (ViewFrame)obj;
			return this == that;
		}
		///
		public override string ToString()
		{
			return "(" + _pos + ", " + _leftPos + ")";
		}
		///
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
