
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using FarNet.Forms;

namespace FarNet
{
	/// <summary>
	/// Common editor events.
	/// </summary>
	public abstract class IEditorBase
	{
		/// <summary>
		/// Called when the editor is closed.
		/// </summary>
		public abstract event EventHandler Closed;
		/// <summary>
		/// Called when the editor is opened.
		/// </summary>
		public abstract event EventHandler Opened;
		/// <summary>
		/// Called before saving.
		/// </summary>
		public abstract event EventHandler<EditorSavingEventArgs> Saving;
		/// <summary>
		/// Called on a key pressed.
		/// </summary>
		public abstract event EventHandler<KeyEventArgs> KeyDown;
		/// <summary>
		/// Called on a key pressed.
		/// </summary>
		public abstract event EventHandler<KeyEventArgs> KeyUp;
		/// <summary>
		/// Occurs when a mouse button is clicked.
		/// </summary>
		public abstract event EventHandler<MouseEventArgs> MouseClick;
		/// <summary>
		/// Occurs when a mouse button is clicked two times.
		/// </summary>
		public abstract event EventHandler<MouseEventArgs> MouseDoubleClick;
		/// <summary>
		/// Occurs when the mouse pointer moves.
		/// </summary>
		public abstract event EventHandler<MouseEventArgs> MouseMove;
		/// <summary>
		/// Occurs when the mouse wheel is rotated.
		/// </summary>
		public abstract event EventHandler<MouseEventArgs> MouseWheel;
		/// <summary>
		/// Called when the editor has got focus.
		/// </summary>
		public abstract event EventHandler GotFocus;
		/// <summary>
		/// Called when the editor is losing focus.
		/// </summary>
		public abstract event EventHandler LosingFocus;
		/// <summary>
		/// Called periodically when a user is idle.
		/// </summary>
		/// <seealso cref="IdledHandler"/>
		public abstract event EventHandler Idled;
		/// <summary>
		/// Called on [CtrlC] in asynchronous mode, see <see cref="IEditor.BeginAsync"/>.
		/// </summary>
		public abstract event EventHandler CtrlCPressed;
		/// <summary>
		/// Called on redrawing.
		/// </summary>
		public abstract event EventHandler Redrawing;
		/// <summary>
		/// Called on changes.
		/// </summary>
		public abstract event EventHandler<EditorChangedEventArgs> Changed;
	}

	/// <summary>
	/// Any editor operator, common editor events, options and tools.
	/// </summary>
	/// <remarks>
	/// It is exposed as <see cref="IFar.AnyEditor"/>.
	/// <para>
	/// It is used to subscribe to events of editors that are not yet opened.
	/// It also exposes common editor tools.
	/// </para>
	/// </remarks>
	public abstract class IAnyEditor : IEditorBase
	{
		/// <summary>
		/// Opens a modal editor in order to edit the text.
		/// </summary>
		/// <param name="args">Arguments.</param>
		/// <returns>The result text.</returns>
		public abstract string EditText(EditTextArgs args);
		/// <summary>
		/// Opens a modal editor in order to edit the text.
		/// </summary>
		/// <param name="text">Input text to be edited.</param>
		/// <param name="title">Editor window title.</param>
		[Obsolete("Use EditText(args).")]
		public string EditText(string text, string title)
		{ return EditText(new EditTextArgs() { Text = text, Title = title }); }
	}

