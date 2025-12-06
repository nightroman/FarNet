using FarNet.Works;

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
		if (Kit.IsVSCode)
		{
			// normal text
			e.Background1 = ConsoleColor.White;
			e.Foreground1 = ConsoleColor.Black;

			// selected text
			e.Background2 = ConsoleColor.Black;
			e.Foreground2 = ConsoleColor.White;

			// unchanged text
			e.Background3 = ConsoleColor.White;
			e.Foreground3 = ConsoleColor.DarkGray;

			// combo
			e.Background4 = ConsoleColor.White;
			e.Foreground4 = ConsoleColor.Gray;
		}
		else
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
	}

	/// <summary>
	/// .
	/// </summary>
	/// <param name="sender">.</param>
	/// <param name="e">.</param>
	public static void Coloring_TextAsConsole(object? sender, ColoringEventArgs e)
	{
		if (Kit.IsVSCode)
		{
			// normal text
			e.Background1 = ConsoleColor.White;
			e.Foreground1 = ConsoleColor.Black;
		}
		else
		{
			// normal text
			e.Background1 = ConsoleColor.Black;
			e.Foreground1 = ConsoleColor.Gray;
		}
	}
}
