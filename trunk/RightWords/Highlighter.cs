
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
			editor.RegisterDrawer(new EditorDrawer(GetColors, My.Guid, 1));
		}
		public bool Disabled { get; set; }
		void GetColors(IEditor editor, ICollection<EditorColor> colors, int startLine, int endLine, int startChar, int endChar)
		{
			if (Disabled)
				return;

			for (int lineIndex = startLine; lineIndex < endLine; ++lineIndex)
			{
				var text = Editor[lineIndex].Text;
				if (text.Length == 0)
					continue;

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

					// add color
					colors.Add(new EditorColor(
						lineIndex,
						match.Index,
						match.Index + match.Length,
						HighlightingForegroundColor,
						HighlightingBackgroundColor));
				}
			}
		}
	}
}
