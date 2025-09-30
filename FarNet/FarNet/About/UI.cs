using FarNet.Works;

namespace FarNet;

/// <summary>
/// Low level UI.
/// </summary>
/// <remarks>
/// It is exposed as <see cref="IFar.UI"/>.
/// This API is mostly for internal use and modules should use it sparingly.
/// </remarks>
public abstract class IUserInterface
{
	/// <summary>
	/// INTERNAL
	/// Gets the text of the specified console buffer line.
	/// </summary>
	/// <param name="lineIndex">The line index. A negative index Y is treated as H + Y, where H is the buffer height.</param>
	/// <returns>The specified line text. End spaces are trimmed.</returns>
	public abstract string GetBufferLineText(int lineIndex);

	/// <summary>
	/// Gets the Far main window handle.
	/// </summary>
	public abstract IntPtr MainWindowHandle { get; }

	/// <summary>
	/// Gets or sets the window title.
	/// </summary>
	public abstract string WindowTitle { get; set; }

	/// <summary>
	/// Gets or sets the background color of text to be written.
	/// </summary>
	public abstract ConsoleColor BackgroundColor { get; set; }

	/// <summary>
	/// Gets or sets the foreground color of text to be written.
	/// </summary>
	public abstract ConsoleColor ForegroundColor { get; set; }

	/// <summary>
	/// Gets or sets the cursor size.
	/// </summary>
	public abstract int CursorSize { get; set; }

	/// <summary>
	/// Gets or sets the cursor position in the buffer.
	/// </summary>
	public abstract Point BufferCursor { get; set; }

	/// <summary>
	/// Gets or sets the cursor position in the window.
	/// </summary>
	public abstract Point WindowCursor { get; set; }

	/// <summary>
	/// Gets or sets the buffer size.
	/// </summary>
	public abstract Point BufferSize { get; set; }

	/// <summary>
	/// Gets the window place in the buffer.
	/// </summary>
	public abstract Place WindowPlace { get; }

	/// <summary>
	/// Gets the window left top point in the buffer.
	/// </summary>
	public abstract Point WindowPoint { get; set; }

	/// <summary>
	/// Gets the window size.
	/// </summary>
	public abstract Point WindowSize { get; set; }

	/// <summary>
	/// Gets the maximum physical window size.
	/// </summary>
	public abstract Point MaxPhysicalWindowSize { get; }

	/// <summary>
	/// Gets the maximum window size.
	/// </summary>
	public abstract Point MaxWindowSize { get; }

	/// <summary>
	/// Gets true if a key press is available in the input buffer.
	/// </summary>
	public abstract bool KeyAvailable { get; }

	/// <summary>
	/// Flushes the input buffer. All input records currently in the input buffer are discarded.
	/// </summary>
	public abstract void FlushInputBuffer();

	/// <summary>
	/// Reads a key from the input buffer.
	/// </summary>
	/// <param name="options">Read key options. See remarks.</param>
	/// <remarks>Either <see cref="ReadKeyOptions.IncludeKeyDown"/>, <see cref="ReadKeyOptions.IncludeKeyUp"/> or both must be specified.</remarks>
	public abstract KeyInfo ReadKey(ReadKeyOptions options);

	///
	public abstract BufferCell[,] GetBufferContents(Place rectangle);

	///
	public abstract void ScrollBufferContents(Place source, Point destination, Place clip, BufferCell fill);

	///
	public abstract void SetBufferContents(Point origin, BufferCell[,] contents);

	///
	public abstract void SetBufferContents(Place rectangle, BufferCell fill);

	/// <summary>
	/// INTERNAL
	/// </summary>
	public abstract void Break();

	/// <summary>
	/// Draws the changes done by the <see cref="DrawColor"/> and <see cref="DrawPalette"/>.
	/// </summary>
	public abstract void Draw();

	/// <summary>
	/// Draws at the specified position with defined colors (in the internal buffer).
	/// </summary>
	/// <include file='doc.xml' path='doc/LT/*'/>
	/// <include file='doc.xml' path='doc/Colors/*'/>
	/// <param name="text">Text.</param>
	/// <seealso cref="GetPaletteForeground"/>
	/// <seealso cref="GetPaletteBackground"/>
	/// <remarks>
	/// When all drawing operations are done call the <see cref="Draw"/>.
	/// </remarks>
	public abstract void DrawColor(int left, int top, ConsoleColor foregroundColor, ConsoleColor backgroundColor, string text);

	/// <summary>
	/// Draws at the specified position using Far palette colors (in the internal buffer).
	/// </summary>
	/// <include file='doc.xml' path='doc/LT/*'/>
	/// <param name="paletteColor">Palette color.</param>
	/// <param name="text">Text.</param>
	/// <remarks>
	/// When all drawing operations are done call the <see cref="Draw"/>.
	/// </remarks>
	public abstract void DrawPalette(int left, int top, PaletteColor paletteColor, string text);

	/// <summary>
	/// Restores previously saved by <see cref="SaveScreen"/> screen area.
	/// </summary>
	/// <param name="screen">
	/// A handle received from <c>SaveScreen</c>.
	/// This handle is no longer usable after calling.
	/// </param>
	/// <remarks>
	/// For performance sake it redraws only the modified screen area.
	/// But if there was screen output produced by an external program,
	/// it can't calculate this area correctly. In that case you have to
	/// call it with <c>screen</c> = 0 and then with an actual screen handle.
	/// </remarks>
	public abstract void RestoreScreen(IntPtr screen);

