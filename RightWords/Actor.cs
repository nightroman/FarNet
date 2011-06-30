
/*
FarNet module RightWords
Copyright (c) 2011 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using NHunspell;
namespace FarNet.RightWords
{
	static class Actor
	{
		static readonly IModuleManager Manager;
		public static readonly List<DictionaryInfo> Dictionaries = new List<DictionaryInfo>();
		public static readonly Dictionary<string, byte> IgnoreWords = new Dictionary<string, byte>();
		static readonly WeakReference _rightWords = new WeakReference(null);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static Actor()
		{
			// expose the manager
			Manager = Far.Net.GetModuleManager(typeof(Actor));

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
		}
		public static Dictionary<string, byte> ReadRightWords()
		{
			var words = (Dictionary<string, byte>)_rightWords.Target;
			if (words != null)
				return words;

			words = new Dictionary<string, byte>();
			var path = Path.Combine(Far.Net.GetModuleManager(typeof(TheTool)).GetFolderPath(SpecialFolder.RoamingData, false), Settings.UserFile);
			if (File.Exists(path))
			{
				foreach (string line in File.ReadAllLines(path))
					words[line] = 0;
			}

			_rightWords.Target = words;
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
			var match = MatchCaret(new Regex(Settings.Default.WordPattern, RegexOptions.IgnorePatternWhitespace), line.Text, line.Caret, false);
			if (match == null)
				return;

			// the current word
			var word = match.Value;

			// get suggestions with check
			List<string> words = null;
			var spell = MultiSpell.GetWeakInstance(Dictionaries);
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
			var cursor = Far.Net.UI.WindowCursor;
			var menu = new UIWordMenu(words, word, cursor.X, cursor.Y + 1);

			// cancel or ignore:
			if (!menu.Show() || menu.IsIgnore)
				return;

			// ignore all:
			if (menu.IsIgnoreAll)
			{
				IgnoreWords.Add(word, 0);
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
			var line = Far.Net.Line;
			if (line != null)
			{
				var match = MatchCaret(new Regex(Settings.Default.WordPattern, RegexOptions.IgnorePatternWhitespace), line.Text, line.Caret, false);
				if (match != null)
					word = match.Value;
			}

			word = Far.Net.Input("Word", Settings.Name, Settings.Name, word);
			if (word == null || (word = word.Trim()).Length == 0)
				return;

			var menu = Far.Net.CreateMenu();
			menu.Title = word;

			Far.Net.UI.SetProgressState(TaskbarProgressBarState.Indeterminate);
			Far.Net.UI.WindowTitle = "Searching...";
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
				Far.Net.UI.SetProgressState(TaskbarProgressBarState.NoProgress);
			}

			if (!menu.Show())
				return;

			Far.Net.CopyToClipboard(menu.Items[menu.Selected].Text);
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
		static void ResetHitCounters()
		{
			foreach (var dictionary in Dictionaries)
				dictionary.HitCount = 0;
		}
		public static Regex GetRegexSkip()
		{
			var pattern = Settings.Default.SkipPattern;
			return string.IsNullOrEmpty(pattern) ? null : new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
		}
		public static void CorrectText()
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
			var spell = MultiSpell.GetWeakInstance(Dictionaries);
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
						var word = match.Value;

						// check cheap skip lists
						if (rightWords.ContainsKey(word) || IgnoreWords.ContainsKey(word))
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
						frame.VisibleLine = frame.CaretLine - Far.Net.UI.WindowSize.Y / 3;
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
							IgnoreWords.Add(word, 0);
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
		public static void Highlight(IEditor editor)
		{
			var highlighter = (Highlighter)editor.Data[Settings.EditorDataId];
			if (highlighter == null)
			{
				editor.Data[Settings.EditorDataId] = new Highlighter(editor);
			}
			else
			{
				highlighter.Stop();
				editor.Data.Remove(Settings.EditorDataId);
			}
		}
		public static string GetUserDictionaryPath(string name, bool create)
		{
			return Path.Combine(Manager.GetFolderPath(SpecialFolder.RoamingData, create), "RightWords." + name + ".dic");
		}
		const string TitleAddWord = "Add to Dictionary";
		static string[] ShowMenuAddWord(string word)
		{
			string word2;

			if (Regex.IsMatch(word, @"^\p{Ll}{2,}$"))
				word2 = word.Substring(0, 1).ToUpper(CultureInfo.CurrentCulture) + word.Substring(1);
			else if (Regex.IsMatch(word, @"^\p{Lu}\p{Ll}+$"))
				word2 = word.ToLower(CultureInfo.CurrentCulture);
			else
				return new string[] { word };

			var menu = Far.Net.CreateMenu();
			menu.Title = TitleAddWord;
			menu.Add(word);
			menu.Add(word + ", " + word2);
			if (!menu.Show())
				return null;

			return menu.Selected == 0 ? new string[] { word } : new string[] { word, word2 };
		}
		static void AddRightWord(Dictionary<string, byte> words, string word)
		{
			var names = new List<string>();
			foreach (var dic in Dictionaries)
				names.Add(dic.Language);
			names.Sort();

			var menu = Far.Net.CreateMenu();
			menu.Title = TitleAddWord;
			menu.AutoAssignHotkeys = true;
			menu.Add("Common");
			foreach (string name in names)
				menu.Add(name);

			MultiSpell multiSpell = null;

			for (; ; )
			{
				if (!menu.Show())
					return;

				// common:
				if (menu.Selected == 0)
				{
					string[] newWords = ShowMenuAddWord(word);
					if (newWords == null)
						return;

					if (words == null)
						words = ReadRightWords();

					// write/add
					var path = Path.Combine(Manager.GetFolderPath(SpecialFolder.RoamingData, true), Settings.UserFile);
					using (var writer = File.AppendText(path))
					{
						foreach(var newWord in newWords)
						{
							if (words.ContainsKey(newWord))
								continue;

							writer.WriteLine(newWord);
							words.Add(newWord, 0);
						}
					}

					return;
				}

				// language:
				var language = menu.Items[menu.Selected].Text;
				var spell = (multiSpell ?? (multiSpell = MultiSpell.GetWeakInstance(Dictionaries))).GetSpell(language);

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

						var menu2 = Far.Net.CreateMenu();
						menu2.Title = "Example Stem";
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
	}
}
