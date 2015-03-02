
/*
FarNet module Drawer
Copyright (c) 2012-2015 Roman Kuzmin
*/

using System;
using System.Text.RegularExpressions;

namespace FarNet.Drawer
{
	[System.Runtime.InteropServices.Guid(Settings.CurrentWordGuid)]
	[ModuleDrawer(Name = Settings.CurrentWordName, Priority = 1)]
	public class CurrentWordDrawer : ModuleDrawer
	{
		readonly Regex _regex = new Regex(Settings.Default.CurrentWordPattern);
		readonly ConsoleColor _foreground = Settings.Default.CurrentWordColorForeground;
		readonly ConsoleColor _background = Settings.Default.CurrentWordColorBackground;
		public override void Invoke(object sender, ModuleDrawerEventArgs e)
		{
			var editor = (IEditor)sender;

			// get current word
			var match = editor.Line.MatchCaret(_regex);
			if (match == null)
				return;

			var word = match.Value;

			// color occurrences
			foreach (var line in e.Lines)
			{
				var text = line.Text;
				if (text.Length == 0 || text.IndexOf(word, StringComparison.OrdinalIgnoreCase) < 0)
					continue;

				for (match = _regex.Match(text); match.Success; match = match.NextMatch())
					if (match.Value.Equals(word, StringComparison.OrdinalIgnoreCase))
						e.Colors.Add(new EditorColor(line.Index, match.Index, match.Index + match.Length, _foreground, _background));
			}
		}
	}
}
