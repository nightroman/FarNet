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
	/// <para>
	/// Basically, the editor works like the indexed list of <see cref="ILine"/> lines. List members have "standard" names:
	/// <see cref="Count"/> (line count), <see cref="this"/> (access by index), <see cref="RemoveAt"/> (removes by index).
	/// </para>
	/// <para>
	/// Still, the editor is not a standard list. If you need a standard line list then use the <see cref="Lines"/> method.
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
		/// Gets line count. At least one line exists.
		/// </summary>
		/// <seealso cref="this"/>
		int Count { get; }
		/// <summary>
		/// Gets the line by the index.
		/// </summary>
		/// <param name="index">Line index, -1 for the current line.</param>
		/// <returns>The requested line.</returns>
		/// <seealso cref="Count"/>
		/// <seealso cref="Lines"/>
		/// <seealso cref="SelectedLines"/>
		ILine this[int index] { get; }
		/// <summary>
		/// Gets the list of editor lines.
		/// Editor must be current.
		/// </summary>
		/// <param name="ignoreEmptyLast">Tells to ignore the empty last line.</param>
		ILineCollection Lines(bool ignoreEmptyLast);
		/// <summary>
		/// Gets the list of selected lines and parts.
		/// Editor must be current.
		/// </summary>
		/// <param name="ignoreEmptyLast">Tells to ignore the empty last line.</param>
		ILineCollection SelectedLines(bool ignoreEmptyLast);
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
		/// Tells to open a new (non-existing) file in the editor, similar to [ShiftF4].
		/// Set it before opening.
		/// </summary>
		/// <remarks>
		/// Perhaps this option in not actually used (Far 2.0.1302).
		/// </remarks>
		bool IsNew { get; set; }
		/// <summary>
		/// Inserts the text at the current caret position.
		/// Editor must be current.
		/// </summary>
		/// <param name="text">The text. Supported line delimiters: CR, LF, CR+LF.</param>
		/// <remarks>
		/// The text is processed in the same way as it is typed.
		/// </remarks>
		void InsertText(string text);
		/// <summary>
		/// Inserts a character.
		/// Editor must be current.
		/// </summary>
		/// <param name="text">Character to be inserted.</param>
		/// <remarks>
		/// The character is processed in the same way as it is typed.
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
		/// Deletes a character under the <see cref="Caret"/>.
		/// Editor must be current.
		/// </summary>
		void DeleteChar();
		/// <summary>
		/// Deletes the line where the <see cref="Caret"/> is.
		/// Editor must be current.
		/// </summary>
		void DeleteLine();
		/// <summary>
		/// Deletes the selected text.
		/// </summary>
		/// <remarks>
		/// To clear selection use <see cref="UnselectText"/>.
		/// </remarks>
		void DeleteText();
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
		/// Inserts a new line at the <see cref="Caret"/> position.
		/// </summary>
		/// <remarks>
		/// After insertion the caret is moved to the first position in the inserted line.
		/// </remarks>
		void InsertLine();
		/// <summary>
		/// Inserts a new line at the <see cref="Caret"/> position with optional indent.
		/// Editor must be current.
		/// </summary>
		/// <param name="indent">Insert a line with indent.</param>
		/// <remarks>
		/// After insertion the caret is moved to the first position in the inserted line
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
		/// Converts char position to tab position for the given line.
		/// </summary>
		/// <param name="line">Line index, -1 for current.</param>
		/// <param name="column">Column index to be converted.</param>
		int ConvertColumnEditorToScreen(int line, int column);
		/// <summary>
		/// Converts tab position to char position for the given line.
		/// </summary>
		/// <param name="line">Line index, -1 for current.</param>
		/// <param name="column">Column index to be converted.</param>
		int ConvertColumnScreenToEditor(int line, int column);
		/// <summary>
		/// Converts the point in screen coordinates to the point in editor coordinates.
		/// </summary>
		Point ConvertPointScreenToEditor(Point point);
		/// <summary>
		/// Gets or sets the current text frame.
		/// </summary>
		/// <seealso cref="Caret"/>
		TextFrame Frame { get; set; }
		/// <summary>
		/// Begins fast line iteration mode.
		/// </summary>
		/// <remarks>
		/// Call this method before processing of large amount of lines, performance can be drastically improved.
		/// It is strongly recommended to call <see cref="End"/> after processing.
		/// Nested calls of <b>Begin()</b> .. <b>End()</b> are allowed.
		/// <para>
		/// Avoid using this method together with getting <see cref="Frame"/> or <see cref="Caret"/>,
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
		/// Gets or sets the caret position.
		/// Editor must be current.
		/// </summary>
		/// <seealso cref="Frame"/>
		/// <seealso cref="GoTo"/>
		Point Caret { get; set; }
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
		/// Sets the caret position or posts it for opening.
		/// </summary>
		/// <param name="column">Column index.</param>
		/// <param name="line">Line index.</param>
		/// <seealso cref="Caret"/>
		/// <seealso cref="Frame"/>
		void GoTo(int column, int line);
		/// <summary>
		/// Sets the current line or posts it for opening.
		/// </summary>
		/// <param name="line">Line index.</param>
		/// <seealso cref="Caret"/>
		/// <seealso cref="Frame"/>
		void GoToLine(int line);
		/// <summary>
		/// Goes to a character in the current line.
		/// </summary>
		/// <param name="column">Column index.</param>
		/// <seealso cref="Caret"/>
		/// <seealso cref="Frame"/>
		void GoToColumn(int column);
		/// <summary>
		/// Goes to the end of text.
		/// Editor must be current.
		/// </summary>
		/// <param name="addLine">Add an empty line if the last is not empty.</param>
		void GoToEnd(bool addLine);
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
		/// It is not recommended to change the <see cref="Caret"/> position during writing,
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
		/// <summary>
		/// Gets the selected text with the default line separator.
		/// </summary>
		string GetSelectedText();
		/// <summary>
		/// Gets the selected text with the specified line separator.
		/// </summary>
		/// <param name="separator">Line separator. null ~ CR+LF.</param>
		string GetSelectedText(string separator);
		/// <summary>
		/// Sets (replaces) the selected text.
		/// </summary>
		/// <param name="text">New text.</param>
		void SetSelectedText(string text);
		/// <summary>
		/// Selects the specified region of text.
		/// </summary>
		/// <param name="kind">Region kind.</param>
		/// <param name="column1">Column 1.</param>
		/// <param name="line1">Line 1.</param>
		/// <param name="column2">Column 2.</param>
		/// <param name="line2">Line 2.</param>
		void SelectText(RegionKind kind, int column1, int line1, int column2, int line2);
		/// <summary>
		/// Selects all text.
		/// </summary>
		void SelectAllText();
		/// <summary>
		/// Turns the text selection off.
		/// </summary>
		/// <remarks>
		/// To delete the selected text use <see cref="DeleteText"/>.
		/// </remarks>
		void UnselectText();
		/// <summary>
		/// Gets true if selection exists.
		/// </summary>
		bool SelectionExists { get; }
		/// <summary>
		/// Gets the selection kind.
		/// </summary>
		RegionKind SelectionKind { get; }
		/// <summary>
		/// Gets the selected place.
		/// </summary>
		Place SelectionPlace { get; }
		/// <summary>
		/// Removes the line by the index.
		/// </summary>
		/// <param name="index">Index of the line to be removed.</param>
		void RemoveAt(int index);
	}

	/// <summary>
	/// Arguments of <see cref="IAnyEditor.OnRedraw"/> event.
	/// </summary>
	/// <remarks>
	/// This API is not complete, perhaps it is not needed in FarNet at all.
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
	/// Line region, for example selected region of <see cref="ILine"/> line.
	/// </summary>
	public struct LineRegion
	{
		/// <summary>
		/// Start position.
		/// </summary>
		public int Start { get; set; }
		/// <summary>
		/// End position.
		/// </summary>
		public int End { get; set; }
		/// <summary>
		/// Gets the region length.
		/// </summary>
		public int Length { get { return End - Start; } }
		///
		public override string ToString()
		{
			return Length < 0 ? "<none>" : Invariant.Format("{0} from {1} to {2}", Length, Start, End);
		}
		///
		public static bool operator ==(LineRegion left, LineRegion right)
		{
			return left.Start == right.Start && left.End == right.End;
		}
		///
		public static bool operator !=(LineRegion left, LineRegion right)
		{
			return left.Start != right.Start || left.End != right.End;
		}
		///
		public override bool Equals(object obj)
		{
			return obj != null && obj.GetType() == typeof(LineRegion) && this == (LineRegion)obj;
		}
		///
		public override int GetHashCode()
		{
			return Start | (End << 16);
		}
	}

	/// <summary>
	/// Abstract line in various text and line editors.
	/// </summary>
	/// <remarks>
	/// It can be:
	/// *) an item of <see cref="IEditor.Lines"/> or <see cref="IEditor.SelectedLines"/> in <see cref="IEditor"/>;
	/// *) the command line <see cref="IFar.CommandLine"/>;
	/// *) an edit box (<see cref="IEdit.Line"/>) or a combo box (<see cref="IComboBox.Line"/>) in a dialog.
	/// </remarks>
	public abstract class ILine
	{
		/// <summary>
		/// Gets the line index in the source line collection.
		/// </summary>
		/// <remarks>
		/// It returns -1 for the editor current line, the command line, and dialog edit lines.
		/// </remarks>
		public virtual int Index { get { return -1; } }
		/// <summary>
		/// Gets or sets the line text.
		/// </summary>
		public abstract string Text { get; set; }
		/// <summary>
		/// Gets or sets (replaces) the selected text.
		/// </summary>
		/// <remarks>
		/// If there is no selection then <c>get</c> returns null, <c>set</c> throws.
		/// </remarks>
		public abstract string SelectedText { get; set; }
		/// <summary>
		/// Gets or sets the end of line.
		/// </summary>
		/// <remarks>
		/// It is used only for editor lines and normally it should be kept default.
		/// </remarks>
		public virtual string EndOfLine { get { return string.Empty; } set { throw new NotSupportedException(); } }
		/// <summary>
		/// Gets or sets the caret position.
		/// </summary>
		/// <remarks>
		/// Returns -1 if it is an editor line and it is not current.
		/// Setting of a negative value moves the caret to the end.
		/// </remarks>
		public abstract int Caret { get; set; }
		/// <summary>
		/// Inserts text into the line at the current caret position.
		/// </summary>
		/// <param name="text">String to insert to the line.</param>
		/// <remarks>
		/// In the editor this method should not be used for the current line only.
		/// </remarks>
		public abstract void InsertText(string text);
		/// <summary>
		/// Selects the text fragment in the current editor line, the command line, or the dialog line.
		/// </summary>
		/// <param name="startPosition">Start position.</param>
		/// <param name="endPosition">End position.</param>
		public abstract void SelectText(int startPosition, int endPosition);
		/// <summary>
		/// Turns selection off in the current editor line, the command line, or the dialog line.
		/// </summary>
		public abstract void UnselectText();
		/// <summary>
		/// Gets an instance of a full line if this line represents only a part,
		/// (e.g. the line is from <see cref="IEditor.SelectedLines"/>),
		/// or returns this instance itself.
		/// </summary>
		public abstract ILine FullLine { get; }
		/// <summary>
		/// Gets the the text length.
		/// </summary>
		/// <remarks>
		/// It helps to avoid calls of more expensive <see cref="Text"/> in some cases when only length is needed.
		/// </remarks>
		public abstract int Length { get; }
		/// <summary>
		/// Gets the parent window kind (<c>Editor</c>, <c>Panels</c>, <c>Dialog</c>).
		/// </summary>
		public abstract WindowKind WindowKind { get; }
		/// <summary>
		/// Gets the selection info.
		/// </summary>
		/// <remarks>
		/// If selection does not exists then returned position and length values are negative.
		/// </remarks>
		public abstract LineRegion Selection { get; }
		/// <summary>
		/// Returns the line <see cref="Text"/>.
		/// </summary>
		/// <remarks>
		/// It gets the line text and this is not going to change, modules can rely on this.
		/// </remarks>
		public sealed override string ToString()
		{
			return Text;
		}
	}

	/// <summary>
	/// Collection of editor lines.
	/// </summary>
	/// <remarks>
	/// Line collections have standard <c>IList(Of T)</c> members (see MSDN) and a few additional helper members.
	/// <para>
	/// It is exposed as <see cref="IEditor.Lines"/> (editor lines), <see cref="IEditor.SelectedLines"/> (editor selected lines and parts).
	/// </para>
	/// </remarks>
	public interface ILineCollection : IList<ILine>
	{
		/// <summary>
		/// Gets the first line.
		/// </summary>
		ILine First { get; }
		/// <summary>
		/// Gets the last line.
		/// </summary>
		ILine Last { get; }
		/// <summary>
		/// Adds the string as a new line to the end.
		/// </summary>
		void AddText(string item);
		/// <summary>
		/// Inserts the string as a new line with the specified line index.
		/// </summary>
		void InsertText(int index, string item);
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
	/// Information about the text frame and the caret position.
	/// </summary>
	public struct TextFrame
	{
		/// <summary>
		/// Sets the same value for all members.
		/// </summary>
		/// <param name="value">Value.</param>
		public TextFrame(int value)
			: this()
		{
			CaretLine = value;
			CaretColumn = value;
			CaretScreenColumn = value;
			VisibleLine = value;
			VisibleChar = value;
		}
		/// <summary>
		/// Caret line index.
		/// </summary>
		public int CaretLine { get; set; }
		/// <summary>
		/// Caret character index.
		/// </summary>
		public int CaretColumn { get; set; }
		/// <summary>
		/// Caret screen column index.
		/// </summary>
		public int CaretScreenColumn { get; set; }
		/// <summary>
		/// First visible line index.
		/// </summary>
		public int VisibleLine { get; set; }
		/// <summary>
		/// First visible character index.
		/// </summary>
		public int VisibleChar { get; set; }
		///
		public static bool operator ==(TextFrame left, TextFrame right)
		{
			return
				left.CaretLine == right.CaretLine &&
				left.CaretColumn == right.CaretColumn &&
				left.CaretScreenColumn == right.CaretScreenColumn &&
				left.VisibleLine == right.VisibleLine &&
				left.VisibleChar == right.VisibleChar;
		}
		///
		public static bool operator !=(TextFrame left, TextFrame right)
		{
			return !(left == right);
		}
		///
		public override bool Equals(Object obj)
		{
			return obj != null && obj.GetType() == typeof(TextFrame) && this == (TextFrame)obj;
		}
		///
		public override string ToString()
		{
			return "((" + CaretColumn + "/" + CaretScreenColumn + ", " + CaretLine + ")(" + VisibleChar + ", " + VisibleLine + "))";
		}
		///
		public override int GetHashCode()
		{
			return CaretLine | (CaretColumn << 16);
		}
	}
}
