
// FarNet module RightWords
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace FarNet.RightWords;

static class Actor
{
	public static readonly List<DictionaryInfo> Dictionaries = new List<DictionaryInfo>();

	static Actor()
	{
		// home directory for dictionaries
		var home = TheHost.Instance.Manager.GetFolderPath(SpecialFolder.RoamingData, true);

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
				}
			}
		}
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
		ILine line;
		IEditor editor;

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
		var settings = Settings.Default.GetData();
		var match = MatchCaret(settings.WordRegex2, line.Text, line.Caret, false);
		if (match == null)
			return;

		// the current word
		var word = MatchToWord(match);

		// get suggestions with check
		List<string> words = null;
		var spell = MultiSpell.Get();
		if (!spell.Check(word))
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
			KnownWords.AddIgnoreWord(word);
			return;
		}

		// add to dictionary:
		if (menu.IsAddToDictionary)
		{
			AddRightWord(word);
			return;
		}

		// replace the word with the suggestion
		word = menu.Word;
		line.SelectText(match.Index, match.Index + match.Length);
		line.SelectedText = word;
		line.UnselectText();
		line.Caret = match.Index + word.Length;
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
		return regex?.Matches(text);
	}

	public static void CorrectText()
	{
		var settings = Settings.Default.GetData();

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
				Match match = MatchCaret(settings.WordRegex2, text, line.Caret, true);
				if (match == null)
					goto NextLine;

				// loop through line words (matches) with no changes
				MatchCollection skip = null;
				for (; match.Success; match = match.NextMatch())
				{
					// the target word
					var word = MatchToWord(match);

					// check cheap skip lists
					if (KnownWords.Contains(word))
						continue;

					// check spelling, expensive but better before the skip pattern
					if (spell.Check(word))
						continue;

					// expensive skip pattern
					if (HasMatch(skip ?? (skip = GetMatches(settings.SkipRegex2, text)), match))
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
						KnownWords.AddIgnoreWord(word);
						continue;
					}

					// add to dictionary:
					if (menu.IsAddToDictionary)
					{
						AddRightWord(word);
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

	internal static string GetUserDictionaryDirectory(bool create)
	{
		var settings = Settings.Default.GetData();

		var path = settings.UserDictionaryDirectory;
		if (string.IsNullOrEmpty(path))
			return TheHost.Instance.Manager.GetFolderPath(SpecialFolder.RoamingData, create);

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
		menu.Title = Text.AddToDictionary;
		menu.HelpTopic = TheHost.Instance.GetHelpTopic(HelpTopic.AddToDictionary);
		menu.Add(word);
		menu.Add(word + ", " + word2);
		if (!menu.Show())
			return null;

		return menu.Selected == 0 ? new string[] { word } : new string[] { word, word2 };
	}

	static void AddRightWord(string word)
	{
		// language names, unique, sorted
		var languages = new List<string>();
		foreach (var dic in Dictionaries)
			if (!languages.Contains(dic.Language))
				languages.Add(dic.Language);
		languages.Sort();

		// dictionary menu
		var menu = Far.Api.CreateMenu();
		menu.Title = Text.AddToDictionary;
		menu.HelpTopic = TheHost.Instance.GetHelpTopic(HelpTopic.AddToDictionary);
		menu.AutoAssignHotkeys = true;
		menu.Add(Text.Common);
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

				// write/add
				KnownWords.AddCommonWords(newWords);
				return;
			}

			// language:
			var language = menu.Items[menu.Selected].Text;
			var spell = (multiSpell ??= MultiSpell.Get()).GetSpell(language);

			// dialog
			var dialog = new UIWordDialog(word, string.Empty);
			while (dialog.Show())
			{
				// stem1 is required
				var stem1 = dialog.Stem1.Trim();
				if (stem1.Length == 0)
					continue;

				// stem2 should give results
				var stem2 = dialog.Stem2.Trim();
				if (stem2.Length > 0 && spell.WordList[stem2].Length == 0)
					continue;

				// bump known words version to refresh caches
				KnownWords.BumpVersion();

				// append to the language dictionary
				var path = GetUserDictionaryPath(language, true);
				var line = stem2.Length == 0 ? stem1 : $"{stem1} {stem2}";
				File.AppendAllLines(path, new[] { line });

				// read the updated user words
				var sb = new StringBuilder();
				spell.UserList = MultiSpell.ReadUserWords(path, spell.WordList, text => sb.AppendLine(text));
				if (sb.Length > 0)
					Far.Api.Message(sb.ToString(), "User words file warnings");
				return;
			}
		}
	}

	public static string MatchToWord(Match match)
	{
		if (match.Groups.Count < 2)
			return match.Value;

		var word = match.Value;

		for (int i = match.Groups.Count; --i >= 1;)
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
