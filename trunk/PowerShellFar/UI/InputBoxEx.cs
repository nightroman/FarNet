
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Text.RegularExpressions;
using FarNet;
using FarNet.Forms;

namespace PowerShellFar.UI
{
	class InputBoxEx
	{
		public string Title { get; set; }
		public string Prompt { get; set; }
		public string Text { get; set; }
		public string History { get; set; }
		public GetValues GetWords { get; set; }

		public bool Show()
		{
			var promptLines = FarNet.Works.Kit.SplitLines(Prompt ?? string.Empty);

			int w = Far.Api.UI.WindowSize.X - 7;
			int h = 5 + promptLines.Length;

			// dialog
			var dialog = Far.Api.CreateDialog(-1, -1, w, h);
			dialog.AddBox(3, 1, w - 4, h - 2, Title);

			// prompt
			var promptTexts = new IText[promptLines.Length];
			for (int i = 0; i < promptLines.Length; ++i)
				promptTexts[i] = dialog.AddText(5, -1, w - 6, promptLines[i]);

			// edit
			var edit = dialog.AddEdit(5, -1, w - 6, Text);
			edit.History = History;

			// expansion
			if (GetWords != null)
			{
				edit.KeyPressed += delegate(object sender, KeyPressedEventArgs e)
				{
					switch (e.Key.VirtualKeyCode)
					{
						case KeyCode.Tab:
							e.Ignore = true;
							CompleteWord(edit.Line, GetWords());
							return;
					}
				};
			}

			if (!dialog.Show())
				return false;

			Text = edit.Text;
			return true;
		}
		public static void CompleteWord(ILine editLine, IEnumerable words)
		{
			// hot line
			if (editLine == null)
			{
				editLine = Far.Api.Line;
				if (editLine == null)
				{
					A.Message("There is no current editor line.");
					return;
				}
			}

			// line and last word
			string text = editLine.Text;
			string head = text.Substring(0, editLine.Caret);
			string tail = text.Substring(head.Length);

			Match match = Regex.Match(head, @"(?:^|\s)(\S+)$");
			string lastWord = match.Success ? match.Groups[1].Value : string.Empty;

			// complete
			CompleteText(editLine, tail, head, lastWord, words);
		}
		public static void CompleteText(ILine editLine, string tail, string line, string lastWord, IEnumerable words)
		{
			// menu
			IListMenu menu = Far.Api.CreateListMenu();
			var cursor = Far.Api.UI.WindowCursor;
			menu.X = cursor.X;
			menu.Y = cursor.Y;
			Settings.Default.PopupMenu(menu);
			menu.Incremental = lastWord + "*";
			menu.IncrementalOptions = PatternOptions.Prefix;

			foreach (var it in words)
			{
				if (it == null)
					continue;

				var candidate = it.ToString();
				if (candidate.Length == 0)
				{
					menu.Add(string.Empty).IsSeparator = true;
					continue;
				}

				if (lastWord.Length == 0 || candidate.StartsWith(lastWord, StringComparison.OrdinalIgnoreCase))
					menu.Add(candidate);
			}

			if (menu.Items.Count == 0)
			{
				menu.Add(Res.Empty).Disabled = true;
				menu.NoInfo = true;
				menu.Show();
				return;
			}

			string word;
			if (menu.Items.Count == 1)
			{
				word = menu.Items[0].Text;
			}
			else
			{
				// show menu
				if (!menu.Show())
					return;
				
				word = menu.Items[menu.Selected].Text;
			}

			// expand last word

			// head before the last word
			line = line.Substring(0, line.Length - lastWord.Length);

			// new caret
			int caret = line.Length + word.Length;

			// set new text = old head + expanded + old tail
			editLine.Text = line + word + tail;

			// set caret
			editLine.Caret = caret;
		}
	}
}
