using FarNet.Works;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace FarNet;

/// <summary>
/// Editor operator. Exposed as <see cref="IFar.Editor"/>. Created by <see cref="IFar.CreateEditor"/>.
/// </summary>
/// <remarks>
/// Normally this object should be created or requested, used instantly and never kept for future use.
/// When you need an editor operator call <see cref="IFar.Editor"/> or <see cref="IFar.Editors()"/>
/// to get it.
/// <para>
/// Most of methods operate on the specified editor, not necessarily current.
/// Color and bookmark methods operate on the current editor only.
/// </para>
/// <para>
/// The editor has members making it semantically similar to a list of <see cref="ILine"/> lines and strings.
/// These members are: <see cref="Count"/> (line count), <see cref="this[int]"/> (gets a line by its index),
/// <see cref="RemoveAt"/> (removes a line by its index), <see cref="Clear"/> (removes all lines),
/// <see cref="Add"/>\<see cref="Insert"/> (adds/inserts text line(s)).
/// </para>
/// <para>
/// Still, the editor is not a standard list of strings or lines.
/// Standard string list is <see cref="Strings"/>, it has all useful list members implemented.
/// Standard line lists are <see cref="Lines"/> or <see cref="SelectedLines"/>, they have members mostly for reading.
/// </para>
/// </remarks>
public abstract class IEditor : IEditorBase, IFace
{
	/// <inheritdoc/>
	public abstract nint Id { get; }

	/// <inheritdoc/>
	public WindowKind WindowKind => WindowKind.Editor;

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
	/// Gets the caret line.
	/// </summary>
	/// <remarks>
	/// The returned object is rather a dynamic reference to the caret line, not a copy.
	/// If the caret moves to another line then the object operates on a new caret line.
	/// </remarks>
	/// <seealso cref="IFar.Line"/>
	public abstract ILine Line { get; }

	/// <summary>
	/// Gets or sets tab size in spaces in the editor.
	/// </summary>
	public abstract int TabSize { get; set; }

	/// <summary>
	/// Gets or sets expand tabs mode in the editor.
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
	/// Gets the editor window size.
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
	/// Closes the editor.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Changes, if any, are lost. Call <see cref="Save()"/> to save them.
	/// </para>
	/// <para>
	/// The call is ignored if the editor is not opened or already closed.
	/// </para>
	/// </remarks>
	public abstract void Close();

	/// <summary>
	/// Saves changes, if any. Exception on failure.
	/// </summary>
	/// <remarks>
	/// This method does nothing if <see cref="IsModified"/> is false.
	/// </remarks>
	public void Save() => Save(false);

	/// <summary>
	/// Saves the file in the editor even with no changes. Exception on failure.
	/// </summary>
	/// <param name="force">Tells to write the file even if there are no changes.</param>
	public abstract void Save(bool force);

	/// <summary>
	/// Saves the file in the editor as the specified file. Exception on failure.
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
	/// Gets or sets the window title before or after opening.
	/// </summary>
	/// <remarks>
	/// For the current editor setting the title to null or empty restores the original title.
	/// </remarks>
	public abstract string? Title { get; set; }

	/// <summary>
	/// Gets or sets overtype mode.
	/// </summary>
	public abstract bool Overtype { get; set; }

	/// <summary>
	/// Gets true if the text is modified.
	/// </summary>
	/// <seealso cref="TimeOfSave"/>
	public abstract bool IsModified { get; }

	/// <summary>
	/// Gets true if the key bar is shown.
	/// </summary>
	public abstract bool IsKeyBar { get; }

	/// <summary>
	/// Gets true if the title bar is shown.
	/// </summary>
	public abstract bool IsTitleBar { get; }

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
	/// Gets or sets the editor text frame.
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
	public abstract void GoTo(int column, int line);

	/// <summary>
	/// Sets the caret line or posts it for opening.
	/// </summary>
	/// <param name="line">Line index.</param>
	/// <seealso cref="Caret"/>
	/// <seealso cref="Frame"/>
	public abstract void GoToLine(int line);

