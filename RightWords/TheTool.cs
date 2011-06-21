
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
	[ModuleTool(Name = TheTool.Name, Options = ModuleToolOptions.Dialog | ModuleToolOptions.Editor | ModuleToolOptions.Panels)]
	public class TheTool : ModuleTool
	{
		const string Name = "RightWords";
		static List<LanguageConfig> _dictionaries;
		static List<string> _ignoreAll = new List<string>();
		public override void Invoke(object sender, ModuleToolEventArgs e)
		{
			if (e == null) return;

			Initialize();

			var menu = Far.Net.CreateMenu();
			menu.Title = Name;

			menu.Add("&1. Correct word").Click += delegate { DoCorrectWord(); };

			if (e.From == ModuleToolOptions.Editor)
				menu.Add("&2. Correct text").Click += delegate { DoCorrectText(); };

			menu.Add("&0. Thesaurus...").Click += delegate { DoThesaurus(); };

			menu.Show();
		}
		static void Initialize()
		{
			if (_dictionaries != null)
				return;

			_dictionaries = new List<LanguageConfig>();

			var home = Path.GetDirectoryName(typeof(Hunspell).Assembly.Location);
			Hunspell.NativeDllPath = home;

			foreach (var dir in Directory.GetDirectories(home))
			{
				foreach (var aff in Directory.GetFiles(dir, "*.aff"))
				{
					var dic = Path.ChangeExtension(aff, ".dic");
					if (File.Exists(dic))
					{
						var language = new LanguageConfig() { HunspellAffFile = aff, HunspellDictFile = dic };
						_dictionaries.Add(language);

						foreach (var dat in Directory.GetFiles(dir, "*.dat"))
							language.MyThesDatFile = dat;
					}
				}
			}
		}
		static Match MatchCaret(Regex regex, string input, int caret, bool next)
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
			var word = Far.Net.Input("Word", Name, Name);
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
		static bool HasMatch(MatchCollection matches, Match match)
		{
			foreach (Match m in matches)
				if (match.Index >= m.Index && match.Index + match.Length <= m.Index + m.Length)
					return true;

			return false;
		}
		static void DoCorrectText()
		{
			// the editor window is expected
			if (Far.Net.Window.Kind != WindowKind.Editor)
				return;

			// regex
			var regexWord = new Regex(Settings.Default.WordPattern, RegexOptions.IgnorePatternWhitespace);
			Regex regexSkip = null;
			var patternSkip = Settings.Default.SkipPattern;
			if (!string.IsNullOrEmpty(patternSkip))
				regexSkip = new Regex(patternSkip, RegexOptions.IgnorePatternWhitespace);

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
					
					var line = editor[iLine];
					var text = line.Text;

					MatchCollection skip = null;
					bool toGetSkip = true;

					Match match = MatchCaret(regexWord, text, line.Caret, true);
					if (match == null)
						goto NextLine;

					for (; ; )
					{
						if (toGetSkip)
						{
							toGetSkip = false;
							if (regexSkip != null)
								skip = regexSkip.Matches(text);
						}

						// skip pattern or ignore list
						if (skip != null && HasMatch(skip, match) || _ignoreAll.Contains(match.Value))
						{
							match = match.NextMatch();
							if (match.Success)
								continue;

							goto NextLine;
						}
						
						var word = match.Value;

						List<string> words = null;
						if (!spell.Spell(word))
							words = spell.Suggest(word);

						if (words == null || words.Count == 0)
						{
							match = match.NextMatch();
							if (match.Success)
								continue;

							goto NextLine;
						}

						// new caret is at the end of word
						int column = match.Index + match.Length;

						// 1) select the target word, !! set the caret now
						line.SelectText(match.Index, column);
						line.Caret = column;

						// 2) reframe vertically, !! keep the caret; !! horisontal is sloppy
						var frame = editor.Frame;
						frame.VisibleLine = frame.CaretLine - Far.Net.UI.WindowSize.Y / 3;
						editor.Frame = frame;

						// commit
						editor.Redraw();

						// make the menu
						var menu = Far.Net.CreateListMenu();
						menu.Title = match.Value;
						var point = editor.ConvertPointEditorToScreen(new Point(column, iLine));
						menu.X = point.X;
						menu.Y = point.Y + 1;

						// fill the menu
						foreach (var it in words)
							menu.Add(it);
						menu.Add(string.Empty).IsSeparator = true;
						var itemIgnore = menu.Add("Ignore &All");
						//??var itemAdd = menu.Add("Add to &Dictionary");
						var itemStop = menu.Add("&Stop Spell-checker");

						// canceled: advance
						if (!menu.Show())
						{
							match = match.NextMatch();
							if (match.Success)
								continue;

							goto NextLine;
						}

						// selected item
						var item = menu.Items[menu.Selected];

						// stopped:
						if (item == itemStop)
							return;

						// ignore:
						if (item == itemIgnore)
						{
							_ignoreAll.Add(word);
							continue;
						}

						// replace the selected word with the suggested
						word = item.Text;
						line.SelectedText = word;
						line.UnselectText();

						// advance
						int caret = match.Index + word.Length + 1;
						if (caret < line.Text.Length)
						{
							line.Caret = caret;
							goto NextWord;
						}

						goto NextLine;
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
	}
}
