
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using FarNet;
using FarNet.Forms;

namespace PowerShellFar
{
	/// <summary>
	/// Editor tools.
	/// </summary>
	static class EditorKit
	{
		static readonly Guid GuidBreakpoint = new Guid("01AD5C51-65EB-4DB3-B8DB-84298219FA66");
		static int _initTabExpansion;
		static ScriptBlock _TabExpansion;
		/// <summary>
		/// Expands PowerShell code in an edit line.
		/// </summary>
		/// <param name="editLine">Editor line, command line or dialog edit box line; if null then <see cref="IFar.Line"/> is used.</param>
		/// <seealso cref="Actor.ExpandCode"/>
		public static void ExpandCode(ILine editLine)
		{
			// dot-source TabExpansion.ps1 once
			if (_initTabExpansion == 0)
			{
				_initTabExpansion = -1;
				string path = A.Psf.AppHome + @"\TabExpansion.ps1";

				// TabExpansion.ps1 must exist
				if (!File.Exists(path))
					throw new FileNotFoundException("path");

				A.InvokeCode(". $args[0]", path);
				_initTabExpansion = +1;
			}

			// hot line
			if (editLine == null)
			{
				editLine = Far.Net.Line;
				if (editLine == null)
				{
					A.Message("There is no current editor line.");
					return;
				}
			}

			// line and last word
			string text = editLine.Text;
			string line = text.Substring(0, editLine.Caret);
			Match match = Regex.Match(line, @"(?:^|\s)(\S+)$");
			if (!match.Success)
				return;

			text = text.Substring(line.Length);
			string lastWord = match.Groups[1].Value;

			// compile once
			if (_TabExpansion == null)
				_TabExpansion = A.Psf.Engine.InvokeCommand.NewScriptBlock("TabExpansion $args[0] $args[1]");

			// invoke
			try
			{
				// call
				Collection<PSObject> words = _TabExpansion.Invoke(line, lastWord);

				// expand
				ExpandText(editLine, text, line, lastWord, words);
			}
			catch (RuntimeException ex)
			{
				A.Message(ex.Message);
			}
		}
		public static void ExpandText(ILine editLine, string text, string line, string lastWord, IList words)
		{
			bool isEmpty = words.Count == 0;
			int hashMode = lastWord[0] == '#' ? 1 : lastWord[lastWord.Length - 1] == '#' ? 2 : 0;

			// select a word
			string word;
			if (words.Count == 1)
			{
				// 1 word
				if (words[0] == null)
					return;
				word = words[0].ToString();
			}
			else
			{
				// make menu
				IListMenu menu = Far.Net.CreateListMenu();
				var cursor = Far.Net.UI.WindowCursor;
				menu.X = cursor.X;
				menu.Y = cursor.Y;
				Settings.Default.PopupMenu(menu);
				if (isEmpty)
				{
					menu.Add(Res.Empty).Disabled = true;
					menu.NoInfo = true;
					menu.Show();
					return;
				}
				menu.Incremental = (hashMode == 1 ? lastWord.Substring(1) : hashMode == 2 ? lastWord.Substring(0, lastWord.Length - 1) : lastWord) + "*";
				menu.IncrementalOptions = PatternOptions.Prefix;

				foreach (var it in words)
				{
					if (it != null)
						menu.Add(it.ToString());
				}

				if (menu.Items.Count == 0)
					return;

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
			}

			// expand last word

			// head before the last word
			line = line.Substring(0, line.Length - lastWord.Length);

			// #-pattern
			int index, caret = -1;
			if (hashMode != 0 && (index = word.IndexOf('#')) >= 0)
			{
				word = word.Substring(0, index) + word.Substring(index + 1);
				caret = line.Length + index;
			}
			// standard
			else
			{
				caret = line.Length + word.Length;
			}

			// set new text = old head + expanded + old tail
			editLine.Text = line + word + text;

			// set caret
			editLine.Caret = caret;
		}
		public static string ActiveText
		{
			get
			{
				// case: editor
				if (Far.Net.Window.Kind == WindowKind.Editor)
				{
					IEditor editor = Far.Net.Editor;
					if (editor.SelectionExists)
						return editor.GetSelectedText();
					return editor.Line.Text;
				}

				// other lines
				ILine line = Far.Net.Line;
				if (line == null)
					return string.Empty;
				else
					return line.ActiveText;
			}
			set
			{
				// case: editor
				if (Far.Net.Window.Kind == WindowKind.Editor)
				{
					IEditor editor = Far.Net.Editor;
					switch (editor.SelectionKind)
					{
						case PlaceKind.Column:
							throw new NotSupportedException("Rectangular selection is not supported.");
						case PlaceKind.Stream:
							editor.SetSelectedText(value);
							return;
					}

					editor.Line.Text = value;
					return;
				}

				// other lines
				ILine line = Far.Net.Line;
				if (line == null)
					throw new InvalidOperationException("There is no current text to set.");
				else
					line.ActiveText = value;
			}
		}
		public static void OnEditorOpened1(object sender, EventArgs e)
		{
			A.Psf.Invoking();

			try
			{
				string code = Settings.Default.StartupEdit;
				if (!string.IsNullOrEmpty(code))
					A.InvokeCode(code);
			}
			catch (RuntimeException ex)
			{
				throw new RuntimeException("Editor startup code failed (see configuration).", ex);
			}
			finally
			{
				Far.Net.AnyEditor.Opened -= OnEditorOpened1;
			}
		}
		public static void OnEditorOpened2(object sender, EventArgs e)
		{
			IEditor editor = (IEditor)sender;
			string fileName = editor.FileName;
			if (editor.Host == null && fileName.EndsWith(Word.ConsoleExtension, StringComparison.OrdinalIgnoreCase))
			{
				editor.Host = new EditorConsole(editor);
			}
			else if (My.PathEx.IsPSFile(fileName))
			{
				editor.KeyDown += OnKeyDownPSFile;
				editor.RegisterDrawer(new EditorDrawer(GetColors, GuidBreakpoint, 2));
			}
		}
		/// <summary>
		/// Called on redrawing in *.ps1.
		/// </summary>
		static IEnumerable<EditorColor> GetColors(IEditor editor, int startLine, int endLine, int startChar, int endChar)
		{
			var script = editor.FileName;
			var breakpoints = A.Psf.Breakpoints.Where(x => script.Equals(x.Script, StringComparison.OrdinalIgnoreCase));

			for (int indexLine = startLine; indexLine < endLine; ++indexLine)
			{
				foreach (var bp in breakpoints)
				{
					if (bp.Line != indexLine + 1)
						continue;

					yield return new EditorColor(
						indexLine,
						startChar,
						endChar,
						ConsoleColor.Black,
						ConsoleColor.Yellow);

					break;
				}
			}
		}
		/// <summary>
		/// Called on key in *.ps1.
		/// </summary>
		static void OnKeyDownPSFile(object sender, KeyEventArgs e)
		{
			// editor; skip if selected
			IEditor editor = (IEditor)sender;

			switch (e.Key.VirtualKeyCode)
			{
				case KeyCode.F1:
					{
						if (e.Key.IsShift())
						{
							// [ShiftF1]
							e.Ignore = true;
							Help.ShowHelpForContext();
						}
						return;
					}
				case KeyCode.F5:
					{
						if (e.Key.Is())
						{
							// [F5]
							e.Ignore = true;
							InvokeScriptBeingEdited(editor);
						}
						return;
					}
				case KeyCode.Tab:
					{
						if (e.Key.Is())
						{
							// [Tab]
							if (!editor.SelectionExists)
							{
								ILine line = editor.Line;
								string text = line.Text;
								int pos = line.Caret - 1;
								if (pos >= 0 && pos < line.Length && text[pos] != ' ' && text[pos] != '\t')
								{
									e.Ignore = true;
									A.Psf.ExpandCode(line);
									editor.Redraw();
								}
							}
						}
						return;
					}
			}
		}
		public static void InvokeSelectedCode()
		{
			string code;
			bool toCleanCmdLine = false;
			WindowKind wt = Far.Net.Window.Kind;

			if (wt == WindowKind.Editor)
			{
				IEditor editor = Far.Net.Editor;
				code = editor.GetSelectedText();
				if (string.IsNullOrEmpty(code))
					code = editor[editor.Caret.Y].Text;
			}
			else if (wt == WindowKind.Dialog)
			{
				IDialog dialog = Far.Net.Dialog;
				IEdit edit = dialog.Focused as IEdit;
				if (edit == null)
				{
					Far.Net.Message("The current control has to be an edit box", Res.InvokeSelectedCode);
					return;
				}
				code = edit.Line.SelectedText;
				if (string.IsNullOrEmpty(code))
					code = edit.Text;
			}
			else
			{
				ILine cl = Far.Net.CommandLine;
				code = cl.SelectedText;
				if (string.IsNullOrEmpty(code))
				{
					code = cl.Text;
					toCleanCmdLine = true;
				}
				code = Regex.Replace(code.Trim(), @"^\s*>:\s*", "");
			}
			if (code.Length == 0)
				return;

			// go
			bool ok = A.Psf.Act(code, null, wt != WindowKind.Editor);

			// clean the command line if ok
			if (ok && toCleanCmdLine && wt != WindowKind.Editor)
				Far.Net.CommandLine.Text = string.Empty;
		}
		//_110609_130945
		public static void InvokeScriptBeingEdited(IEditor editor)
		{
			// editor
			if (editor == null)
				editor = A.Psf.Editor();

			// commit
			editor.Save();

			// sync the directory and location to the script directory
			// maybe it is questionable but it is very handy too often
			string dir0, dir1;

			// save/set the directory, allow failures (e.g. a long path)
			// note: GetDirectoryName fails on a long path, too
			try
			{
				dir1 = Path.GetDirectoryName(editor.FileName);
				dir0 = Environment.CurrentDirectory;
				Environment.CurrentDirectory = dir1;
			}
			catch (PathTooLongException)
			{
				// PowerShell is not able to invoke this script anyway, almost for sure
				Far.Net.Message("The script path is too long.\rInvoking is not supported.");
				return;
			}

			try
			{
				Far.Net.UI.WindowTitle = "Running...";

				// push/set the location; let's ignore issues
				A.Psf.Engine.SessionState.Path.PushCurrentLocation(null);
				A.Psf.Engine.SessionState.Path.SetLocation(Kit.EscapeWildcard(dir1));

				// invoke the script
				A.Psf.Act("& '" + editor.FileName.Replace("'", "''") + "'", null, false);
				Far.Net.UI.WindowTitle = "Done " + DateTime.Now;
			}
			catch
			{
				Far.Net.UI.WindowTitle = "Failed";
				throw;
			}
			finally
			{
				// restore the directory first
				Environment.CurrentDirectory = dir0;

				// then pop the location, it may fail perhaps
				A.Psf.Engine.SessionState.Path.PopLocation(null);
			}
		}
	}
}