	/// <summary>
	/// Goes to a character in the caret line.
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
	public object? Host
	{
		get => _Host;
		set => _Host = _Host == null ? value : throw new InvalidOperationException();
	}
	object? _Host;

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
	/// Begins a new undo block.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This call fails if the editor is locked.
	/// </para>
	/// <para>
	/// Ensure <see cref="EndUndo"/> after this call.
	/// </para>
	/// </remarks>
	public abstract void BeginUndo();

	/// <summary>
	/// Ends the current undo block.
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
	/// Opens and returns a writer for output text at the caret position of the editor.
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
	/// This mode is designed for writing to a not current editor or from background jobs.
	/// The editor is partially blocked until the mode is ended by <see cref="EndAsync"/>.
	/// Actual writing happens when the editor has focus, otherwise data are queued.
	/// Use <see cref="Sync"/> in order to write queued data.
	/// <para>
	/// The asynchronous mode appends text to the end, it works like console.
	/// </para>
	/// <para>
	/// Only <c>Insert*</c> methods should be called in the asynchronous mode.
	/// </para>
	/// <para>
	/// Input events (keys, mouse) are disabled in the asynchronous mode.
	/// There only special event is <see cref="IEditorBase.CtrlCPressed"/>.
	/// </para>
	/// <ul>
	/// <li>Nested calls are not allowed.</li>
	/// <li><b>PowerShellFar</b> uses this mode for asynchronous interactives.</li>
	/// </ul>
	/// </remarks>
	public abstract void BeginAsync();

	/// <summary>
	/// Ends the asynchronous mode.
	/// </summary>
	/// <remarks>
	/// It must be called after <see cref="BeginAsync"/> when asynchronous operations complete.
	/// It may be called with no effect when the asynchronous mode is stopped or not started.
	/// </remarks>
	public abstract void EndAsync();

	/// <summary>
	/// Inserts pending text in async mode.
	/// </summary>
	public abstract void Sync();

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
	/// Gets the got focus time of the instance.
	/// </summary>
	public abstract DateTime TimeOfGotFocus { get; }

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

	/// <include file='doc.xml' path='doc/Data/*'/>
	public Hashtable Data => _Data ??= [];
	Hashtable? _Data;

	/// <summary>
	/// Collects color spans of the specified line.
	/// </summary>
	/// <param name="line">Index of the line.</param>
	/// <param name="colors">Line colors.</param>
	public abstract void GetColors(int line, List<EditorColorInfo> colors);

	/// <summary>
	/// INTERNAL
	/// </summary>
	/// <param name="owner">Color owner ID.</param>
	/// <param name="priority">Color priority.</param>
	/// <param name="colors">Color info.</param>
	public abstract void WorksSetColors(Guid owner, int priority, IEnumerable<EditorColor> colors);

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

	/// <summary>
	/// Gets true if the plugin Colorer is enabled.
	/// </summary>
	/// <remarks>
	/// This method gets the result once for this editor, then returns the same value.
	/// </remarks>
	public abstract bool HasColorer();

	/// <summary>
	/// Makes the window current.
	/// </summary>
	public void Activate()
	{
		var myId = Id;
		for (int i = Far.Api.Window.Count - 1; i >= 0; i--)
		{
			if (Far.Api.Window.GetIdAt(i) == myId && Far.Api.Window.GetKindAt(i) == WindowKind.Editor)
			{
				Far.Api.Window.SetCurrentAt(i);
				return;
			}
		}
	}

	/// <summary>
	/// Gets the line text.
	/// </summary>
	/// <param name="line">Line index.</param>
	[Experimental("FarNet250102")]
	public unsafe ReadOnlySpan<char> GetLineText2(int line)
	{
		var (p, n) = Far2.Api.IEditorLineText(Id, line);
		return new((char*)p, n);
	}

	/// <summary>
	/// Sets the line text.
	/// </summary>
	/// <param name="line">Line index.</param>
	/// <param name="text">Line text.</param>
	[Experimental("FarNet250102")]
	public unsafe void SetLineText2(int line, ReadOnlySpan<char> text)
	{
		fixed (char* p = text)
		{
			Far2.Api.IEditorLineText(Id, line, (nint)p, text.Length);
		}
	}

	/// <summary>
	/// Gets the number of change events.
	/// </summary>
	[Experimental("FarNet250106")]
	public abstract int ChangeCount { get; }
}