	/// <summary>
	/// Arguments of <see cref="IAnyEditor.EditText(EditTextArgs)"/>.
	/// </summary>
	public class EditTextArgs
	{
		/// <summary>
		/// Input text to be edited.
		/// </summary>
		public string Text { get; set; }
		/// <summary>
		/// Editor window title.
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// File extension (for Colorer).
		/// </summary>
		public string Extension { get; set; }
		/// <summary>
		/// Tells to open text locked for changes.
		/// </summary>
		public bool IsLocked { get; set; }
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
	/// The editor has members making it semantically similar to a list of <see cref="ILine"/> lines and strings.
	/// These members are: <see cref="Count"/> (line count), <see cref="this[int]"/> (gets a line by its index),
	/// <see cref="RemoveAt"/> (removes a line by its index), <see cref="Clear"/> (removes all lines),
	/// <see cref="Add"/>\<see cref="Insert"/> (adds\inserts text line(s)).
	/// </para>
	/// <para>
	/// Still, the editor is not a standard list of strings or lines.
	/// Standard string list is <see cref="Strings"/>, it has all useful list members implemented.
	/// Standard line lists are <see cref="Lines"/> or <see cref="SelectedLines"/>, they have members mostly for reading.
	/// </para>
	/// </remarks>
	public abstract class IEditor : IEditorBase
	{
		#region Line list
		/// <summary>
		/// Gets line count. At least one line always exists.
		/// </summary>
		/// <seealso cref="this[int]"/>
		public abstract int Count { get; }
		/// <summary>
		/// Gets the line by its index.
		/// </summary>
		/// <param name="index">Line index.</param>
		/// <returns>The requested line.</returns>
		/// <seealso cref="Count"/>
		/// <seealso cref="Lines"/>
		/// <seealso cref="SelectedLines"/>
		/// <remarks>
		/// The returned line instance should be used instantly and should never be kept for future use.
		/// The index is permanent, the instance always points to a line at this index even if it is invalid after text changes.
		/// </remarks>
		public abstract ILine this[int index] { get; }
		/// <summary>
		/// Adds the text to the end.
		/// </summary>
		/// <param name="text">Text to be inserted.</param>
		/// <remarks>
		/// If the last editor line is empty then it does not add a new line
		/// into the list but technically speaking it inserts it before the
		/// last. But this way is actually rather expected for the editor.
		/// </remarks>
		public abstract void Add(string text);
		/// <summary>
		/// Removes all lines but one empty.
		/// </summary>
		public abstract void Clear();
		/// <summary>
		/// Inserts the text at the given line index.
		/// </summary>
		/// <param name="line">Line index.</param>
		/// <param name="text">Text to be inserted.</param>
		public abstract void Insert(int line, string text);
		/// <summary>
		/// Removes the line by its index.
		/// </summary>
		/// <param name="index">Index of the line to be removed.</param>
		public abstract void RemoveAt(int index);
		#endregion
		/// <summary>
		/// Gets the current editor line.
		/// </summary>
		/// <remarks>
		/// The returned object is not a copy of the current line but rather a pointer to the current line.
		/// If the caret moves to another line then the object operates on a new current line.
		/// </remarks>
		/// <seealso cref="IFar.Line"/>
		public abstract ILine Line { get; }
		/// <summary>
		/// Gets the internal identifier.
		/// </summary>
		public abstract IntPtr Id { get; }
		/// <summary>
		/// Gets or sets tab size in spaces in the current editor.
		/// </summary>
		public abstract int TabSize { get; set; }
		/// <summary>
		/// Gets or sets expand tabs mode in the current editor.
		/// </summary>
		public abstract ExpandTabsMode ExpandTabs { get; set; }
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
		/// Gets the list of editor lines.
		/// </summary>
		/// <remarks>
		/// <include file='doc.xml' path='doc/Experimental/*'/>
		/// <include file='doc.xml' path='doc/EditorList/*'/>
		/// </remarks>
		/// <seealso cref="Strings"/>
		/// <seealso cref="SelectedLines"/>
		public abstract IList<ILine> Lines { get; }
		/// <summary>
		/// Gets the list of selected lines.
		/// </summary>
		/// <remarks>
		/// <include file='doc.xml' path='doc/Experimental/*'/>
		/// <include file='doc.xml' path='doc/EditorList/*'/>
		/// <para>
		/// Recommended ways to change the selected text are:
		/// get the selected text by <see cref="GetSelectedText()"/> and operate on this string
		/// or iterate through selected lines and build a new text, for example using a string builder.
		/// Then use <see cref="SetSelectedText"/> if you want new text to be selected after replacement
		/// or use <see cref="DeleteText"/> + <see cref="InsertText"/> to delete selected and insert new text.
		/// </para>
		/// <para>
		/// The last line of the selection area is not included if nothing is actually selected there.
		/// </para>
		/// </remarks>
		public abstract IList<ILine> SelectedLines { get; }
		/// <summary>
		/// Gets the string list representation of editor lines.
		/// </summary>
		/// <remarks>
		/// <include file='doc.xml' path='doc/Experimental/*'/>
		/// <para>
		/// See MSDN <c>IList(Of T)</c> interface for members, almost all of them are implemented.
		/// Not implemented members are: <c>Contains(string)</c>, <c>IndexOf(string)</c>, and <c>Remove(string)</c>.
		/// </para>
		/// <para>
		/// Note that this string list is almost like any standard list but
		/// there are three main differences. 1) Clear() does not removes all
		/// lines because one empty line still exists. 2) If the last line is
		/// empty then Add() actually inserts before it. 3) If a new string
		/// being inserted contains line separators then more than one item is
		/// inserted into the list.
		/// </para>
		/// </remarks>
		public abstract IList<string> Strings { get; }
		/// <summary>
		/// Gets or sets the name of a file being or to be edited.
		/// Set it before opening.
		/// </summary>
		/// <remarks>
		/// Before opening it sets a file to be edited (on opening it can be changed, e.g. converted into its full path).
		/// For an opened editor it gets the file being edited.
		/// </remarks>
		public abstract string FileName { get; set; }
		/// <summary>
		/// Gets or sets the code page identifier.
		/// </summary>
		/// <remarks>
		/// Before opening it sets encoding for reading a file.
		/// After opening it gets and sets the current encoding.
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
		/// Inserts the text at the caret position.
		/// </summary>
		/// <param name="text">The text. Supported line delimiters: CR, LF, CR+LF.</param>
		/// <remarks>
		/// The text is processed in the same way as it is typed.
		/// </remarks>
		public abstract void InsertText(string text);
		/// <summary>
		/// Inserts a character at the caret position.
		/// </summary>
		/// <param name="text">Character to be inserted.</param>
		/// <remarks>
		/// The character is processed in the same way as it is typed.
		/// </remarks>
		public abstract void InsertChar(char text);
		/// <summary>
		/// Redraws the editor window.
		/// </summary>
		/// <remarks>
		/// Normally it should be called when changes are done to make them visible immediately.
		/// </remarks>
		public abstract void Redraw();
		/// <summary>
		/// Deletes a character under the caret.
		/// </summary>
		public abstract void DeleteChar();
		/// <summary>
		/// Deletes the line where the caret is.
		/// </summary>
		public abstract void DeleteLine();
		/// <summary>
		/// Deletes the selected text.
		/// </summary>
		/// <remarks>
		/// To clear selection use <see cref="UnselectText"/>.
		/// </remarks>
		public abstract void DeleteText();
		/// <summary>
		/// Closes the current editor.
		/// </summary>
		/// <remarks>
		/// Changes, if any, are lost. Call <see cref="Save()"/> to save them.
		/// </remarks>
		public abstract void Close();
		/// <summary>
		/// Saves changes, if any. Exception on failure.
		/// </summary>
		/// <remarks>
		/// This method does nothing if there are no changes to save (<see cref="IsSaved"/> gets true).
		/// </remarks>
		public abstract void Save();
		/// <summary>
		/// Saves the file in the current editor even with no changes. Exception on failure.
		/// </summary>
		/// <param name="force">Tells to write the file even if there are no changes.</param>
		public abstract void Save(bool force);
		/// <summary>
		/// Saves the file in the current editor as the specified file. Exception on failure.
		/// </summary>
		/// <param name="fileName">File name to save to.</param>
		public abstract void Save(string fileName);
		/// <summary>
		/// Inserts a new line at the caret position.
		/// </summary>
		/// <remarks>
		/// After insertion the caret is moved to the first position in the inserted line.
		/// </remarks>
		public abstract void InsertLine();
		/// <summary>
		/// Inserts a new line at the caret position with optional indent.
		/// </summary>
		/// <param name="indent">Insert a line with indent.</param>
		/// <remarks>
		/// After insertion the caret is moved to the first position in the inserted line
		/// or to the indented position in it. Indent is the same as on [Enter].
		/// </remarks>
		public abstract void InsertLine(bool indent);
		/// <summary>
		/// Gets true if the editor is opened.
		/// </summary>
		public abstract bool IsOpened { get; }
		/// <summary>
		/// Gets or sets the window title. Set it before or after opening.
		/// </summary>
		/// <remarks>
		/// For the current editor setting the title to null or empty restores the original title.
		/// <para>
		/// NOTE: Far API only allows setting the title.
		/// Thus, the title just gets the last value set by a module, if any, not the actual title.
		/// </para>
		/// </remarks>
		public abstract string Title { get; set; }
		/// <summary>
		/// Gets or sets overtype mode.
		/// </summary>
		public abstract bool Overtype { get; set; }
		/// <summary>
		/// Gets true if the text is modified in the current editor (see remarks).
		/// </summary>
		/// <remarks>
		/// It gets true if the text is modified at least once and these changes are not undone.
		/// Note that in this case it will get true even after saving. Use <see cref="IsSaved"/>
		/// in order to check for not saved changes.
		/// </remarks>
		/// <seealso cref="TimeOfSave"/>
		public abstract bool IsModified { get; }
		/// <summary>
		/// Gets true if there are no changes to save in the current editor (see remarks).
		/// </summary>
		/// <remarks>
		/// It is true when the editor is just opened or saved.
		/// Use <see cref="TimeOfSave"/> to check whether it was saved at least once.
		/// </remarks>
		public abstract bool IsSaved { get; }
		/// <summary>
		/// Gets or sets the lock mode ([CtrlL]).
		/// </summary>
		/// <remarks>
		/// Set it before or after opening.
		/// </remarks>
		public abstract bool IsLocked { get; set; }
		/// <summary>
		/// Converts char position to tab position for the given line.
		/// </summary>
		/// <param name="line">Line index, -1 for current.</param>
		/// <param name="column">Column index to be converted.</param>
		public abstract int ConvertColumnEditorToScreen(int line, int column);
		/// <summary>
		/// Converts tab position to char position for the given line.
		/// </summary>
		/// <param name="line">Line index, -1 for current.</param>
		/// <param name="column">Column index to be converted.</param>
		public abstract int ConvertColumnScreenToEditor(int line, int column);
		/// <summary>
		/// Converts the point in editor coordinates to the point in screen coordinates.
		/// </summary>
		/// <param name="point">The point in editor coordinates.</param>
		public abstract Point ConvertPointEditorToScreen(Point point);
		/// <summary>
		/// Converts the point in screen coordinates to the point in editor coordinates.
		/// </summary>
		/// <param name="point">The point in screen coordinates.</param>
		public abstract Point ConvertPointScreenToEditor(Point point);
		/// <summary>
		/// Gets or sets the current text frame.
		/// </summary>
		/// <seealso cref="Caret"/>
		public abstract TextFrame Frame { get; set; }
		/// <summary>
		/// Gets or sets the caret position.
		/// </summary>
		/// <seealso cref="Frame"/>
		/// <seealso cref="GoTo"/>
		public abstract Point Caret { get; set; }
		/// <summary>
		/// Sets the caret position or posts it for opening.
		/// </summary>
		/// <param name="column">Column index.</param>
		/// <param name="line">Line index.</param>
		/// <seealso cref="Caret"/>
		/// <seealso cref="Frame"/>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
		public abstract void GoTo(int column, int line);
		/// <summary>
		/// Sets the current line or posts it for opening.
		/// </summary>
		/// <param name="line">Line index.</param>
		/// <seealso cref="Caret"/>
		/// <seealso cref="Frame"/>
		public abstract void GoToLine(int line);
		/// <summary>
		/// Goes to a character in the current line.
		/// </summary>
		/// <param name="column">Column index.</param>
		/// <seealso cref="Caret"/>
		/// <seealso cref="Frame"/>
		public abstract void GoToColumn(int column);
		/// <summary>
		/// Goes to the end of text.
		/// </summary>
		/// <param name="addLine">Add an empty line if the last is not empty.</param>
		public abstract void GoToEnd(bool addLine);
		/// <summary>
		/// Gets or sets a host operating on the editor.
		/// </summary>
		/// <remarks>
		/// This property is set by a module in advanced scenarios when an editor is used in a very unusual way.
		/// It can be set once, usually by a creator or by a handler on opening. It is not used internally, it
		/// is just for avoiding module conflicts.
		/// <para>Example scenario: <b>PowerShellFar</b> may use editors as command consoles. On opening it
		/// attaches a host object which is subscribed to the editor events. This approach makes impossible
		/// to attach yet another editor host and prevents advanced use of the editor from other modules
		/// (if they also follow this technique of attaching a host).
		/// </para>
		/// </remarks>
		public object Host
		{
			get { return _Host; }
			set
			{
				if (_Host != null)
					throw new InvalidOperationException();

				_Host = value;
			}
		}
		object _Host;
		/// <summary>
		/// Gets text with the default line separator.
		/// </summary>
		public string GetText()
		{
			return GetText(Environment.NewLine);
		}
		/// <summary>
		/// Gets text with the specified line separator.
		/// </summary>
		/// <param name="separator">Line separator. null ~ default.</param>
		public abstract string GetText(string separator);
		/// <summary>
		/// Sets the new text.
		/// </summary>
		/// <param name="text">New text.</param>
		/// <remarks>
		/// There is no selection after this.
		/// </remarks>
		public abstract void SetText(string text);
		/// <summary>
		/// Opens the editor.
		/// </summary>
		/// <remarks>
		/// It is the same as <see cref="Open(OpenMode)"/> with open mode <see cref="OpenMode.None"/>.
		/// See remarks there.
		/// </remarks>
		public abstract void Open();
		/// <summary>
		/// Opens the editor.
		/// </summary>
		/// <param name="mode">The open mode.</param>
		/// <remarks>
		/// To open an editor you should create an editor operator by <see cref="IFar.CreateEditor"/>,
		/// set at least its <see cref="FileName"/> and optionally: <see cref="DeleteSource"/>,
		/// <see cref="DisableHistory"/>, <see cref="Switching"/>, <see cref="Title"/>,
		/// and <see cref="Window"/>. Then this method is called.
		/// <para>
		/// If the file is already opened in an editor then this instance should not be used after opening
		/// because technically an editor was not opened but reused. The safe way is to get the current
		/// <see cref="IFar.Editor"/> after opening and work with it.
		/// </para>
		/// </remarks>
		public abstract void Open(OpenMode mode);
		/// <summary>
		/// Begins an undo block.
		/// </summary>
		public abstract void BeginUndo();
		/// <summary>
		/// Ends an undo block.
		/// </summary>
		public abstract void EndUndo();
		/// <summary>
		/// Invokes undo.
		/// </summary>
		public abstract void Undo();
		/// <summary>
		/// Invokes redo.
		/// </summary>
		public abstract void Redo();
		/// <summary>
		/// Opens and returns a writer for output text at the caret position of the current editor.
		/// </summary>
		/// <remarks>
		/// It is not recommended to change the caret position during writing,
		/// but it seems to be safe to do so if you <c>Flush()</c> the writer before the change.
		/// </remarks>
		/// <returns>Opened writer. It has to be closed after use.</returns>
		public abstract TextWriter OpenWriter();
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
		/// There is only a special event <see cref="IEditorBase.CtrlCPressed"/>
		/// that can be used for example for stopping the mode by a user.
		/// </para>
		/// <ul>
		/// <li>Nested calls of are not allowed.</li>
		/// <li>Use this mode only when it is absolutely needed.</li>
		/// <li>Module <b>PowerShellFar</b> uses this mode for asynchronous editor consoles.</li>
		/// </ul>
		/// </remarks>
		public abstract void BeginAsync();
		/// <summary>
		/// Ends asynchronous mode.
		/// </summary>
		/// <remarks>
		/// It must be called after <see cref="BeginAsync"/> when asynchronous operations complete.
		/// Note: it is OK to call it when asynchronous mode is already stopped or even was not started.
		/// </remarks>
		public abstract void EndAsync();
		/// <summary>
		/// Tells to enable or disable the caret position beyond end of lines.
		/// Set it before or after opening.
		/// </summary>
		public abstract bool IsVirtualSpace { get; set; }
		/// <summary>
		/// Tells to show or hide white space symbols.
		/// Set it before or after opening.
		/// </summary>
		public abstract bool ShowWhiteSpace { get; set; }
		/// <summary>
		/// Tells to write BOM on saving.
		/// Set it before or after opening.
		/// </summary>
		public abstract bool WriteByteOrderMark { get; set; }
		/// <summary>
		/// Gets the selected text with the default line separator.
		/// </summary>
		public string GetSelectedText()
		{
			return GetSelectedText(Environment.NewLine);
		}
		/// <summary>
		/// Gets the selected text with the specified line separator.
		/// </summary>
		/// <param name="separator">Line separator. null ~ default.</param>
		public abstract string GetSelectedText(string separator);
		/// <summary>
		/// Sets (replaces) the selected text.
		/// </summary>
		/// <param name="text">New text.</param>
		/// <seealso cref="UnselectText"/>
		public abstract void SetSelectedText(string text);
		/// <summary>
		/// Selects the specified stream of text.
		/// </summary>
		/// <param name="column1">Column 1.</param>
		/// <param name="line1">Line 1.</param>
		/// <param name="column2">Column 2.</param>
		/// <param name="line2">Line 2.</param>
		/// <seealso cref="UnselectText"/>
		public void SelectText(int column1, int line1, int column2, int line2)
		{
			SelectText(column1, line1, column2, line2, PlaceKind.Stream);
		}
		/// <summary>
		/// Selects the specified place of text.
		/// </summary>
		/// <param name="column1">Column 1.</param>
		/// <param name="line1">Line 1.</param>
		/// <param name="column2">Column 2.</param>
		/// <param name="line2">Line 2.</param>
		/// <param name="kind">Selected place kind.</param>
		/// <remarks>
		/// Columns are given in editor coordinates for stream selection
		/// and in screen coordinates for column selection.
		/// </remarks>
		/// <seealso cref="ConvertColumnEditorToScreen"/>
		/// <seealso cref="UnselectText"/>
		public abstract void SelectText(int column1, int line1, int column2, int line2, PlaceKind kind);
		/// <summary>
		/// Selects all text.
		/// </summary>
		public abstract void SelectAllText();
		/// <summary>
		/// Turns the text selection off.
		/// </summary>
		/// <remarks>
		/// To delete the selected text use <see cref="DeleteText"/>.
		/// </remarks>
		public abstract void UnselectText();
		/// <summary>
		/// Gets true if selection exists.
		/// </summary>
		public abstract bool SelectionExists { get; }
		/// <summary>
		/// Gets the selection kind.
		/// </summary>
		public abstract PlaceKind SelectionKind { get; }
		/// <summary>
		/// Gets the selected place.
		/// </summary>
		/// <remarks>
		/// The returned columns are given in editor coordinates for any kind of selection.
		/// </remarks>
		/// <seealso cref="ConvertColumnEditorToScreen"/>
		public abstract Place SelectionPlace { get; }
		/// <summary>
		/// Gets the selected point.
		/// </summary>
		public abstract Point SelectionPoint { get; }
		/// <summary>
		/// Gets or sets editor word delimiters.
		/// Set it before or after opening.
		/// </summary>
		public abstract string WordDiv { get; set; } //! see _100324_160008
		/// <summary>
		/// Gets the bookmark operator.
		/// </summary>
		public abstract IEditorBookmark Bookmark { get; }
		/// <summary>
		/// Gets the opening time of the instance.
		/// </summary>
		public abstract DateTime TimeOfOpen { get; }
		/// <summary>
		/// Gets the saving time of the instance.
		/// </summary>
		/// <remarks>
		/// If the editor has not been saved at least once then it is equal to <c>DateTime.MinValue</c>.
		/// </remarks>
		public abstract DateTime TimeOfSave { get; }
		/// <summary>
		/// Gets count of changes.
		/// </summary>
		/// <remarks>
		/// It is designed for the Vessel module and not recommended for public use.
		/// </remarks>
		public abstract int KeyCount { get; }
		/// <summary>
		/// Makes the instance window active.
		/// </summary>
		/// <remarks>It may throw if the window cannot be activated.</remarks>
		public abstract void Activate();
		/// <include file='doc.xml' path='doc/Data/*'/>
		public Hashtable Data { get { return _Data ?? (_Data = new Hashtable()); } }
		Hashtable _Data;
		/// <summary>
		/// Returns color spans of the specified line.
		/// </summary>
		/// <param name="line">Index of the line.</param>
		public abstract IList<EditorColorInfo> GetColors(int line);
		/// <summary>
		/// Adds the drawer to this editor.
		/// </summary>
		/// <param name="drawer">The drawer.</param>
		public abstract void AddDrawer(IModuleDrawer drawer);
		/// <summary>
		/// Removes the drawer from this editor.
		/// </summary>
		/// <param name="id">The drawer ID.</param>
		public abstract void RemoveDrawer(Guid id);
	}

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

