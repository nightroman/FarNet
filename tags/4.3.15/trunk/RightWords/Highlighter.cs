
/*
FarNet module RightWords
Copyright (c) 2011 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
namespace FarNet.RightWords
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	class Highlighter
	{
		readonly MultiSpell Spell = MultiSpell.GetWeakInstance(Actor.Dictionaries);
		readonly Regex RegexSkip = Actor.GetRegexSkip();
		readonly Regex RegexWord = new Regex(Settings.Default.WordPattern, RegexOptions.IgnorePatternWhitespace);
		readonly Dictionary<string, byte> RightWords = Actor.ReadRightWords();
		readonly ConsoleColor HighlightingBackgroundColor = Settings.Default.HighlightingBackgroundColor;
		readonly ConsoleColor HighlightingForegroundColor = Settings.Default.HighlightingForegroundColor;
		readonly IEditor Editor;
		bool? IsRemoveColors;
		public Highlighter(IEditor editor)
		{
			Editor = editor;

			editor.Redrawing += Redrawing;
			editor.Closed += Closed;
		}
		public void Stop()
		{
			Editor.Redrawing -= Redrawing;
			Editor.Closed -= Closed;
		}
		void Closed(object sender, EventArgs e)
		{
			Stop();
		}
		void Redrawing(object sender, EditorRedrawingEventArgs e)
		{
			// do not draw `Line`, `Screen` is called anyway
			if (e.Mode == EditorRedrawMode.Line)
				return;

			// do not draw `Change` if we do not remove colors, `Screen` is called anyway
			if (e.Mode == EditorRedrawMode.Change && IsRemoveColors.HasValue && !IsRemoveColors.Value)
				return;

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
			var text = Editor[lineIndex].Text;
			if (text.Length == 0)
				return;

			// check colors
			if (!IsRemoveColors.HasValue)
				IsRemoveColors = Editor.GetColors(lineIndex).Count == 0;

			// remove colors
			if (IsRemoveColors.Value)
				Editor.AddColor(lineIndex, new ColorSpan() { Start = -1 });

			MatchCollection skip = null;
			for (var match = RegexWord.Match(text); match.Success; match = match.NextMatch())
			{
				// the target word
				var word = match.Value;

				// check cheap skip lists
				if (RightWords.ContainsKey(word) || Actor.IgnoreWords.ContainsKey(word))
					continue;

				// check spelling, expensive but better before the skip pattern
				if (Spell.Spell(word))
					continue;

				// expensive skip pattern
				if (Actor.HasMatch(skip ?? (skip = Actor.GetMatches(RegexSkip, text)), match))
					continue;

				// color
				var color = new ColorSpan();
				color.Start = match.Index;
				color.End = match.Index + match.Length;
				color.Background = HighlightingBackgroundColor;
				color.Foreground = HighlightingForegroundColor;

				// add color
				Editor.AddColor(lineIndex, color);
			}
		}
	}
}
