/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using FarNet.Forms;

namespace FarNet
{
	/// <summary>
	/// Any editor operator, common editor events, options and tools.
	/// </summary>
	/// <remarks>
	/// Exposed as:
	/// 1) <see cref="IFar.AnyEditor"/> (common editor settings and tools);
	/// 2) <see cref="IFar.Editor"/> (the current editor instance);
	/// 3) <see cref="IFar.CreateEditor"/> (new editor to open).
	/// </remarks>
	public interface IAnyEditor
	{
		/// <summary>
		/// Called when the editor is closed.
		/// </summary>
		event EventHandler Closed;
		/// <summary>
		/// Called when the editor is opened.
		/// </summary>
		event EventHandler Opened;
		/// <summary>
		/// Called before saving.
		/// </summary>
		event EventHandler Saving;
		/// <summary>
		/// Called on redrawing.
		/// </summary>
		event EventHandler<RedrawEventArgs> OnRedraw;
		/// <summary>
		/// Called on a key pressed.
		/// </summary>
		event EventHandler<KeyEventArgs> OnKey;
		/// <summary>
		/// Called on mouse actions.
		/// </summary>
		event EventHandler<MouseEventArgs> OnMouse;
		/// <summary>
		/// Called when the editor has got focus.
		/// </summary>
		event EventHandler GotFocus;
		/// <summary>
		/// Called when the editor is losing focus.
		/// </summary>
		event EventHandler LosingFocus;
		/// <summary>
		/// Called periodically when a user is idle.
		/// </summary>
		/// <seealso cref="IdledHandler"/>
		event EventHandler Idled;
		/// <summary>
		/// Called on [CtrlC] in asynchronous mode, see <see cref="IEditor.BeginAsync"/> .
		/// </summary>
		event EventHandler CtrlCPressed;
		/// <summary>
		/// Opens a modal temporary editor to edit and return some text.
		/// </summary>
		string EditText(string text, string title);
		/// <summary>
		/// Gets or sets editor word delimiters.
		/// </summary>
		/// <remarks>
		/// For the current editor <see cref="IFar.Editor"/> operator it has read\write access.
		/// For an editor from <see cref="IFar.CreateEditor"/> you can set it before or after opening.
		/// For the <see cref="IFar.AnyEditor"/> operator it is read only and depends on Far editor options.
		/// </remarks>
		string WordDiv { get; set; } //! Name 'WordDiv' is kind of standard, e.g. it is used by Far and Colorer.
	}

