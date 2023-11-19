
// FarNet module Drawer
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FarNet.Drawer;

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
		var sets = Settings.Default.GetData().CurrentWord;

		// get current word
		var regex = new Regex(sets.WordRegex);
		var match = editor.Line.MatchCaret(regex);
		if (match == null)
			return;

		var word = match.Value;
		var caret = editor.Caret;

		// color occurrences
		var colors = new List<EditorColorInfo>();
		bool hasColorer = editor.HasColorer();
		foreach (var line in e.Lines)
		{
			var text = line.Text;
			if (text.Length == 0 || text.IndexOf(word, StringComparison.OrdinalIgnoreCase) < 0)
				continue;

			// find line words
			match = regex.Match(text);
			if (!match.Success)
				continue;

			// get original colors
			if (hasColorer)
				editor.GetColors(line.Index, colors);

			// line word matches
			for (; match.Success; match = match.NextMatch())
			{
				// skip different words
				if (!match.Value.Equals(word, StringComparison.OrdinalIgnoreCase))
					continue;

				// the match position
				var myStart = match.Index;
				var myEnd = match.Index + match.Length;

				// skip current word at the caret
				if (sets.ExcludeCurrent && line.Index == caret.Y)
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
						sets.ColorForeground,
						sets.ColorBackground));
				}
			}
		}
	}
}
