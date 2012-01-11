
/*
FarNet module RightWords
Copyright (c) 2011-2012 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace FarNet.RightWords
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	class Highlighter
	{
		readonly MultiSpell Spell = MultiSpell.Get();
		readonly Regex RegexSkip = Actor.GetRegexSkip();
		readonly Regex RegexWord = new Regex(Settings.Default.WordPattern, RegexOptions.IgnorePatternWhitespace);
		readonly HashSet<string> CommonWords = Actor.GetCommonWords();
		readonly ConsoleColor HighlightingBackgroundColor = Settings.Default.HighlightingBackgroundColor;
		readonly ConsoleColor HighlightingForegroundColor = Settings.Default.HighlightingForegroundColor;
		readonly IEditor Editor;
		public Highlighter(IEditor editor)
		{
			Editor = editor;
			editor.Redrawing += Redrawing;
		}
		public bool Disabled { get; set; }
		void Redrawing(object sender, EditorRedrawingEventArgs e)
		{
			// test: try to edit a line with colors with and without Colorer
			if (e.Mode == EditorRedrawMode.Line || e.Mode == EditorRedrawMode.Change)
			{
				HighlightLine(Editor.Caret.Y);
				return;
			}

			int height = Far.Net.UI.WindowSize.Y;
			TextFrame frame = Editor.Frame;
			int lineCount = Editor.Count;

			for (int i = 0; i < height; ++i)
			{
				int index = frame.VisibleLine + i;
				if (index >= lineCount)
					break;

				HighlightLine(index);
			}
		}
		void HighlightLine(int lineIndex)
		{
			// remove colors always ([Home ShiftEnd Del] => existing colors should be removed)
			Editor.RemoveColors(My.Guid, lineIndex, -1);

			if (Disabled)
				return;

			var text = Editor[lineIndex].Text;
			if (text.Length == 0)
				return;

			MatchCollection skip = null;
			for (var match = RegexWord.Match(text); match.Success; match = match.NextMatch())
			{
				// the target word
				var word = Actor.MatchToWord(match);

				// check cheap skip lists
				if (CommonWords.Contains(word) || Actor.IgnoreWords.Contains(word))
					continue;

				// check spelling, expensive but better before the skip pattern
				if (Spell.Spell(word))
					continue;

				// expensive skip pattern
				if (Actor.HasMatch(skip ?? (skip = Actor.GetMatches(RegexSkip, text)), match))
					continue;

				// color
				var color = new EditorColorInfo(
				My.Guid,
				match.Index,
				match.Index + match.Length,
				HighlightingForegroundColor,
				HighlightingBackgroundColor);

				// add color
				Editor.AddColor(lineIndex, color, 1);
			}
		}
	}
}