	/// <summary>
	/// Editor operator. Exposed as <see cref="IFar.Editor"/>. Created by <see cref="IFar.CreateEditor"/>.
	/// </summary>
	/// <remarks>
	/// Normally this object should be created or requested, used instantly and never kept for future use.
	/// When you need the current editor operator next time call <see cref="IFar.Editor"/> again to get it.
	/// <para>
	/// In fact all dynamic members operate on the current editor, not on the editor associated with the instance.
	/// Thus, if you use an operator of not current editor then results may be unexpected.
	/// </para>
	/// </remarks>
	public interface IEditor : IAnyEditor
	{
		/// <summary>
		/// Gets the internal identifier.
		/// </summary>
		int Id { get; }
		/// <summary>
		/// Gets or sets tab size in spaces in the current editor.
		/// </summary>
		int TabSize { get; set; }
		/// <summary>
		/// Gets or sets expand tabs mode in the current editor.
		/// </summary>
		ExpandTabsMode ExpandTabs { get; set; }
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
		/// Gets the current line operator.
		/// </summary>
		/// <remarks>
		/// The returned object refers to the current line dynamically, it is not a copy;
		/// when cursor moves to another line the operator works on the new current line.
		/// </remarks>
		ILine CurrentLine { get; }
		/// <summary>
		/// Gets the list of editor lines as they are.
		/// Editor must be current.
		/// </summary>
		/// <remarks>
		/// This list always contains at least one line.
		/// </remarks>
		ILines Lines { get; }
		/// <summary>
		/// Gets the list of editor lines with no last empty line if any.
		/// Editor must be current.
		/// </summary>
		/// <remarks>
		/// The last editor line is excluded if it empty.
		/// Thus, this list is empty when an editor has no text.
		/// </remarks>
		ILines TrueLines { get; }
		/// <summary>
		/// Gets or sets the name of a file being or to be edited.
		/// Set it before opening.
		/// </summary>
		/// <remarks>
		/// Before opening it sets a file to be edited (on opening it can be changed, e.g. converted into its full path).
		/// For an opened editor it gets the file being edited.
		/// </remarks>
		string FileName { get; set; }
		/// <summary>
		/// Gets or sets the code page identifier.
		/// </summary>
		/// <remarks>
		/// Before opening it sets encoding for reading a file.
		/// After opening it gets and sets the current encoding.
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
		/// Gets the current selection operator as it is.
		/// Editor must be current.
		/// </summary>
		/// <remarks>
		/// It is a collection <see cref="ILines"/> of selected line parts and a few extra members.
		/// If selection exists (<see cref="ISelection.Exists"/>) it contains at least one line.
		/// </remarks>
		ISelection Selection { get; }
		/// <summary>
		/// Gets the current selection operator with no last empty line if any.
		/// Editor must be current.
		/// </summary>
		/// <remarks>
		/// Unlike <see cref="Selection"/> it can be empty even if selection <see cref="ISelection.Exists"/>.
		/// </remarks>
		ISelection TrueSelection { get; }
		/// <summary>
		/// Tells to open a new (non-existing) file in the editor, similar to [ShiftF4].
		/// Set it before opening.
		/// </summary>
		/// <remarks>
		/// Perhaps this option in not actually used (Far 2.0.1302).
		/// </remarks>
		bool IsNew { get; set; }
		/// <summary>
		/// Inserts a string.
		/// Editor must be current.
		/// </summary>
		/// <param name="text">The text. Supported line delimiters: CR, LF, CR+LF.</param>
		/// <remarks>
		/// The text is processed in the same way as it is typed.
		/// </remarks>
		void Insert(string text);
		/// <summary>
		/// Inserts a character.
		/// Editor must be current.
		/// </summary>
		/// <param name="text">A character.</param>
		/// <remarks>
		/// The text is processed in the same way as it is typed.
		/// </remarks>
		void InsertChar(char text);
		/// <summary>
		/// Redraws the editor window.
		/// Editor must be current.
		/// </summary>
		/// <remarks>
		/// Normally it should be called when changes are done to make them visible immediately.
		/// </remarks>
		void Redraw();
		/// <summary>
		/// Deletes a character under <see cref="Cursor"/>.
		/// Editor must be current.
		/// </summary>
		void DeleteChar();
		/// <summary>
		/// Deletes a line under <see cref="Cursor"/>.
		/// Editor must be current.
		/// </summary>
		void DeleteLine();
		/// <summary>
		/// Closes the current editor.
		/// </summary>
		void Close();
		/// <summary>
		/// Saves the file in the current editor. Exception on failure.
		/// </summary>
		void Save();
		/// <summary>
		/// Saves the file in the current editor as the specified file. Exception on failure.
		/// </summary>
		/// <param name="fileName">File name to save to.</param>
		void Save(string fileName);
		/// <summary>
		/// Inserts a new line at the current <see cref="Cursor"/> position.
		/// </summary>
		/// <remarks>
		/// After insertion the cursor is moved to the first position in the inserted line.
		/// </remarks>
		void InsertLine();
		/// <summary>
		/// Inserts a new line at the current <see cref="Cursor"/> position with optional indent.
		/// Editor must be current.
		/// </summary>
		/// <param name="indent">Insert a line with indent.</param>
		/// <remarks>
		/// After insertion the cursor is moved to the first position in the inserted line
		/// or to the indented position in it. Indent is the same as on [Enter].
		/// </remarks>
		void InsertLine(bool indent);
		/// <summary>
		/// Gets true if the editor is opened.
		/// </summary>
		bool IsOpened { get; }
		/// <summary>
		/// Gets or sets the window title. Set it before opening (standard title) or after opening (temporary title).
		/// </summary>
		/// <remarks>
		/// When the editor is opened the standard title will be automatically restored when Far gets control.
		/// </remarks>
		string Title { get; set; }
		/// <summary>
		/// Gets or sets overtype mode.
		/// Editor must be current.
		/// </summary>
		bool Overtype { get; set; }
		/// <summary>
		/// Gets true if the editor text is modified.
		/// Editor must be current.
		/// </summary>
		bool IsModified { get; }
		/// <summary>
		/// Gets true if the editor text is saved.
		/// Editor must be current.
		/// </summary>
		bool IsSaved { get; }
		/// <summary>
		/// Gets true if the file is locked (by [CtrlL]).
		/// Editor must be current.
		/// </summary>
		bool IsLocked { get; }
		/// <summary>
		/// Converts char position to tab position for a given line.
		/// </summary>
		/// <param name="line">Line index, -1 for current.</param>
		/// <param name="pos">Char posistion.</param>
		int ConvertPosToTab(int line, int pos);
		/// <summary>
		/// Converts tab position to char position for a given line.
		/// </summary>
		/// <param name="line">Line index, -1 for current.</param>
		/// <param name="tab">Tab posistion.</param>
		int ConvertTabToPos(int line, int tab);
		/// <summary>
		/// Converts screen coordinates to editor cursor coordinates.
		/// </summary>
		Point ConvertScreenToCursor(Point screen);
		/// <summary>
		/// Gets or sets the current text frame.
		/// </summary>
		/// <seealso cref="Cursor"/>
		TextFrame Frame { get; set; }
		/// <summary>
		/// Begins fast line iteration mode.
		/// </summary>
		/// <remarks>
		/// Call this method before processing of large amount of lines, performance can be drastically improved.
		/// It is strongly recommended to call <see cref="End"/> after processing.
		/// Nested calls of <b>Begin()</b> .. <b>End()</b> are allowed.
		/// <para>
		/// Avoid using this method together with getting <see cref="Frame"/> or <see cref="Cursor"/>,
		/// their values are unpredictable. You have to get them before. But it is OK to set them
		/// between <b>Begin()</b> and <b>End()</b>, directly or by <see cref="GoTo"/> methods.
		/// </para>
		/// </remarks>
		void Begin();
		/// <summary>
		/// Ends fast line iteration mode.
		/// </summary>
		/// <remarks>
		/// Call it after any <see cref="Begin"/> when editor lines processing is done.
		/// </remarks>
		void End();
		/// <summary>
		/// Gets or sets the current cursor position.
		/// Editor must be current.
		/// </summary>
		/// <seealso cref="Frame"/>
		/// <seealso cref="GoTo"/>
		Point Cursor { get; set; }
		/// <summary>
		/// Gets bookmarks in the current editor.
		/// </summary>
		/// <remarks>
		/// Bookmarks are defined as <see cref="TextFrame"/>.
		/// Negative <c>Line</c> means undefined bookmark.
		/// To go to a bookmark set <see cref="Frame"/>.
		/// </remarks>
		ICollection<TextFrame> Bookmarks();
		/// <summary>
		/// Goes to a new cursor position or sets it for opening.
		/// </summary>
		/// <param name="pos">Position.</param>
		/// <param name="line">Line.</param>
		/// <seealso cref="Cursor"/>
		/// <seealso cref="Frame"/>
		void GoTo(int pos, int line);
		/// <summary>
		/// Goes to a line or sets it for opening.
		/// </summary>
		/// <param name="line">Line.</param>
		/// <seealso cref="Cursor"/>
		/// <seealso cref="Frame"/>
		void GoToLine(int line);
		/// <summary>
		/// Goes to a position in the current line.
		/// </summary>
		/// <param name="pos">Position.</param>
		/// <seealso cref="Cursor"/>
		/// <seealso cref="Frame"/>
		void GoToPos(int pos);
		/// <summary>
		/// Goes to the end of text.
		/// Editor must be current.
		/// </summary>
		/// <param name="addLine">Add an empty line if the last is not empty.</param>
		void GoEnd(bool addLine);
		/// <summary>
		/// Gets or sets any user data.
		/// </summary>
		object Data { get; set; }
		/// <summary>
		/// Gets or sets a host operating on the editor.
		/// </summary>
		/// <remarks>
		/// This property is set by a module in advanced scenarios when an editor is used in a very unusual way.
		/// It can be set once, usually by a creator or by a handler on opening. It is not used internally, it
		/// only helps to avoid conflicts between modules.
		/// <para>Example scenario: <b>PowerShellFar</b> may use editors as command consoles. On opening it
		/// attaches a host object which is subscribed to the editor events. This approach makes impossible
		/// to attach yet another editor host and prevents advanced use of the editor from other modules
		/// (if they also follow this technique of attaching a host).
		/// </para>
		/// </remarks>
		object Host { get; set; }
		/// <summary>
		/// Gets true if the last line is current.
		/// Editor must be current.
		/// </summary>
		bool IsLastLine { get; }
		/// <summary>
		/// Gets text with default line separator.
		/// Editor must be current.
		/// </summary>
		string GetText();
		/// <summary>
		/// Gets text.
		/// Editor must be current.
		/// </summary>
		/// <param name="separator">Line separator. Empty or null ~ CRLF.</param>
		string GetText(string separator);
		/// <summary>
		/// Sets text.
		/// Editor must be current.
		/// </summary>
		/// <param name="text">New text.</param>
		void SetText(string text);
		/// <summary>
		/// Opens the editor.
		/// </summary>
		/// <remarks>
		/// It is the same as <see cref="Open(OpenMode)"/> with open mode <see cref="OpenMode.None"/>.
		/// See remarks there.
		/// </remarks>
		void Open();
		/// <summary>
		/// Opens the editor.
		/// </summary>
		/// <remarks>
		/// To open an editor you should create an editor operator by <see cref="IFar.CreateEditor"/>,
		/// set at least its <see cref="FileName"/> and optionally: <see cref="DeleteSource"/>,
		/// <see cref="DisableHistory"/>, <see cref="Switching"/>, <see cref="IsNew"/>,
		/// <see cref="Title"/>, and <see cref="Window"/>. Then this method is called.
		/// <para>
		/// If the file is already opened in an editor then this instance should not be used after opening
		/// because technically an editor was not opened but reused. The safe way is to get the current
		/// <see cref="IFar.Editor"/> after opening and work with it.
		/// </para>
		/// </remarks>
		void Open(OpenMode mode);
		/// <summary>
		/// Begins an undo block.
		/// </summary>
		void BeginUndo();
		/// <summary>
		/// Ends an undo block.
		/// </summary>
		void EndUndo();
		/// <summary>
		/// Invokes undo.
		/// </summary>
		void Undo();
		/// <summary>
		/// Invokes redo.
		/// </summary>
		void Redo();
		/// <summary>
		/// Creates a writer that writes to the current position of the current editor.
		/// </summary>
		/// <remarks>
		/// It is not recommended to change the <see cref="Cursor"/> position during writing,
		/// but it seems to be safe to do so if you <c>Flush()</c> the writer before the change.
		/// </remarks>
		/// <returns>Created writer. As any writer, it has to be closed after use.</returns>
		TextWriter CreateWriter();
		/// <summary>
		/// Begins asynchronous mode.
		/// </summary>
		/// <remarks>
		/// This mode is designed for writing text to a not current editor or from background jobs.
		/// The editor is partially blocked until the mode is not closed by <see cref="EndAsync"/>.
		/// Actual writing happens when the editor has or gets focus, otherwise data are queued.
		/// <para>
		/// In this mode data are always appended to the end of the current text, so that the
		/// output procedure is similar to console output.
		/// </para>
		/// <para>
		/// Only <c>Insert*</c> methods should be called during asynchronous mode even if you can
		/// call something else technically without problems.
		/// </para>
		/// <para>
		/// Input events (keys, mouse, idle) are disabled in asynchronous mode.
		/// There is only a special event <see cref="IAnyEditor.CtrlCPressed"/>
		/// that can be used for example for stopping the mode by a user.
		/// </para>
		/// <ul>
		/// <li>Nested calls of are not allowed.</li>
		/// <li>Use this mode only when it is absolutely needed.</li>
		/// <li>Module <b>PowerShellFar</b> uses this mode for asynchronous editor consoles.</li>
		/// </ul>
		/// </remarks>
		void BeginAsync();
		/// <summary>
		/// Ends asynchronous mode.
		/// </summary>
		/// <remarks>
		/// It must be called after <see cref="BeginAsync"/> when asynchronous operations complete.
		/// Note: it is OK to call it when asynchronous mode is already stopped or even was not started.
		/// </remarks>
		void EndAsync();
		/// <summary>
		/// Gets or sets show white space flag.
		/// Set it before or after opening.
		/// </summary>
		bool ShowWhiteSpace { get; set; }
		/// <summary>
		/// Tells to write BOM on saving.
		/// Set it before or after opening.
		/// </summary>
		bool WriteByteOrderMark { get; set; }
	}

