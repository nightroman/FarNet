using System.Collections.Generic;
using System;

namespace FarManager
{
	/// <summary>
	/// Interface of any editor.
	/// </summary>
	public interface IAnyEditor
	{
		/// <summary>
		/// Redraw event.
		/// </summary>
		event EventHandler<RedrawEventArgs> OnRedraw;
		/// <summary>
		/// Fire <see cref="OnRedraw"/>.
		/// </summary>
		void FireOnRedraw(IEditor sender, int mode);

		/// <summary>
		/// Editor is opened event.
		/// </summary>
		event EventHandler AfterOpen;
		/// <summary>
		/// Fire <see cref="AfterOpen"/>.
		/// </summary>
		void FireAfterOpen(IEditor sender);

		/// <summary>
		/// Event is fired before editor contents is saved.
		/// </summary>
		event EventHandler BeforeSave;
		/// <summary>
		/// Fire <see cref="BeforeSave"/>.
		/// </summary>
		void FireBeforeSave(IEditor sender);

		/// <summary>
		/// Event is fired when the editor is closed.
		/// </summary>
		event EventHandler AfterClose;
		/// <summary>
		/// Fire <see cref="AfterClose"/>.
		/// </summary>
		void FireAfterClose(IEditor sender);

		/// <summary>
		/// Key pressed in editor.
		/// </summary>
		event EventHandler<KeyEventArgs> OnKey;
		/// <summary>
		/// Fire <see cref="OnKey"/>.
		/// </summary>
		void FireOnKey(IEditor sender, Key key);

		/// <summary>
		/// Mouse state is changed.
		/// </summary>
		event EventHandler<MouseEventArgs> OnMouse;
		/// <summary>
		/// Fire <see cref="OnMouse"/>.
		/// </summary>
		void FireOnMouse(IEditor sender, Mouse mouse);
	}

