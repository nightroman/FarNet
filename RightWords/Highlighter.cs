
// FarNet module RightWords
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FarNet.RightWords;

[ModuleDrawer(Name = "Spelling mistakes", Mask = "*.hlf;*.htm;*.html;*.lng;*.restext", Priority = 1, Id = "bbed2ef1-97d1-4ba2-ac56-9de56bc8030c")]
public class Highlighter : ModuleDrawer
{
	readonly MultiSpell _spell = MultiSpell.Get();
	int _knownWordsVersion;

	/// <summary>
	/// Last checked line data cache.
	/// </summary>
	LineData[] _lastData = [];

	public Highlighter()
	{
		_knownWordsVersion = KnownWords.Version;
	}

	/// <summary>
	/// Line text and color spans.
	/// </summary>
	class LineData
	{
		public string Text;
		public ValueTuple<int, int>[] Spans;
	}

	/// <summary>
	/// Finds the cached data by the line hint index and text.
	/// </summary>
	LineData FindLineData(int index, string text)
	{
		// if the frame is not changed (arrows without scrolling or typing in the same line)
		// then many not changed lines are in the same positions, check the hint index first
		if (index < _lastData.Length)
		{
			if (_lastData[index]?.Text == text)
				return _lastData[index];
		}

		// on scrolling or typing many lines shift positions, find from the hint index
		for (int i = 1; ; ++i)
		{
			int j = index - i;
			int k = index + i;
			bool ok1 = j >= 0 && j < _lastData.Length;
			bool ok2 = k >= 0 && k < _lastData.Length;
			if (!ok1 && !ok2)
				break;
			if (ok1 && _lastData[j]?.Text == text)
				return _lastData[j];
			if (ok2 && _lastData[k]?.Text == text)
				return _lastData[k];
		}

		return null;
	}

	public override void Invoke(IEditor editor, ModuleDrawerEventArgs e)
	{
		var settings = Settings.Default.GetData();

		// if known words changed then drop the cache
		if (_knownWordsVersion != KnownWords.Version)
		{
			_lastData = [];
			_knownWordsVersion = KnownWords.Version;
		}

		int topLineIndex = e.Lines[0].Index;
		var newData = new LineData[e.Lines.Count];

		int newDataIndex = -1;
		var lineSpans = new List<ValueTuple<int, int>>();
		foreach (var line in e.Lines)
		{
			++newDataIndex;
			var text = line.Text;
			if (text.Length == 0)
				continue;

			// rare case: too long line, color the whole line
			if (settings.MaximumLineLength > 0 && text.Length > settings.MaximumLineLength)
			{
				e.Colors.Add(new EditorColor(
					line.Index,
					0,
					text.Length,
					settings.HighlightingForegroundColor,
					settings.HighlightingBackgroundColor));
				continue;
			}

			// try get the cached line data
			var data = FindLineData(line.Index - topLineIndex, text);
			if (data != null)
			{
				// keep it as new and add colors
				newData[newDataIndex] = data;
				if (data.Spans != null)
				{
					foreach (var item in data.Spans)
						e.Colors.Add(new EditorColor(
							line.Index,
							item.Item1,
							item.Item2,
							settings.HighlightingForegroundColor,
							settings.HighlightingBackgroundColor));
				}
				continue;
			}

			// parse and check words, collect color spans
			lineSpans.Clear();
			MatchCollection skip = null;
			for (var match = settings.WordRegex2.Match(text); match.Success; match = match.NextMatch())
			{
				// the target word
				var word = Actor.MatchToWord(match);

				// check cheap skip lists
				if (KnownWords.Contains(word))
					continue;

				// check spelling, expensive but better before the skip pattern
				if (_spell.Check(word))
					continue;

				// expensive skip pattern
				if (Actor.HasMatch(skip ??= Actor.GetMatches(settings.SkipRegex2, text), match))
					continue;

				// add the span
				lineSpans.Add((match.Index, match.Index + match.Length));
			}

			// cache the data and add colors if any
			if (lineSpans.Count == 0)
			{
				newData[newDataIndex] = new LineData { Text = text };
			}
			else
			{
				newData[newDataIndex] = new LineData { Text = text, Spans = [.. lineSpans] };

				foreach (var span in lineSpans)
					e.Colors.Add(new EditorColor(
						line.Index,
						span.Item1,
						span.Item2,
						settings.HighlightingForegroundColor,
						settings.HighlightingBackgroundColor));
			}
		}

		// update cache
		_lastData = newData;
	}
}