	/// <summary>
	/// Arguments of <see cref="IAnyEditor.OnRedraw"/> event.
	/// </summary>
	/// <remarks>
	/// This API is not complete and will be improved when needed.
	/// </remarks>
	public sealed class RedrawEventArgs : EventArgs
	{
		int _mode;
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
	/// *) if it is <see cref="ILines.Strings"/> from <see cref="IEditor.Lines"/> then strings are lines of editor
	/// and standard list operations affect editor text;
	/// *) if it is <see cref="ILines.Strings"/> from <see cref="IEditor.Selection"/> then strings are selected line parts
	/// and list operations affect only selected text.
	/// </remarks>
	public interface IStrings : IList<string>
	{
	}

	/// <summary>
	/// Selection in <see cref="ILine"/>.
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
		/// End position of selection in the line. If selection includes the end of line sequence this field has a value of -1.
		/// </summary>
		int End { get; }
		/// <summary>
		/// Selection length. If line doesn't contain selection it is -1.
		/// </summary>
		int Length { get; }
	}

	/// <summary>
	/// Interface of a line in various text and line editors:
	/// *) item of <see cref="IEditor.Lines"/> or <see cref="IEditor.Selection"/> in <see cref="IEditor"/>;
	/// *) command line <see cref="IFar.CommandLine"/>;
	/// *) edit box (<see cref="IEdit.Line"/>) and combo box (<see cref="IComboBox.Line"/>) in <see cref="IDialog"/>.
	/// </summary>
	public interface ILine
	{
		/// <summary>
		/// Line number in source <see cref="IEditor"/>.
		/// -1 for <see cref="IEditor.CurrentLine"/> and <see cref="IFar.CommandLine"/>.
		/// </summary>
		int No { get; }
		/// <summary>
		/// Line text (<see cref="IEditor.Lines"/>, <see cref="IEditor.CurrentLine"/>, <see cref="IFar.CommandLine"/>)
		/// or text of selected line part (<see cref="IEditor.Selection"/>).
		/// </summary>
		string Text { get; set; }
		/// <summary>
		/// End of line characters.
		/// </summary>
		string EndOfLine { get; set; }
		/// <summary>
		/// Selected line part.
		/// </summary>
		ILineSelection Selection { get; }
		/// <summary>
		/// Cursor position.
		/// Returns -1 if the line is not current.
		/// Setting of negative value moves cursor to the line end.
		/// </summary>
		int Pos { get; set; }
		/// <summary>
		/// Inserts text into the line at the current cursor position.
		/// Editor: don't use if it is not the current line.
		/// </summary>
		/// <param name="text">String to insert to the line.</param>
		void Insert(string text);
		/// <summary>
		/// Selects a text fragment in the current or command line.
		/// </summary>
		/// <param name="startPosition">Start position.</param>
		/// <param name="endPosition">End position.</param>
		void Select(int startPosition, int endPosition);
		/// <summary>
		/// Clears selection in the current or command line.
		/// </summary>
		void Unselect();
		/// <summary>
		/// Gets an instance of a full line if this line represents only a part,
		/// (e.g. the line is from <see cref="IEditor.Selection"/>),
		/// or returns this instance itself.
		/// </summary>
		ILine FullLine { get; }
		/// <summary>
		/// The text length.
		/// </summary>
		int Length { get; }
		/// <summary>
		/// Parent window kind (<c>Editor</c>, <c>Panels</c>, <c>Dialog</c>).
		/// </summary>
		WindowKind WindowKind { get; }
	}

