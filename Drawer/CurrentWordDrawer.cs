using FarNet;
using System;
using System.Collections.Generic;

namespace Drawer;

// `Priority = 2` because e.g. PowerShell breakpoints use 1.
[ModuleDrawer(Name = Settings.CurrentWordName, Priority = 2, Id = Settings.CurrentWordGuid)]
public class CurrentWordDrawer : ModuleDrawer
{
	static EditorColor NewColorKeepForeground(int lineIndex, int start, int end, ConsoleColor fg, ConsoleColor bg)
	{
		if (bg == ConsoleColor.Yellow)
		{
			bg = ConsoleColor.Gray;
			if (fg == ConsoleColor.Gray)
				fg = ConsoleColor.Black;
		}
		else
		{
			bg = ConsoleColor.Yellow;
			if (fg == ConsoleColor.Yellow || fg == ConsoleColor.White)
				fg = ConsoleColor.Black;
		}
		return new EditorColor(lineIndex, start, end, fg, bg);
	}

	public override void Invoke(IEditor editor, ModuleDrawerEventArgs e)
	{
		var settings = Settings.Default.GetData().CurrentWord;

		// get current word
		var word = editor.Line.MatchCaret2(settings.WordRegex2, out int index);
		if (index < 0)
			return;

		var caret = editor.Caret;

		// color occurrences
		var colors = new List<EditorColorInfo>();
		bool hasColorer = editor.HasColorer();
		foreach (var line in e.Lines)
		{
			var text = line.Text2;
			if (text.Length == 0 || text.IndexOf(word, StringComparison.OrdinalIgnoreCase) < 0)
				continue;

			// find line words
			var matches = settings.WordRegex2.EnumerateMatches(text);
			if (!matches.MoveNext())
				continue;

			// get original colors
			if (hasColorer)
				editor.GetColors(line.Index, colors);

			// line word matches
			for (bool next = true; next; next = matches.MoveNext())
			{
				var match = matches.Current;
				var value = text.Slice(match.Index, match.Length);

				// skip different words
				if (!value.Equals(word, StringComparison.OrdinalIgnoreCase))
					continue;

				// the match position
				var myStart = match.Index;
				var myEnd = match.Index + match.Length;

				// skip current word at the caret
				if (settings.ExcludeCurrent && line.Index == caret.Y)
				{
					if (caret.X >= myStart && caret.X <= myEnd)
						continue;
				}

				if (hasColorer)
				{
					foreach (var color in colors)
					{
						// case: color totally covers the match
						if (color.Start <= myStart && color.End >= myEnd)
						{
							e.Colors.Add(NewColorKeepForeground(
								line.Index,
								myStart,
								myEnd,
								color.Foreground,
								color.Background));
							continue;
						}

						// 1 of 2: color starts after the match, handle the left part of match
						if (color.Start > myStart && color.Start < myEnd)
						{
							var st = color.Start;
							var en = Math.Min(color.End, myEnd);
							if (st < en)
								e.Colors.Add(NewColorKeepForeground(
									line.Index,
									st,
									en,
									color.Foreground,
									color.Background));
						}

						// 2 of 2: color ends before the match, handle the right part of match
						if (color.End < myEnd && color.End > myStart)
						{
							var st = Math.Max(color.Start, myStart);
							var en = color.End;
							if (st < en)
								e.Colors.Add(NewColorKeepForeground(
									line.Index,
									st,
									en,
									color.Foreground,
									color.Background));
						}
					}
				}
				else
				{
					e.Colors.Add(new EditorColor(
						line.Index,
						myStart,
						myEnd,
						settings.ColorForeground,
						settings.ColorBackground));
				}
			}
		}
	}
}
