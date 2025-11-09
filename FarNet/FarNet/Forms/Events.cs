namespace FarNet.Forms;

/// <summary>
/// Common UI event handlers.
/// </summary>
public static class Events
{
	/// <summary>
	/// .
	/// </summary>
	/// <param name="sender">.</param>
	/// <param name="e">.</param>
	public static void Coloring_EditAsConsole(object? sender, ColoringEventArgs e)
	{
		// normal text
		e.Background1 = ConsoleColor.Black;
		e.Foreground1 = ConsoleColor.Gray;
		// selected text
		e.Background2 = ConsoleColor.White;
		e.Foreground2 = ConsoleColor.Black;
		// unchanged text
		e.Background3 = ConsoleColor.Black;
		e.Foreground3 = ConsoleColor.DarkGray;
		// combo
		e.Background4 = ConsoleColor.Black;
		e.Foreground4 = ConsoleColor.Gray;
	}

	/// <summary>
	/// .
	/// </summary>
	/// <param name="sender">.</param>
	/// <param name="e">.</param>
	public static void Coloring_TextAsConsole(object? sender, ColoringEventArgs e)
	{
		// normal text
		e.Background1 = ConsoleColor.Black;
		e.Foreground1 = ConsoleColor.Gray;
	}

	/// <summary>
	/// .
	/// </summary>
	/// <param name="sender">.</param>
	/// <param name="e">.</param>
	[Obsolete("Use IDialog.NoClickOutside")]
	public static void MouseClicked_IgnoreOutside(object? sender, MouseClickedEventArgs e)
	{
		if (e.Control is null)
			e.Ignore = true;
	}
}