	/// <summary>
	/// List of lines. See <see cref="IEditor.Lines"/> (all editor lines), <see cref="IEditor.Selection"/> (editor selected lines\parts).
	/// </summary>
	public interface ILines : IList<ILine>
	{
		/// <summary>
		/// First line.
		/// </summary>
		ILine First { get; }
		/// <summary>
		/// Last line.
		/// </summary>
		ILine Last { get; }
		/// <summary>
		/// Lines as strings.
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
	/// Arguments of <see cref="IAnyEditor.OnKey"/> event.
	/// </summary>
	public sealed class KeyEventArgs : EventArgs
	{
		/// <param name="key">Key data.</param>
		public KeyEventArgs(KeyInfo key)
		{
			_key = key;
		}
		/// <summary>
		/// Key data.
		/// </summary>
		public KeyInfo Key
		{
			get { return _key; }
		}
		KeyInfo _key;
		/// <summary>
		/// Ignore event.
		/// </summary>
		public bool Ignore { get; set; }
	}

	/// <summary>
	/// Arguments of <see cref="IAnyEditor.OnMouse"/> event.
	/// </summary>
	public sealed class MouseEventArgs : EventArgs
	{
		/// <param name="mouse">Mouse data.</param>
		public MouseEventArgs(MouseInfo mouse)
		{
			_mouse = mouse;
		}
		/// <summary>
		/// Mouse data.
		/// </summary>
		public MouseInfo Mouse
		{
			get { return _mouse; }
		}
		MouseInfo _mouse;
		/// <summary>
		/// Ignore event.
		/// </summary>
		public bool Ignore { get; set; }
	}

