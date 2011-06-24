
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
	public class Highlighter
	{
		IEditor _editor;
		MultiSpell _spell;
		Regex _regexSkip;
		Regex _regexWord;
		Dictionary<string, byte> _rightWords;
		public Highlighter(IEditor editor)
		{
			TheTool.Initialize();
			_editor = editor;

			_spell = new MultiSpell(TheTool._dictionaries);
			_regexSkip = TheTool.GetRegexSkip();
			_regexWord = new Regex(Settings.Default.WordPattern, RegexOptions.IgnorePatternWhitespace);
			_rightWords = TheTool.ReadRightWords();

			editor.Redrawing += OnRedrawing;
			editor.Closed += OnClosed;

			editor.Redraw();
		}
		public void Stop()
		{
			_editor.Redrawing -= OnRedrawing;
			_editor.Closed -= OnClosed;
			_spell.Dispose();
		}
		void OnClosed(object sender, EventArgs e)
		{
			Stop();
		}
		void OnRedrawing(object sender, EditorRedrawingEventArgs e)
		{
			if (e.Mode == EditorRedrawMode.Line)
			{
				HighlightLine(-1);
				return;
			}

			int height = Far.Net.UI.WindowSize.Y;
			TextFrame frame = _editor.Frame;
			int lineCount = _editor.Count;
			
			for (int i = 0; i < height; ++i)
			{
				int index = frame.VisibleLine + i;
				if (index >= lineCount)
					break;

				HighlightLine(index);
			}
		}
		void HighlightLine(int index)
		{
			var line = _editor[index];
			var text = line.Text;
			int caret = line.Caret;

			Match match = TheTool.MatchCaret(_regexWord, text, 0, true);
			if (match == null)
				return;

			MatchCollection skip = _regexSkip == null ? null : _regexSkip.Matches(text);

			for (; match.Success; match = match.NextMatch())
			{
				var word = match.Value;

				if (TheTool.HasMatch(skip, match) || _rightWords.ContainsKey(word) || TheTool._ignore.ContainsKey(word)) //???
					continue;

				if (_spell.Spell(word))
					continue;

				var color = new LineColor();
				color.Start = match.Index;
				color.End = match.Index + match.Length;
				color.Foreground = ConsoleColor.Black;
				color.Background = ConsoleColor.Red;

				_editor.SetColor(index, color);
			}
		}
	}
}