	/// <summary>
	/// Editor change constants.
	/// </summary>
	public enum EditorChangeKind
	{
		///
		LineChanged,
		///
		LineAdded,
		///
		LineRemoved
	}

	/// <summary>
	/// Arguments of editor changed event.
	/// </summary>
	public sealed class EditorChangedEventArgs : EventArgs
	{
		/// <param name="kind">See <see cref="Kind"/></param>
		/// <param name="line">See <see cref="Line"/></param>
		public EditorChangedEventArgs(EditorChangeKind kind, int line)
		{
			Kind = kind;
			Line = line;
		}
		/// <summary>
		/// Gets the editor change kind.
		/// </summary>
		public EditorChangeKind Kind { get; private set; }
		/// <summary>
		/// Gets the changed line index.
		/// </summary>
		public int Line { get; private set; }
	}

	/// <summary>
	/// Arguments of editor saving event.
	/// </summary>
	public sealed class EditorSavingEventArgs : EventArgs
	{
		/// <param name="fileName">See <see cref="FileName"/></param>
		/// <param name="codePage">See <see cref="CodePage"/></param>
		public EditorSavingEventArgs(string fileName, int codePage)
		{
			FileName = fileName;
			CodePage = codePage;
		}
		/// <summary>
		/// Gets the file name being saved.
		/// </summary>
		public string FileName { get; private set; }
		/// <summary>
		/// Gets the code page used on saving.
		/// </summary>
		public int CodePage { get; private set; }
	}

