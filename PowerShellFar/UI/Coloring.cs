
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet.Forms;
using System;

namespace PowerShellFar.UI
{
	static class Coloring
	{
		public static void ColorEditAsConsole(object sender, ColoringEventArgs e)
		{
			// normal text
			e.Background1 = ConsoleColor.Black;
			e.Foreground1 = ConsoleColor.Gray;
			// selected text
			e.Background2 = ConsoleColor.White;
			e.Foreground2 = ConsoleColor.Black;
			// unchanged text
			e.Background3 = ConsoleColor.Black;
			e.Foreground3 = ConsoleColor.Gray;
			// combo
			e.Background4 = ConsoleColor.Black;
			e.Foreground4 = ConsoleColor.Gray;
		}
		public static void ColorTextAsConsole(object sender, ColoringEventArgs e)
		{
			// normal text
			e.Background1 = ConsoleColor.Black;
			e.Foreground1 = ConsoleColor.Gray;
		}
	}
}