	/// <summary>
	/// Editor expand tabs mode.
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

	/// <summary>
	/// Complete information about text frame and cursor position in an editor.
	/// </summary>
	public struct TextFrame
	{
		/// <summary>
		/// Sets the same value for all members.
		/// </summary>
		/// <param name="value">Value.</param>
		public TextFrame(int value)
		{
			_line = _pos = _tabPos = _topLine = _leftPos = value;
		}
		/// <summary>
		/// Line index (starting with 0).
		/// </summary>
		public int Line { get { return _line; } set { _line = value; } }
		int _line;
		/// <summary>
		/// Position in the string (i.e. in chars).
		/// </summary>
		public int Pos { get { return _pos; } set { _pos = value; } }
		int _pos;
		/// <summary>
		/// Position on the screen (i.e. in columns).
		/// </summary>
		public int TabPos { get { return _tabPos; } set { _tabPos = value; } }
		int _tabPos;
		/// <summary>
		/// First visible line index.
		/// </summary>
		public int TopLine { get { return _topLine; } set { _topLine = value; } }
		int _topLine;
		/// <summary>
		/// Leftmost visible position of the text on the screen.
		/// </summary>
		public int LeftPos { get { return _leftPos; } set { _leftPos = value; } }
		int _leftPos;
		///
		public static bool operator ==(TextFrame left, TextFrame right)
		{
			return
				left._line == right._line &&
				left._pos == right._pos &&
				left._tabPos == right._tabPos &&
				left._topLine == right._topLine &&
				left._leftPos == right._leftPos;
		}
		///
		public static bool operator !=(TextFrame left, TextFrame right)
		{
			return !(left == right);
		}
		///
		public override bool Equals(Object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;
			TextFrame that = (TextFrame)obj;
			return this == that;
		}
		///
		public override string ToString()
		{
			return "((" + _pos + "/" + _tabPos + ", " + _line + ")(" + _leftPos + ", " + _topLine + "))";
		}
		///
		public override int GetHashCode()
		{
			return (_line << 16) ^ (_pos << 16) ^ _tabPos ^ _topLine ^ _leftPos;
		}
	}
}