	/// <summary>
	/// Abstract line in various text and line editors.
	/// </summary>
	/// <remarks>
	/// It can be:
	/// *) an item of <see cref="IEditor.Lines"/> or <see cref="IEditor.SelectedLines"/> in <see cref="IEditor"/>;
	/// *) the command line <see cref="IFar.CommandLine"/>;
	/// *) <see cref="IEditable.Line"/> of <see cref="IEdit"/>) or <see cref="IComboBox"/> in a dialog.
	/// </remarks>
	public abstract class ILine
	{
		/// <summary>
		/// Gets the line index in the source editor.
		/// </summary>
		/// <remarks>
		/// It returns -1 for the editor current line, the command line, and dialog edit lines.
		/// </remarks>
		public virtual int Index { get { return -1; } }
		/// <summary>
		/// Gets or sets the line text.
		/// </summary>
		/// <seealso cref="ActiveText"/>
		/// <seealso cref="SelectedText"/>
		public abstract string Text { get; set; }
		/// <summary>
		/// Gets or sets (replaces) the selected text.
		/// </summary>
		/// <remarks>
		/// If there is no selection then <c>get</c> returns null, <c>set</c> throws.
		/// </remarks>
		/// <seealso cref="ActiveText"/>
		/// <seealso cref="Text"/>
		public abstract string SelectedText { get; set; }
		/// <summary>
		/// Gets or sets the caret position.
		/// </summary>
		/// <remarks>
		/// Returns -1 if it is an editor line and it is not current.
		/// Setting of a negative value moves the caret to the end.
		/// </remarks>
		public abstract int Caret { get; set; }
		/// <summary>
		/// Inserts text at the caret position.
		/// </summary>
		/// <param name="text">String to insert to the line.</param>
		/// <remarks>
		/// In the editor this method should not be used for the current line only.
		/// </remarks>
		public abstract void InsertText(string text);
		/// <summary>
		/// Selects the span of text in the current editor line, the command line, or the dialog line.
		/// </summary>
		/// <param name="startPosition">Start position.</param>
		/// <param name="endPosition">End position, not included into the span.</param>
		public abstract void SelectText(int startPosition, int endPosition);
		/// <summary>
		/// Turns selection off in the current editor line, the command line, or the dialog line.
		/// </summary>
		public abstract void UnselectText();
		/// <summary>
		/// Gets the text length.
		/// </summary>
		/// <remarks>
		/// Use it instead of more expensive <see cref="Text"/> in cases when just length is needed.
		/// </remarks>
		public abstract int Length { get; }
		/// <summary>
		/// Gets the parent window kind (<c>Editor</c>, <c>Panels</c>, <c>Dialog</c>).
		/// </summary>
		public abstract WindowKind WindowKind { get; }
		/// <summary>
		/// Gets the selection span.
		/// </summary>
		/// <remarks>
		/// If selection does not exists then returned position and length values are negative.
		/// </remarks>
		public abstract Span SelectionSpan { get; }
		/// <summary>
		/// Gets or sets <see cref="SelectedText"/> if any, otherwise gets or sets <see cref="Text"/>.
		/// </summary>
		public string ActiveText
		{
			get
			{
				return SelectedText ?? Text;
			}
			set
			{
				if (SelectionSpan.Length < 0)
					Text = value;
				else
					SelectedText = value;
			}
		}
		/// <summary>
		/// Returns the line text.
		/// </summary>
		public sealed override string ToString()
		{
			return Text;
		}
		/// <summary>
		/// Gets the match for the current caret position.
		/// </summary>
		/// <param name="regex">Regular expression that defines "words".</param>
		/// <returns>The found match or null if the caret is not at any "word".</returns>
		/// <remarks>
		/// This methods is useful for the common task of getting the current "word".
		/// In the editor it should be called on the current line only.
		/// "Words" to look for are defined by a regular expression.
		/// </remarks>
		public Match MatchCaret(Regex regex)
		{
			if (regex == null) throw new ArgumentNullException("regex");

			int caret = Caret;
			for (var match = regex.Match(Text); match.Success; match = match.NextMatch())
				if (caret <= match.Index + match.Length)
					return caret < match.Index ? null : match;

			return null;
		}
		/// <summary>
		/// Gets true if the text is read only.
		/// </summary>
		public virtual bool IsReadOnly { get { return false; } }
	}

