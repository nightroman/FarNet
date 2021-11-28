
// FarNet module RightWords
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FarNet.RightWords
{
	[ModuleDrawer(Name = "Spelling mistakes", Mask = "*.hlf;*.htm;*.html;*.lng;*.restext", Priority = 1)]
	[System.Runtime.InteropServices.Guid("bbed2ef1-97d1-4ba2-ac56-9de56bc8030c")]
	public class Highlighter : ModuleDrawer
	{
		readonly MultiSpell Spell = MultiSpell.Get();
		readonly HashSet<string> CommonWords = Actor.GetCommonWords();
		public override void Invoke(IEditor editor, ModuleDrawerEventArgs e)
		{
			var sets = Settings.Default.GetData();

			foreach (var line in e.Lines)
			{
				var text = line.Text;
				if (text.Length == 0)
					continue;

				if (sets.MaximumLineLength > 0 && text.Length > sets.MaximumLineLength)
				{
					e.Colors.Add(new EditorColor(
						line.Index,
						0,
						text.Length,
						sets.HighlightingForegroundColor,
						sets.HighlightingBackgroundColor));
					continue;
				}

				MatchCollection skip = null;
				for (var match = sets.WordRegex2.Match(text); match.Success; match = match.NextMatch())
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
					if (Actor.HasMatch(skip ?? (skip = Actor.GetMatches(sets.SkipRegex2, text)), match))
						continue;

					// add color
					e.Colors.Add(new EditorColor(
						line.Index,
						match.Index,
						match.Index + match.Length,
						sets.HighlightingForegroundColor,
						sets.HighlightingBackgroundColor));
				}
			}
		}
	}
}