	/// <summary>
	/// Far Editor interface.
	/// </summary>
	public interface IEditor : IAnyEditor
	{
		/// <summary>
		/// Internal editor identifier.
		/// </summary>
		int Id { get; }
		/// <summary>
		/// Size of tab symbol in spaces.
		/// Only for opened and current editor.
		/// </summary>
		int TabSize { get; set; }
		/// <summary>
		/// Expand tabs mode.
		/// Only for opened and current editor.
		/// </summary>
		ExpandTabsMode ExpandTabs { get; set; }
		/// <summary>
		/// Returns control to the calling function immediately after <see cref="Open"/>.
		/// If false continues when a user has closed the editor.
		/// It is read only when the editor is opened.
		/// </summary>
		bool Async { get; set; }
		/// <summary>
		/// Delete a directory with a file when it is closed.
		/// It is read only when an editor is opened.
		/// </summary>
		/// <seealso cref="DeleteOnlyFileOnClose"/>
		bool DeleteOnClose { get; set; }
		/// <summary>
		/// Delete a file when it is closed.
		/// It is read only when an editor is opened.
		/// </summary>
		/// <seealso cref="DeleteOnClose"/>
		bool DeleteOnlyFileOnClose { get; set; }
		/// <summary>
		/// Enable switching to viewer.
		/// It is read only when an editor is opened.
		/// </summary>
		bool EnableSwitch { get; set; }
		/// <summary>
		/// Do not use editor history.
		/// It is read only when an editor is opened.
		/// </summary>
		bool DisableHistory { get; set; }
		/// <summary>
		/// Word delimiters specific to the current editor.
		/// Only for opened and current editor.
		/// </summary>
		/// <seealso cref="IFar.WordDiv"/>
		string WordDiv { get; set; }
		/// <summary>
		/// Editor lines.
		/// Only for opened and current editor.
		/// </summary>
		ILines Lines { get; }
		/// <summary>
		/// Name of a file being edited.
		/// It is read only when an editor is opened.
		/// </summary>
		string FileName { get; set; }
		/// <summary>
		/// Editor window position.
		/// </summary>
		IRect Window { get; set; }
		/// <summary>
		/// Controls the current cursor position.
		/// Normally it is for opened and current editor.
		/// </summary>
		ICursor Cursor { get; }
		/// <summary>
		/// Current selection. It is a collection <see cref="ILines"/> of selected line parts and a few extra members.
		/// Only for opened and current editor.
		/// </summary>
		/// <seealso cref="ISelection.Type"/>
		ISelection Selection { get; }
		/// <summary>
		/// Disable switching to other windows.
		/// It is read only when an editor is opened.
		/// </summary>
		bool IsModal { get; set; }
		/// <summary>
		/// Open a new (non-existing) file in the editor, similar to pressing Shift-F4 in FAR. 
		/// It is read only when an editor is opened.
		/// </summary>
		bool IsNew { get; set; }
		/// <summary>
		/// Insert text.
		/// The text is processed in the same way as it it had been entered from the keyboard.
		/// Only for opened and current editor.
		/// </summary>
		/// <param name="text">The text. Supported EOL: CR, LF, CR+LF.</param>
		void Insert(string text);
		/// <summary>
		/// Redraw editor.
		/// Only for opened and current editor.
		/// </summary>
		void Redraw();
		/// <summary>
		/// Delete a character under <see cref="Cursor"/>.
		/// Only for opened and current editor.
		/// </summary>
		void DeleteChar();
		/// <summary>
		/// Delete a line under <see cref="Cursor"/>.
		/// Only for opened and current editor.
		/// </summary>
		void DeleteLine();
		/// <summary>
		/// Close the editor.
		/// Only for opened and current editor.
		/// </summary>
		void Close();
		/// <summary>
		/// Save the file being edited. Exception on failure.
		/// Only for opened and current editor.
		/// </summary>
		void Save();
		/// <summary>
		/// Save the file being edited to <paramref name="fileName"/>. Exception on failure.
		/// Only for opened and current editor.
		/// </summary>
		/// <param name="fileName">File name to save to.</param>
		void Save(string fileName);
		/// <summary>
		/// Inserts a new line at the current <see cref="Cursor"/> position
		/// and moves the cursor to the first position in the new line.
		/// Only for opened and current editor.
		/// </summary>
		void InsertLine();
		/// <summary>
		/// Inserts a new line at the current <see cref="Cursor"/> position
		/// and moves the cursor to the first position in the new line or to the indented position.
		/// The indent behaviour is the same as on pressing Enter in the editor.
		/// Only for opened and current editor.
		/// </summary>
		/// <param name="indent">Insert a line with indent.</param>
		void InsertLine(bool indent);
		/// <summary>
		/// Open an editor using properties:
		/// <see cref="FileName"/>,
		/// <see cref="Title"/>,
		/// <see cref="Async"/>,
		/// <see cref="DeleteOnClose"/>,
		/// <see cref="DeleteOnlyFileOnClose"/>,
		/// <see cref="DisableHistory"/>,
		/// <see cref="EnableSwitch"/>,
		/// <see cref="IsModal"/>,
		/// <see cref="IsNew"/>.
		/// </summary>
		void Open();
		/// <summary>
		/// Is this editor opened?
		/// </summary>
		bool IsOpened { get; }
		/// <summary>
		/// Editor window title. Set it before opening (standard title) or after opening (temporary title).
		/// When an editor is opened the standard title will be automatically restored after the plugin has finished processing.
		/// </summary>
		string Title { get; set; }
		/// <summary>
		/// Gets bookmarks in the current editor.
		/// Each bookmark is defined as ICursor. Negative Line means undefined bookmark.
		/// To go to a bookmark use <see cref="Cursor"/>.<see cref="ICursor.Assign"/>.
		/// </summary>
		ICollection<ICursor> Bookmarks();
		/// <summary>
		/// Overtype mode.
		/// Only for opened and current editor.
		/// </summary>
		bool Overtype { get; set; }
		/// <summary>
		/// Is the file modified?
		/// Only for opened and current editor.
		/// </summary>
		bool IsModified { get; }
		/// <summary>
		/// Is the file saved?
		/// Only for opened and current editor.
		/// </summary>
		bool IsSaved { get; }
		/// <summary>
		/// Is the file locked (Ctrl-L)?
		/// Only for opened and current editor.
		/// </summary>
		bool IsLocked { get; }
	}

	/// <summary>
	/// Arguments of <see cref="IAnyEditor.OnRedraw"/> event.
	/// </summary>
	public class RedrawEventArgs : EventArgs
	{
		int _mode;
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mode">See <see cref="Mode"/>.</param>
		public RedrawEventArgs(int mode)
		{
			_mode = mode;
		}
		/// <summary>
		/// Parameter of Far EE_REDRAW event, see Far API, ProcessEditorEvent.
		/// </summary>
		public int Mode
		{
			get { return _mode; }
		}
	}

	/// <summary>
	/// List of strings.
	/// </summary>
	/// <remarks>
	/// Usually the list is internally connected to <see cref="ILines"/>,
	/// thus, strings in the list are dependent on this source; i.e.
	/// *) if it is <see cref="ILines.Text"/> from <see cref="IEditor.Lines"/> then strings are lines of editor
	/// and list operations Add, Clear, Insert, RemoveAt and etc. affect editor text;
	/// *) if it is <see cref="ILines.Text"/> from <see cref="IEditor.Selection"/> then strings are selected line parts
	/// and list operations affect only selected text.
	/// </remarks>
	public interface IStrings : IList<string>
	{
		/// <summary>
		/// Strings concatenated with CRLF
		/// </summary>
		string Text { get; set; }
	}

