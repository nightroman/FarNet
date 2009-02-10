/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

using System.Diagnostics;
using System;

namespace FarNet
{
	/// <summary>
	/// Viewer base interface. Exposed as <see cref="IFar.AnyViewer"/>.
	/// </summary>
	public interface IAnyViewer
	{
		/// <summary>
		/// A file is opened in the viewer.
		/// </summary>
		/// <remarks>
		/// This event can be triggered more than once for the same viewer instance,
		/// e.g. by 'Add', 'Subtract' keys.
		/// </remarks>
		event EventHandler Opened;
		/// <summary>
		/// Viewer is closed. Don't operate on it, it has really gone.
		/// </summary>
		/// <remarks>
		/// This event is triggered once for the viewer instance,
		/// even if there were several files opened in it.
		/// </remarks>
		event EventHandler Closed;
		/// <summary>
		/// Viewer window has got focus. FAR 1.71.2406
		/// </summary>
		event EventHandler GotFocus;
		/// <summary>
		/// Viewer window is losing focus. FAR 1.71.2406
		/// </summary>
		event EventHandler LosingFocus;
		/// <summary>
		/// Opens a viewer to view some text.
		/// </summary>
		void ViewText(string text, string title, OpenMode mode);
	}

	/// <summary>
	/// Viewer interface. Exposed as <see cref="IFar.Viewer"/>. Created by <see cref="IFar.CreateViewer"/>.
	/// </summary>
	/// <remarks>
	/// Normally this object should be created or requested, used instantly and never kept for future use.
	/// When you need the current viewer instance next time call <see cref="IFar.Viewer"/> again to get it.
	/// </remarks>
	public interface IViewer : IAnyViewer
	{
		/// <summary>
		/// Internal ID.
		/// </summary>
		int Id { get; }
		/// <summary>
		/// Option to delete a source file when the viewer is closed.
		/// </summary>
		DeleteSource DeleteSource { get; set; }
		/// <summary>
		/// Switching between editor and viewer. Set it before opening.
		/// </summary>
		Switching Switching { get; set; }
		/// <summary>
		/// Do not use viewer history. Set it before opening.
		/// </summary>
		bool DisableHistory { get; set; }
		/// <summary>
		/// Name of a file being viewed. Set it before opening.
		/// On opening it can be corrected, e.g. converted into full path.
		/// </summary>
		string FileName { get; set; }
		/// <summary>
		/// Code page identifier. Set it before opening.
		/// </summary>
		int CodePage { get; set; }
		/// <summary>
		/// Window start position. Set it before opening.
		/// </summary>
		Place Window { get; set; }
		/// <summary>
		/// Current viewer window size.
		/// </summary>
		Point WindowSize { get; }
		/// <summary>
		/// Window title. Set it before opening.
		/// </summary>
		string Title { get; set; }
		/// <summary>
		/// Opens the viewer using properties:
		/// <see cref="DeleteSource"/>
		/// <see cref="DisableHistory"/>
		/// <see cref="Switching"/>
		/// <see cref="FileName"/>
		/// <see cref="Title"/>
		/// <see cref="Window"/>
		/// </summary>
		void Open(OpenMode mode);
		/// <summary>
		/// See <see cref="Open(OpenMode)"/> with <see cref="OpenMode.None"/>.
		/// </summary>
		void Open();
		/// <summary>
		/// File size.
		/// </summary>
		long FileSize { get; }
		/// <summary>
		/// View frame.
		/// </summary>
		ViewFrame Frame { get; set; }
		/// <summary>
		/// Sets new viewer frame.
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
		/// Sets selected block.
		/// </summary>
		/// <param name="symbolStart">Selection start in charactes, not in bytes.</param>
		/// <param name="symbolCount">Selected character count.</param>
		void Select(long symbolStart, int symbolCount);
		/// <summary>
		/// Hexadecimal mode.
		/// </summary>
		bool HexMode { get; set; }
		/// <summary>
		/// Wrap mode. 
		/// </summary>
		bool WrapMode { get; set; }
		/// <summary>
		/// Word wrap mode. 
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
	/// Options to delete a file when a viewer or editor is closed.
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
		/// Delete a file if it was not used. The file is "used" if:
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
	[DebuggerStepThroughAttribute]
	public struct ViewFrame
	{
		///
		public ViewFrame(long pos, int left)
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
		public int LeftPos { get { return _leftPos; } set { _leftPos = value; } }
		int _leftPos;
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
