
// FarNet module Drawer
// Copyright (c) Roman Kuzmin

using System;
using System.Text.RegularExpressions;

namespace FarNet.Drawer
{
	// `Priority = 2` because e.g. PowerShell breakpoints use 1.
	[System.Runtime.InteropServices.Guid(Settings.CurrentWordGuid)]
	[ModuleDrawer(Name = Settings.CurrentWordName, Priority = 2)]
	public class CurrentWordDrawer : ModuleDrawer
	{
		public override void Invoke(IEditor editor, ModuleDrawerEventArgs e)
		{
			var sets = Settings.Default.GetData().CurrentWord;

			// get current word
			var regex = new Regex(sets.WordRegex);
			var match = editor.Line.MatchCaret(regex);
			if (match == null)
				return;

			var word = match.Value;

			// color occurrences
			foreach (var line in e.Lines)
			{
				var text = line.Text;
				if (text.Length == 0 || text.IndexOf(word, StringComparison.OrdinalIgnoreCase) < 0)
					continue;

				for (match = regex.Match(text); match.Success; match = match.NextMatch())
					if (match.Value.Equals(word, StringComparison.OrdinalIgnoreCase))
						e.Colors.Add(new EditorColor(line.Index, match.Index, match.Index + match.Length, sets.ColorForeground, sets.ColorBackground));
			}
		}
	}
}