	/// <summary>
	/// Selection of <see cref="ILine">editor line</see>
	/// </summary>
	public interface ILineSelection
	{
		/// <summary>
		/// Text of selection. If line doesn't contain selection it is null.
		/// </summary>
		string Text { get; set; }
		/// <summary>
		/// Start position of selection in the line. If line doesn't contain selection it is -1.
		/// </summary>
		int Start { get; }
		/// <summary>
		/// End position of selection in the line. If selection includes the EOL sequence this field has a value of -1.
		/// </summary>
		int End { get; }
		/// <summary>
		/// Selection length. If line doesn't contain selection it is -1.
		/// </summary>
		int Length { get; }
	}

	/// <summary>
	/// Line in <see cref="IEditor.Lines"/> or <see cref="IEditor.Selection"/> from <see cref="IEditor"/>.
	/// </summary>
	public interface ILine
	{
		/// <summary>
		/// Line number in source <see cref="IEditor"/>.
		/// </summary>
		int No { get; }
		/// <summary>
		/// Line text or selected text depending on a source (<see cref="IEditor.Lines"/> or <see cref="IEditor.Selection"/>).
		/// </summary>
		string Text { get; set; }
		/// <summary>
		/// End-of-line characters.
		/// </summary>
		string Eol { get; set; }
		/// <summary>
		/// Selected part of the line.
		/// </summary>
		ILineSelection Selection { get; }
	}

	/// <summary>
	/// List of lines
	/// </summary>
	public interface ILines : IList<ILine>
	{
		/// <summary>
		/// First line. Note that it always exists in editor.
		/// </summary>
		ILine First { get; }
		/// <summary>
		/// Last line. Note that it always exists in editor.
		/// </summary>
		ILine Last { get; }
		/// <summary>
		/// All text as string.
		/// </summary>
		string Text { get; set; }
		/// <summary>
		/// Text as string list, see <see cref="IStrings"/>.
		/// </summary>
		IStrings Strings { get; }
		/// <summary>
		/// Add a string to the end of the list.
		/// </summary>
		void Add(string item);
		/// <summary>
		/// Insert a string as a new line with specified line index.
		/// </summary>
		void Insert(int index, string item);
	}

	/// <summary>
	/// <see cref="IEditor">Editor</see> cursor position.
	/// </summary>
	public interface ICursor
	{
		/// <summary>
		/// Line index (starting with 0).
		/// </summary>
		int Line { get; set; }
		/// <summary>
		/// Position in the string (i.e. in chars).
		/// </summary>
		int Pos { get; set; }
		/// <summary>
		/// Position on the screen (i.e. in columns).
		/// </summary>
		int TabPos { get; set; }
		/// <summary>
		/// First visible line index.
		/// </summary>
		int TopLine { get; set; }
		/// <summary>
		/// Leftmost visible position of the text on the screen.
		/// </summary>
		int LeftPos { get; set; }
		/// <summary>
		/// Assign another cursor position.
		/// </summary>
		/// <param name="cursor">Another cursor</param>
		void Assign(ICursor cursor);
		/// <summary>
		/// Sets both line and position.
		/// </summary>
		/// <param name="pos">Position.</param>
		/// <param name="line">Line.</param>
		void Set(int pos, int line);
	}

	/// <summary>
	/// Arguments of <see cref="IAnyEditor.OnKey"/> event.
	/// </summary>
	public class KeyEventArgs : EventArgs
	{
		Key _key;
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="key">Key data.</param>
		public KeyEventArgs(Key key)
		{
			_key = key;
		}
		/// <summary>
		/// Key data.
		/// </summary>
		public Key Key
		{
			get { return _key; }
		}
	}

	/// <summary>
	/// Arguments of <see cref="IAnyEditor.OnMouse"/> event.
	/// </summary>
	public class MouseEventArgs : EventArgs
	{
		Mouse _mouse;
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mouse">Mouse data.</param>
		public MouseEventArgs(Mouse mouse)
		{
			_mouse = mouse;
		}
		/// <summary>
		/// Mouse data.
		/// </summary>
		public Mouse Mouse
		{
			get { return _mouse; }
		}
	}

	/// <summary>
	/// Expand tabs mode
	/// </summary>
	public enum ExpandTabsMode
	{
		/// <summary>
		/// Tabs are not replaced with spaces.
		/// </summary>
		None,
		/// <summary>
		/// All tabs are replaced with spaces.
		/// </summary>
		All,
		/// <summary>
		/// Only new tabs are replaced with spaces.
		/// </summary>
		New
	}
}
