
/*
FarNet module RightWords
Copyright (c) 2011 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NHunspell;

namespace FarNet.RightWords
{
	[System.Runtime.InteropServices.Guid("ca7ecdc0-f446-4bff-a99d-06c90fe0a3a9")]
	[ModuleTool(Name = Settings.Name, Options = ModuleToolOptions.Dialog | ModuleToolOptions.Editor | ModuleToolOptions.Panels)]
	public class TheTool : ModuleTool
	{
		internal static List<DictionaryInfo> _dictionaries;
		internal static Dictionary<string, byte> _ignore = new Dictionary<string, byte>();
		static Guid DataId = new Guid("0f1db61f-0cf8-4859-8ee6-46b567ee21ad");
		internal static IModuleManager _manager;
		public override void Invoke(object sender, ModuleToolEventArgs e)
		{
			if (e == null) return;

			_manager = Manager;
			Initialize();

			var menu = Far.Net.CreateMenu();
			menu.Title = Settings.Name;

			menu.Add("&1. Correct word").Click += delegate { DoCorrectWord(); };

			if (e.From == ModuleToolOptions.Editor)
			{
				var editor = Far.Net.Editor;
				
				menu.Add("&2. Correct text").Click += delegate { DoCorrectText(); };
				
				var itemHighlighting = menu.Add("&3. Highlighting");
				itemHighlighting.Click += delegate { DoHighlighting(); };
				if (editor.Data[DataId] != null)
					itemHighlighting.Checked = true;
			}

			menu.Add("&0. Thesaurus...").Click += delegate { DoThesaurus(); };

			menu.Show();
		}
		internal static void Initialize()
		{
			if (_dictionaries != null)
				return;

			_dictionaries = new List<DictionaryInfo>();

			var home = Path.GetDirectoryName(typeof(Hunspell).Assembly.Location);
			Hunspell.NativeDllPath = home;

			foreach (var dir in Directory.GetDirectories(home))
			{
				foreach (var aff in Directory.GetFiles(dir, "*.aff"))
				{
					var dic = Path.ChangeExtension(aff, ".dic");
					if (File.Exists(dic))
					{
						var language = new DictionaryInfo() { HunspellAffFile = aff, HunspellDictFile = dic };
						_dictionaries.Add(language);

						foreach (var dat in Directory.GetFiles(dir, "*.dat"))
							language.MyThesDatFile = dat;
					}
				}
			}
		}
		internal static Dictionary<string, byte> ReadRightWords()
		{
			var words = new Dictionary<string, byte>();
			var path = Path.Combine(_manager.GetFolderPath(SpecialFolder.RoamingData, false), Settings.UserFile);
			if (File.Exists(path))
			{
				foreach (string line in File.ReadAllLines(path))
					words[line] = 0;
			}
			return words;
		}
		void AddRightWord(Dictionary<string, byte> words, string word)
		{
			if (words.ContainsKey(word))
				return;

			var path = Path.Combine(Manager.GetFolderPath(SpecialFolder.RoamingData, true), Settings.UserFile);
			using (var writer = File.AppendText(path))
				writer.WriteLine(word);

			words.Add(word, 0);
		}
		internal static Match MatchCaret(Regex regex, string input, int caret, bool next)
		{
			Match match = regex.Match(input);
			while (match.Success)
			{
				if (caret > match.Index + match.Length)
					match = match.NextMatch();
				else if (caret < match.Index)
					return next ? match : null;
				else
					break;
			}
			return match.Success ? match : null;
		}
		static void DoCorrectWord()
		{
			ILine line = null;
			IEditor editor = null;

			var kind = Far.Net.Window.Kind;
			if (kind == WindowKind.Editor)
			{
				editor = Far.Net.Editor;
				line = editor[-1];
			}
			else
			{
				line = Far.Net.Line;
				if (line == null)
					return;
			}

			// search for the current word
			Match match = MatchCaret(new Regex(Settings.Default.WordPattern, RegexOptions.IgnorePatternWhitespace), line.Text, line.Caret, false);
			if (match == null)
				return;

			// the current word
			var word = match.Value;

			// get suggestions with check
			List<string> words = null;
			using (var spell = new MultiSpell(_dictionaries))
				if (!spell.Spell(word))
					words = spell.Suggest(word);

			// it is correct or nothing is suggested
			if (words == null || words.Count == 0)
			{
				// move caret to the end of word
				line.Caret = match.Index + match.Length;
				return;
			}

			// show suggestions
			var menu = Far.Net.CreateListMenu();
			menu.Title = match.Value;
			var cursor = Far.Net.UI.WindowCursor;
			menu.X = cursor.X;
			menu.Y = cursor.Y;
			foreach (var it in words)
				menu.Add(it);
			if (!menu.Show())
				return;

			// replace the word with the suggestion
			word = menu.Items[menu.Selected].Text;
			line.SelectText(match.Index, match.Index + match.Length);
			line.SelectedText = word;
			line.UnselectText();
			line.Caret = match.Index + word.Length;
		}
		static void DoThesaurus()
		{
			var word = Far.Net.Input("Word", Settings.Name, Settings.Name);
			if (word == null || (word = word.Trim()).Length == 0)
				return;

			var menu = Far.Net.CreateMenu();
			menu.Title = word;

			Far.Net.UI.SetProgressState(TaskbarProgressBarState.Indeterminate);
			Far.Net.UI.WindowTitle = "Searching...";
			try
			{
				using (var thesaurus = new MultiThesaurus(_dictionaries))
				{
					foreach (var result in thesaurus.Lookup(word))
					{
						foreach (var meaning in result.Meanings)
						{
							menu.Add(meaning.Description).IsSeparator = true;
							foreach (var term in meaning.Synonyms)
								menu.Add(term);
						}
					}
				}
			}
			finally
			{
				Far.Net.UI.SetProgressState(TaskbarProgressBarState.NoProgress);
			}

			if (!menu.Show())
				return;

			Far.Net.CopyToClipboard(menu.Items[menu.Selected].Text);
		}
		internal static bool HasMatch(MatchCollection matches, Match match)
		{
			if (matches != null)
				foreach (Match m in matches)
					if (match.Index >= m.Index && match.Index + match.Length <= m.Index + m.Length)
						return true;

			return false;
		}
		static void ResetHitCounters()
		{
			foreach (var dictionary in _dictionaries)
				dictionary.HitCount = 0;
		}
		internal static Regex GetRegexSkip()
		{
			var pattern = Settings.Default.SkipPattern;
			return string.IsNullOrEmpty(pattern) ? null : new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
		}
		void DoCorrectText()
		{
			// reset counters
			ResetHitCounters();

			// regular expressions
			var regexWord = new Regex(Settings.Default.WordPattern, RegexOptions.IgnorePatternWhitespace);
			Regex regexSkip = GetRegexSkip();

			// right words
			var rightWords = ReadRightWords();

			// initial editor data
			var editor = Far.Net.Editor;
			var caret0 = editor.Caret;
			int iLine1, iLine2;
			if (editor.SelectionExists)
			{
				var rect = editor.SelectionPlace;
				iLine1 = rect.First.Y;
				iLine2 = rect.Last.Y;
				if (rect.Last.X < 0)
					--iLine2;
			}
			else
			{
				iLine1 = caret0.Y;
				iLine2 = editor.Count - 1;
			}

			// use the spell checker
			var spell = new MultiSpell(_dictionaries);
			try
			{
				// loop through words and lines
				int iLine = iLine1;
				for (; ; )
				{
				NextWord:

					// the line and its text now
					var line = editor[iLine];
					var text = line.Text;

					// the first word
					Match match = MatchCaret(regexWord, text, line.Caret, true);
					if (match == null)
						goto NextLine;

					// skip matches
					MatchCollection skip = regexSkip == null ? null : regexSkip.Matches(text);

					// loop through line words (matches) with no changes
					for (; match.Success; match = match.NextMatch())
					{
						// the target word
						var word = match.Value;

						// skip if the match is in the skip area or the word is in one of the ignore lists
						if (HasMatch(skip, match) || rightWords.ContainsKey(word) || _ignore.ContainsKey(word))
							continue;

						// check spelling and get suggestions
						List<string> words = null;
						if (!spell.Spell(word))
							words = spell.Suggest(word);

						// next match on success or no suggestions
						if (words == null || words.Count == 0)
							continue;

						// new caret and selection is at the word end
						int column = match.Index + match.Length;

						// 1) select the word, !! set the caret now
						line.SelectText(match.Index, column);
						line.Caret = column;

						// 2) reframe vertically (!! horisontal is sloppy), !! keep the caret
						var frame = editor.Frame;
						frame.VisibleLine = frame.CaretLine - Far.Net.UI.WindowSize.Y / 3;
						editor.Frame = frame;

						// commit
						editor.Redraw();

						// menu
						var menu = Far.Net.CreateListMenu();
						menu.Title = match.Value;
						menu.NoInfo = true;

						// menu keys
						menu.AddKey('1');
						menu.AddKey('2');
						menu.AddKey('3');

						// menu position
						var point = editor.ConvertPointEditorToScreen(new Point(column, iLine));
						menu.X = point.X;
						menu.Y = point.Y + 1;

						// menu items
						foreach (var it in words)
							menu.Add(it);

						// menu commands
						menu.Add(string.Empty).IsSeparator = true;
						var itemIgnore = menu.Add("&1. Ignore");
						var itemIgnoreAll = menu.Add("&2. Ignore All");
						var itemAddToDictionary = menu.Add("&3. Add to Dictionary");

						// stop:
						if (!menu.Show())
							return;

						// selected item
						var item = menu.Selected < 0 ? null : menu.Items[menu.Selected];

						// ignore:
						if (menu.BreakKey == '1' || item == itemIgnore)
							continue;

						// ignore all:
						if (menu.BreakKey == '2' || item == itemIgnoreAll)
						{
							_ignore.Add(word, 0);
							continue;
						}

						// add to dictionary:
						if (menu.BreakKey == '3' || item == itemAddToDictionary)
						{
							AddRightWord(rightWords, word);
							continue;
						}

						// replace editor selection with correction
						var correction = item.Text;
						line.SelectedText = correction;
						line.UnselectText();

						// advance in the same line
						int caret = match.Index + correction.Length + 1;
						if (caret < line.Text.Length)
						{
							line.Caret = caret;
							goto NextWord;
						}

						// next line
						break;
					}

				NextLine:

					++iLine;
					if (iLine > iLine2)
						break;

					editor.GoTo(0, iLine);
				}
			}
			finally
			{
				spell.Dispose();
				editor.UnselectText();
			}
		}
		void DoHighlighting()
		{
			var editor = Far.Net.Editor;

			var highlighter = (Highlighter)editor.Data[DataId];
			if (highlighter == null)
			{
				editor.Data[DataId] = new Highlighter(editor);
			}
			else
			{
				highlighter.Stop();
				editor.Data.Remove(DataId);
			}
		}
	}
}