	/// <summary>
	/// Saves screen area.
	/// You always have to call <see cref="RestoreScreen"/>.
	/// </summary>
	/// <include file='doc.xml' path='doc/LTRB/*'/>
	/// <returns>A handle for restoring the screen.</returns>
	/// <remarks>
	/// If <c>right</c> and <c>bottom</c> are equal to -1,
	/// they are replaced with screen right and bottom coordinates.
	/// So <c>SaveScreen(0,0,-1,-1)</c> will save the entire screen.
	/// </remarks>
	public abstract IntPtr SaveScreen(int left, int top, int right, int bottom);

	/// <summary>
	/// INTERNAL
	/// </summary>
	/// <param name="n">0: new line; 1: no new line.</param>
	public abstract void GetUserScreen(int n);

	/// <summary>
	/// INTERNAL
	/// </summary>
	/// <param name="n">0: new line; 1: no new line.</param>
	public abstract void SetUserScreen(int n);

	/// <summary>
	/// Copies the current screen contents to the user screen buffer
	/// (which is displayed when the panels are switched off).
	/// </summary>
	/// <remarks>
	/// Normally it is called after <see cref="ShowUserScreen"/> and direct console writing.
	/// Note: try to avoid these low level operations.
	/// </remarks>
	public abstract void SaveUserScreen();

	/// <summary>
	/// Copies the current user screen buffer to console screen
	/// (which is displayed when the panels are switched off).
	/// </summary>
	/// <remarks>
	/// Normally it is called before direct console screen operations and <see cref="SaveUserScreen"/> has to be called after.
	/// Note: try to avoid these low level operations.
	/// </remarks>
	public abstract void ShowUserScreen();

	/// <summary>
	/// Writes text to the console with the current colors.
	/// </summary>
	/// <param name="text">The text to be written.</param>
	/// <remarks>
	/// This method is called from the panels area in order to simulate classic console output.
	/// Calls from other areas are allowed but console output is unexpected, difficult to see,
	/// and basically should be avoided.
	/// <para>
	/// In PowerShell scripts consider to use <c>Write-Host</c> instead of this method.
	/// </para>
	/// </remarks>
	public abstract void Write(string text);

	/// <summary>
	/// Writes an empty line to the console.
	/// </summary>
	public void WriteLine()
	{ Write(Environment.NewLine); }

	/// <summary>
	/// Writes text and a new line to the console with the current colors.
	/// </summary>
	/// <inheritdoc cref="Write(string)"/>
	public void WriteLine(string text)
	{ Write(text + Environment.NewLine); }

	/// <summary>
	/// Writes text to the console with the specified foreground color.
	/// </summary>
	/// <inheritdoc cref="Write(string)"/>
	/// <param name="text">The text to be written.</param>
	/// <param name="foregroundColor">Text foreground color.</param>
	public abstract void Write(string text, ConsoleColor foregroundColor);

	/// <summary>
	/// Writes text and a new line to the console with the specified foreground color.
	/// </summary>
	/// <inheritdoc cref="Write(string, ConsoleColor)"/>
	public void WriteLine(string text, ConsoleColor foregroundColor)
	{ Write(text + Environment.NewLine, foregroundColor); }

	/// <summary>
	/// Writes text to the console with the specified foreground and background colors.
	/// </summary>
	/// <inheritdoc cref="Write(string)"/>
	/// <param name="text">The text to be written.</param>
	/// <param name="foregroundColor">Text foreground color.</param>
	/// <param name="backgroundColor">Text background color.</param>
	public abstract void Write(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor);

	/// <summary>
	/// Writes text and a new line to the console with the specified foreground and background colors.
	/// </summary>
	/// <inheritdoc cref="Write(string, ConsoleColor, ConsoleColor)"/>
	public void WriteLine(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
	{ Write(text + Environment.NewLine, foregroundColor, backgroundColor); }

	/// <summary>
	/// Tells the icon of not active window to flash.
	/// </summary>
	/// <remarks>
	/// It is normally used to show that an operation is complete.
	/// If the window is not active the icon flashes a few times and then remains highlighted until the window gets focus.
	/// </remarks>
	public abstract void SetProgressFlash();

	/// <summary>
	/// Sets the type and state of the progress indicator displayed on a taskbar button of the main application window.
	/// </summary>
	/// <param name="state">Progress state of the progress button.</param>
	public abstract void SetProgressState(TaskbarProgressBarState state);

	/// <summary>
	/// Displays or updates a progress bar hosted in a taskbar button of the main application window
	/// to show the specific percentage completed of the full operation.
	/// </summary>
	/// <param name="currentValue">Indicates the proportion of the operation that has been completed.</param>
	/// <param name="maximumValue">Specifies the value <c>currentValue</c> will have when the operation is complete.</param>
	public abstract void SetProgressValue(int currentValue, int maximumValue);

	/// <summary>
	/// Returns background color of Far palette.
	/// </summary>
	/// <param name="paletteColor">Palette color.</param>
	public abstract ConsoleColor GetPaletteBackground(PaletteColor paletteColor);

	/// <summary>
	/// Returns foreground color of Far palette.
	/// </summary>
	/// <param name="paletteColor">Palette color.</param>
	public abstract ConsoleColor GetPaletteForeground(PaletteColor paletteColor);

	/// <summary>
	/// Clears the buffer and the user screen.
	/// </summary>
	public abstract void Clear();

	/// <summary>
	/// Redraws all windows.
	/// </summary>
	/// <remarks>
	/// This method helps in rare cases when all windows should be redrawn
	/// even those that are normally not, e.g. panels under opened dialogs.
	/// </remarks>
	public abstract void Redraw();

	/// <summary>
	/// Reads all keys from the input buffer and finds one of the given.
	/// </summary>
	/// <param name="keys">Array of keys to find.</param>
	/// <returns>Index of the first found key or -1.</returns>
	/// <remarks>
	/// The input buffer is empty after the call.
	/// </remarks>
	public abstract int ReadKeys(params KeyData[] keys);
}
