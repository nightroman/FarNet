using FarNet;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DataStamp = (int ChangeCount, int LineCount, int TopLineIndex);
using LineData = (string? Text, (int, int)[]? Spans);

namespace RightWords;

[ModuleDrawer(Name = "Spelling mistakes", Mask = "*.hlf;*.htm;*.html;*.lng;*.restext", Priority = 1, Id = "bbed2ef1-97d1-4ba2-ac56-9de56bc8030c")]
public class Highlighter : ModuleDrawer
{
	readonly MultiSpell _spell = MultiSpell.Get();
	int _knownWordsVersion = KnownWords.Version;

	/// <summary>
	/// Last checked line data cache.
	/// </summary>
	LineData[] _lastData = [];
	DataStamp _lastStamp;

	/// <summary>
	/// Finds the cached data by the line hint index and text.
	/// </summary>
	bool FindLineData(ReadOnlySpan<char> text, int index, out LineData data)
	{
		// if the frame is not changed (arrows without scrolling or typing in the same line)
		// then many not changed lines are in the same positions, check the hint index first
		if (index < _lastData.Length)
		{
			if (text.SequenceEqual(_lastData[index].Text))
			{
				data = _lastData[index];
				return true;
			}
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

			if (ok1 && text.SequenceEqual(_lastData[j].Text))
			{
				data = _lastData[j];
				return true;
			}

			if (ok2 && text.SequenceEqual(_lastData[k].Text))
			{
				data = _lastData[k];
				return true;
			}
		}

		data = default;
		return false;
	}

	public override void Invoke(IEditor editor, ModuleDrawerEventArgs e)
	{
#if DEBUG
		// New strings should be 0 on moving caret without scrolling and 0-1 on line scrolling.
		var sw = Stopwatch.StartNew();
		int newStrings = 0;
#endif

		var settings = Settings.Default.GetData();

		// if known words changed then drop the cache
		if (_knownWordsVersion != KnownWords.Version)
		{
			_lastData = [];
			_knownWordsVersion = KnownWords.Version;
		}

		// new stats
		int N = e.Lines.Count;
		int topLineIndex = e.Lines[0].Index;
		var newStamp = new DataStamp { ChangeCount = editor.ChangeCount, LineCount = N, TopLineIndex = topLineIndex };

		// if nothing changed then use spans from cache
		if (_lastStamp == newStamp && _lastData.Length == N)
		{
			for (int iData = 0; iData < N; ++iData)
			{
				var spans = _lastData[iData].Spans;
				if (spans is null)
					continue;

				int lineIndex = topLineIndex + iData;
				foreach (var item in spans)
					e.Colors.Add(new EditorColor(
						lineIndex,
						item.Item1,
						item.Item2,
						settings.HighlightingForegroundColor,
						settings.HighlightingBackgroundColor));
			}

#if DEBUG
			Debug.WriteLine($"##rw {sw.Elapsed} cache");
#endif

			return;
		}

		// new lines cache
		var newData = new LineData[N];

		// to use for all lines
		var lineSpans = new List<(int, int)>();

		// process input lines, find some cached and parse new
		var prefixes = settings.Prefixes;
		for (int iInputLine = 0; iInputLine < N; ++iInputLine)
		{
			var line = e.Lines[iInputLine];
			var text = line.Text2.TrimEnd();

			// case: ignore empty
			if (text.Length == 0)
				continue;

			// case: too long line, color the whole line
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

			// case: cached line, keep it again and use its spans
			if (FindLineData(text, line.Index - topLineIndex, out var data))
			{
				newData[iInputLine] = data;
				if (data.Spans is { })
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

#if DEBUG
			++newStrings;
#endif

			// parse and check words, collect color spans
			lineSpans.Clear();
			MatchCollection? skip = null;
			var textString = text.ToString();
			foreach (var match in settings.WordRegex2.EnumerateMatches(text))
			{
				// the target word
				var word = Kit.CleanWord(text.Slice(match.Index, match.Length), settings.RemoveRegex2);

				// check cheap skip lists
				if (KnownWords.Contains(word))
					continue;

				// check spelling, expensive but better before the skip pattern
				if (_spell.Check(word))
					continue;

				// try prefixes
				string? prefix = null;
				for (int i = prefixes.Length; --i >= 0;)
				{
					if (word.StartsWith(prefixes[i], StringComparison.OrdinalIgnoreCase))
					{
						prefix = prefixes[i];
						break;
					}
				}
				if (prefix is { } && word.Length - prefix.Length > 2)
				{
					var word2 = word[prefix.Length..];
					if (_spell.Check(word2))
						continue;
				}

				// expensive skip pattern
				if (Kit.HasMatch(skip ??= Kit.GetMatches(settings.SkipRegex2, textString), match.Index, match.Length))
					continue;

				// add the span
				lineSpans.Add((match.Index, match.Index + match.Length));
			}

			// cache data, add colors
			if (lineSpans.Count == 0)
			{
				newData[iInputLine] = new LineData { Text = textString };
			}
			else
			{
				newData[iInputLine] = new LineData { Text = textString, Spans = [.. lineSpans] };
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
		_lastStamp = newStamp;

#if DEBUG
		Debug.WriteLine($"##rw {sw.Elapsed} {newStrings}");
#endif
	}
}
