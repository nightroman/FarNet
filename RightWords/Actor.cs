
/*
FarNet module RightWords
Copyright (c) 2011-2014 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using NHunspell;
namespace FarNet.RightWords
{
	static class Actor
	{
		public static readonly List<DictionaryInfo> Dictionaries = new List<DictionaryInfo>();
		public static readonly HashSet<string> IgnoreWords = new HashSet<string>();
		static readonly IModuleManager Manager = Far.Api.GetModuleManager(Settings.ModuleName);
		static readonly WeakReference CommonWords = new WeakReference(null);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		static readonly bool Initialized = Initialize();
		static bool Initialize()
		{
			// home directory, for libraries and dictionaries
			var home = Path.GetDirectoryName(typeof(Hunspell).Assembly.Location);

			//! catch `Native Library is already loaded`, e.g. loaded by another module
			try { Hunspell.NativeDllPath = home; }
			catch (InvalidOperationException ex) { Log.TraceException(ex); }

			// initialize dictionaries
			foreach (var dir in Directory.GetDirectories(home))
			{
				foreach (var aff in Directory.GetFiles(dir, "*.aff"))
				{
					var dic = Path.ChangeExtension(aff, ".dic");
					if (File.Exists(dic))
					{
						var language = new DictionaryInfo() { HunspellAffFile = aff, HunspellDictFile = dic, Language = Path.GetFileName(dir) };
						Dictionaries.Add(language);

						foreach (var dat in Directory.GetFiles(dir, "*.dat"))
							language.MyThesDatFile = dat;
					}
				}
			}

			return true;
		}
		public static HashSet<string> GetCommonWords()
		{
			var words = (HashSet<string>)CommonWords.Target;
			if (words != null)
				return words;

			words = new HashSet<string>();
			var path = Path.Combine(GetUserDictionaryDirectory(false), Settings.UserFile);
			if (File.Exists(path))
			{
				foreach (string line in File.ReadAllLines(path))
					words.Add(line);
			}

			CommonWords.Target = words;
			return words;
		}
		public static Match MatchCaret(Regex regex, string input, int caret, bool next)
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
		public static void CorrectWord()
		{
			ILine line = null;
			IEditor editor = null;

			var kind = Far.Api.Window.Kind;
			if (kind == WindowKind.Editor)
			{
				editor = Far.Api.Editor;
				line = editor[-1];
			}
			else
			{
				line = Far.Api.Line;
				if (line == null)
					return;
			}

			// search for the current word
			var match = MatchCaret(new Regex(Settings.Default.WordPattern, RegexOptions.IgnorePatternWhitespace), line.Text, line.Caret, false);
			if (match == null)
				return;

			// the current word
			var word = MatchToWord(match);

			// get suggestions with check
			List<string> words = null;
			var spell = MultiSpell.Get();
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
			var cursor = Far.Api.UI.WindowCursor;
			var menu = new UIWordMenu(words, word, cursor.X, cursor.Y + 1);

			// cancel or ignore:
			if (!menu.Show() || menu.IsIgnore)
				return;

			// ignore all:
			if (menu.IsIgnoreAll)
			{
				IgnoreWords.Add(word);
				return;
			}

			// add to dictionary:
			if (menu.IsAddToDictionary)
			{
				AddRightWord(null, word);
				return;
			}

			// replace the word with the suggestion
			word = menu.Word;
			line.SelectText(match.Index, match.Index + match.Length);
			line.SelectedText = word;
			line.UnselectText();
			line.Caret = match.Index + word.Length;
		}
		public static void ShowThesaurus()
		{
			string word = string.Empty;
			var line = Far.Api.Line;
			if (line != null)
			{
				var match = MatchCaret(new Regex(Settings.Default.WordPattern, RegexOptions.IgnorePatternWhitespace), line.Text, line.Caret, false);
				if (match != null)
					word = match.Value;
			}

			word = Far.Api.Input(My.Word, Settings.ModuleName, My.Thesaurus, word);
			if (word == null || (word = word.Trim()).Length == 0)
				return;

			var menu = Far.Api.CreateMenu();
			menu.Title = word;

			Far.Api.UI.SetProgressState(TaskbarProgressBarState.Indeterminate);
			Far.Api.UI.WindowTitle = My.Searching;
			try
			{
				using (var thesaurus = new MultiThesaurus(Dictionaries))
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
				Far.Api.UI.SetProgressState(TaskbarProgressBarState.NoProgress);
			}

			if (!menu.Show())
				return;

			Far.Api.CopyToClipboard(menu.Items[menu.Selected].Text);
		}
		public static bool HasMatch(MatchCollection matches, Match match)
		{
			if (matches != null)
				foreach (Match m in matches)
					if (match.Index >= m.Index && match.Index + match.Length <= m.Index + m.Length)
						return true;

			return false;
		}
		public static MatchCollection GetMatches(Regex regex, string text)
		{
			return regex == null ? null : regex.Matches(text);
		}
		public static Regex GetRegexSkip()
		{
			var pattern = Settings.Default.SkipPattern;
			return string.IsNullOrEmpty(pattern) ? null : new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
		}
		public static void CorrectText()
		{
			// regular expressions
			var regexWord = new Regex(Settings.Default.WordPattern, RegexOptions.IgnorePatternWhitespace);
			Regex regexSkip = GetRegexSkip();

			// right words
			var rightWords = GetCommonWords();

			// initial editor data
			var editor = Far.Api.Editor;
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
			var spell = MultiSpell.Get();
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

					// loop through line words (matches) with no changes
					MatchCollection skip = null;
					for (; match.Success; match = match.NextMatch())
					{
						// the target word
						var word = MatchToWord(match);

						// check cheap skip lists
						if (rightWords.Contains(word) || IgnoreWords.Contains(word))
							continue;

						// check spelling, expensive but better before the skip pattern
						if (spell.Spell(word))
							continue;

						// expensive skip pattern
						if (Actor.HasMatch(skip ?? (skip = Actor.GetMatches(regexSkip, text)), match))
							continue;

						// check spelling and get suggestions
						List<string> words = spell.Suggest(word);

						// next match on success or no suggestions
						if (words.Count == 0)
							continue;

						// new caret and selection is at the word end
						int column = match.Index + match.Length;

						// 1) select the word, !! set the caret now
						line.SelectText(match.Index, column);
						line.Caret = column;

						// 2) reframe vertically (!! horisontal is sloppy), !! keep the caret
						var frame = editor.Frame;
						frame.VisibleLine = frame.CaretLine - Far.Api.UI.WindowSize.Y / 3;
						editor.Frame = frame;

						// commit
						editor.Redraw();

						// menu
						var point = editor.ConvertPointEditorToScreen(new Point(column, iLine));
						var menu = new UIWordMenu(words, word, point.X, point.Y + 1);

						// cancel:
						if (!menu.Show())
							return;

						// ignore:
						if (menu.IsIgnore)
							continue;

						// ignore all:
						if (menu.IsIgnoreAll)
						{
							IgnoreWords.Add(word);
							continue;
						}

						// add to dictionary:
						if (menu.IsAddToDictionary)
						{
							AddRightWord(rightWords, word);
							continue;
						}

						// replace editor selection with correction
						var correction = menu.Word;
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
				editor.UnselectText();
			}
		}
		static string GetUserDictionaryDirectory(bool create)
		{
			var path = Settings.Default.UserDictionaryDirectory;
			if (string.IsNullOrEmpty(path))
				return Manager.GetFolderPath(SpecialFolder.RoamingData, create);

			path = Environment.ExpandEnvironmentVariables(path);

			if (create && !Directory.Exists(path))
				Directory.CreateDirectory(path);

			return path;
		}
		public static string GetUserDictionaryPath(string name, bool create)
		{
			return Path.Combine(GetUserDictionaryDirectory(create), "RightWords." + name + ".dic");
		}
		static string[] ShowMenuAddWord(string word)
		{
			string word2;

			if (Regex.IsMatch(word, @"^\p{Ll}{2,}$"))
				word2 = word.Substring(0, 1).ToUpper(CultureInfo.CurrentCulture) + word.Substring(1);
			else if (Regex.IsMatch(word, @"^\p{Lu}\p{Ll}+$"))
				word2 = word.ToLower(CultureInfo.CurrentCulture);
			else
				return new string[] { word };

			var menu = Far.Api.CreateMenu();
			menu.Title = My.AddToDictionary;
			menu.Add(word);
			menu.Add(word + ", " + word2);
			if (!menu.Show())
				return null;

			return menu.Selected == 0 ? new string[] { word } : new string[] { word, word2 };
		}
		static void AddRightWord(HashSet<string> words, string word)
		{
			// language names, unique, sorted
			var languages = new List<string>();
			foreach (var dic in Dictionaries)
				if (!languages.Contains(dic.Language))
					languages.Add(dic.Language);
			languages.Sort();

			// dictionary menu
			var menu = Far.Api.CreateMenu();
			menu.Title = My.AddToDictionary;
			menu.AutoAssignHotkeys = true;
			menu.Add(My.Common);
			foreach (string name in languages)
				menu.Add(name);

			// repeat the menu
			MultiSpell multiSpell = null;
			while (menu.Show())
			{
				// common:
				if (menu.Selected == 0)
				{
					string[] newWords = ShowMenuAddWord(word);
					if (newWords == null)
						continue;

					if (words == null)
						words = GetCommonWords();

					// write/add
					var path = Path.Combine(GetUserDictionaryDirectory(true), Settings.UserFile);
					using (var writer = File.AppendText(path))
					{
						foreach (var newWord in newWords)
						{
							if (words.Contains(newWord))
								continue;

							writer.WriteLine(newWord);
							words.Add(newWord);
						}
					}

					return;
				}

				// language:
				var language = menu.Items[menu.Selected].Text;
				var spell = (multiSpell ?? (multiSpell = MultiSpell.Get())).GetSpell(language);

				// dialog
				var dialog = new UIWordDialog(word, string.Empty);
				while (dialog.Show())
				{
					var stem1 = dialog.Stem1.Trim();
					if (stem1.Length == 0)
						continue;

					var stem2 = dialog.Stem2.Trim();

					bool ok = (stem2.Length == 0) ? spell.Add(stem1) : spell.AddWithAffix(stem1, stem2);
					if (!ok)
					{
						var stems = spell.Stem(stem2);
						if (stems.Count == 0 || stems.Count == 1 && stems[0] == stem2)
							continue;

						var menu2 = Far.Api.CreateMenu();
						menu2.Title = My.ExampleStem;
						foreach (var it in stems)
							menu2.Add(it);

						if (menu2.Show())
							dialog.Stem2 = menu2.Items[menu2.Selected].Text;

						continue;
					}

					var path = Actor.GetUserDictionaryPath(language, true);
					using (var writer = File.AppendText(path))
					{
						if (stem2.Length == 0)
							writer.WriteLine(stem1);
						else
							writer.WriteLine(stem1 + " " + stem2);
					}

					return;
				}
			}
		}
		public static string MatchToWord(Match match)
		{
			if (match.Groups.Count < 2)
				return match.Value;

			var word = match.Value;

			for (int i = match.Groups.Count; --i >= 1; )
			{
				var group = match.Groups[i];
				if (!group.Success)
					continue;

				var index = group.Index - match.Index;
				if (index + group.Length <= word.Length)
					word = word.Remove(index, group.Length);
			}

			return word;
		}
	}
}