	/// <summary>
	/// Arguments of key events.
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
	/// Arguments of mouse events.
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
		/// <param name="value">The same value assigned to all properties.</param>
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
		/// Gets or sets the caret line index.
		/// </summary>
		public int CaretLine { get; set; }
		/// <summary>
		/// Gets or sets the caret character index.
		/// </summary>
		public int CaretColumn { get; set; }
		/// <summary>
		/// Gets or sets the caret screen column index.
		/// </summary>
		public int CaretScreenColumn { get; set; }
		/// <summary>
		/// Gets or sets the first visible line index.
		/// </summary>
		public int VisibleLine { get; set; }
		/// <summary>
		/// Gets or sets the first visible character index.
		/// </summary>
		public int VisibleChar { get; set; }
		/// <include file='doc.xml' path='doc/OpEqual/*'/>
		public static bool operator ==(TextFrame left, TextFrame right)
		{
			return
				left.CaretLine == right.CaretLine &&
				left.CaretColumn == right.CaretColumn &&
				left.CaretScreenColumn == right.CaretScreenColumn &&
				left.VisibleLine == right.VisibleLine &&
				left.VisibleChar == right.VisibleChar;
		}
		/// <include file='doc.xml' path='doc/OpNotEqual/*'/>
		public static bool operator !=(TextFrame left, TextFrame right)
		{
			return !(left == right);
		}
		/// <inheritdoc/>
		public override bool Equals(Object obj)
		{
			return obj != null && obj.GetType() == typeof(TextFrame) && this == (TextFrame)obj;
		}
		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return CaretLine | (CaretColumn << 16);
		}
		/// <summary>
		/// Returns the string "(({0}/{1}, {2})({3}, {4}))", CaretColumn, CaretScreenColumn, CaretLine, VisibleChar, VisibleLine.
		/// </summary>
		public override string ToString()
		{
			return string.Format(null, "(({0}/{1}, {2})({3}, {4}))", CaretColumn, CaretScreenColumn, CaretLine, VisibleChar, VisibleLine);
		}
	}

	/// <summary>
	/// Editor line color span.
	/// </summary>
	public class EditorColor
	{
		/// <param name="line">See <see cref="Line"/></param>
		/// <param name="start">See <see cref="Start"/></param>
		/// <param name="end">See <see cref="End"/></param>
		/// <param name="foreground">See <see cref="Foreground"/></param>
		/// <param name="background">See <see cref="Background"/></param>
		public EditorColor(int line, int start, int end, ConsoleColor foreground, ConsoleColor background)
		{
			Line = line;
			Start = start;
			End = end;
			Foreground = foreground;
			Background = background;
		}
		/// <summary>
		/// Line index.
		/// </summary>
		public int Line { get; private set; }
		/// <summary>
		/// Start position.
		/// </summary>
		public int Start { get; private set; }
		/// <summary>
		/// End position, not included into the span, <c>End - Start</c> is the span length.
		/// </summary>
		public int End { get; private set; }
		/// <summary>
		/// Foreground color. Black on black is the special case.
		/// </summary>
		public ConsoleColor Foreground { get; private set; }
		/// <summary>
		/// Background color. Black on black is the special case.
		/// </summary>
		public ConsoleColor Background { get; private set; }
		/// <summary>
		/// Returns the string "({0}, {1}) {2}/{3}", Start, End, Foreground, Background.
		/// </summary>
		public override string ToString()
		{
			return string.Format(null, "({0}, {1}) {2}/{3}", Start, End, Foreground, Background);
		}
	}

	/// <summary>
	/// Editor line color info.
	/// </summary>
	public class EditorColorInfo : EditorColor
	{
		/// <param name="line">See <see cref="EditorColor.Line"/></param>
		/// <param name="start">See <see cref="EditorColor.Start"/></param>
		/// <param name="end">See <see cref="EditorColor.End"/></param>
		/// <param name="foreground">See <see cref="EditorColor.Foreground"/></param>
		/// <param name="background">See <see cref="EditorColor.Background"/></param>
		/// <param name="owner">See <see cref="Owner"/></param>
		/// <param name="priority">See <see cref="Priority"/></param>
		public EditorColorInfo(int line, int start, int end, ConsoleColor foreground, ConsoleColor background, Guid owner, int priority)
			: base(line, start, end, foreground, background)
		{
			Owner = owner;
			Priority = priority;
		}
		/// <summary>
		/// Color owner ID.
		/// </summary>
		public Guid Owner { get; private set; }
		/// <summary>
		/// Color priority.
		/// </summary>
		public int Priority { get; private set; }
		/// <summary>
		/// Returns the string "{0} {1} {2} ({3}, {4}) {5}/{6}", Priority, Owner, Line, Start, End, Foreground, Background.
		/// </summary>
		public override string ToString()
		{
			return string.Format(null, "{0} {1} {2} ({3}, {4}) {5}/{6}", Priority, Owner, Line, Start, End, Foreground, Background);
		}
	}
}
