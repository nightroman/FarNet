
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
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
		const string CompletionText = "CompletionText";
		const string ListItemText = "ListItemText";

		static int _initTabExpansion;
		static string _callTabExpansion;
		
		static void InitTabExpansion()
		{
			if (_initTabExpansion != 0)
				return;

			_initTabExpansion = -1;

			string path;

			if (A.PSVersion.Major > 2)
			{
				path = A.Psf.AppHome + @"\TabExpansion2.ps1";
				_callTabExpansion = @"
param($inputScript, $cursorColumn)
$r = TabExpansion2 $inputScript $cursorColumn
@{
	CompletionMatches = @(foreach($_ in $r.CompletionMatches) { @{CompletionText = $_.CompletionText; ListItemText = $_.ListItemText} })
	ReplacementIndex = $r.ReplacementIndex
	ReplacementLength = $r.ReplacementLength
}
";
			}
			else
			{
				path = A.Psf.AppHome + @"\TabExpansion.ps1";
				_callTabExpansion = @"
param($inputScript, $cursorColumn)
$line = $inputScript.Substring(0, $cursorColumn)
$word = if ($line -match '(?:^|\s)(\S+)$') {$matches[1]} else {''}
@{
	CompletionMatches = @(TabExpansion $line $word)
	ReplacementIndex = $line.Length - $word.Length
	ReplacementLength = $word.Length
}
";
			}

			// TabExpansion.ps1 must exist
			if (!File.Exists(path))
				throw new FileNotFoundException("path");

			A.InvokeCode(". $args[0]", path);
			_initTabExpansion = +1;
		}
		static string TECompletionText(object value)
		{
			var t = value as Hashtable;
			if (t == null)
				return value.ToString();

			return t[CompletionText].ToString();
		}
		static string TEListItemText(object value)
		{
			var t = value as Hashtable;
			if (t == null)
				return value.ToString();

			var r = t[ListItemText];
			if (r != null)
				return r.ToString();

			return t[CompletionText].ToString();
		}
		/// <summary>
		/// Expands PowerShell code in an edit line.
		/// </summary>
		/// <param name="editLine">Editor line, command line or dialog edit box line; if null then <see cref="IFar.Line"/> is used.</param>
		/// <param name="runspace">Runspace or null for the main.</param>
		/// <seealso cref="Actor.ExpandCode"/>
		public static void ExpandCode(ILine editLine, Runspace runspace)
		{
			InitTabExpansion();

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

			int lineOffset = 0;
			string inputScript;
			int cursorColumn;
			var prefix = string.Empty;

			IEditor editor = null;
			EditorConsole console;
			EditorConsole.Area area;

			// script?
			if (A.PSVersion.Major > 2 && editLine.WindowKind == WindowKind.Editor && My.PathEx.IsPSFile((editor = Far.Api.Editor).FileName))
			{
				int lineIndex = editor.Caret.Y;
				int lastIndex = editor.Count - 1;

				// previous text
				var sb = new StringBuilder();
				for (int i = 0; i < lineIndex; ++i)
					sb.AppendLine(editor[i].Text);

				// current line
				lineOffset = sb.Length;
				cursorColumn = lineOffset + editLine.Caret;

				// remaining text
				for (int i = lineIndex; i < lastIndex; ++i)
					sb.AppendLine(editor[i].Text);
				sb.Append(editor[lastIndex]);

				// whole text
				inputScript = sb.ToString();
			}
			// area?
			else if (editor != null && (console = editor.Host as EditorConsole) != null && (area = console.GetCommandArea()) != null)
			{
				int lineIndex = area.Caret.Y;
				int lastIndex = area.LastLineIndex;

				// previous text
				var sb = new StringBuilder();
				for (int i = area.FirstLineIndex; i < lineIndex; ++i)
					sb.AppendLine(editor[i].Text);

				// current line
				lineOffset = sb.Length;
				cursorColumn = lineOffset + area.Caret.X;

				// remaining text
				for (int i = lineIndex; i < lastIndex; ++i)
					sb.AppendLine(editor[i].Text);
				sb.Append(editor[lastIndex]);

				// whole text
				inputScript = sb.ToString();
			}
			// line
			else
			{
				// original line
				inputScript = editLine.Text;
				cursorColumn = editLine.Caret;

				// process prefix
				if (editLine.WindowKind == WindowKind.Panels)
					Entry.SplitCommandWithPrefix(ref inputScript, out prefix);

				// correct caret
				cursorColumn -= prefix.Length;
				if (cursorColumn < 0)
					return;
			}

			// skip empty (also avoid errors)
			if (inputScript.Length == 0)
				return;

			// invoke
			try
			{
				// call TabExpansion
				Hashtable result;
				using (var ps = runspace == null ? A.Psf.NewPowerShell() : PowerShell.Create())
				{
					if (runspace != null)
						ps.Runspace = runspace;

					result = (Hashtable)ps.AddScript(_callTabExpansion, true).AddArgument(inputScript).AddArgument(cursorColumn).Invoke()[0].BaseObject;
				}

				// results
				var words = (IList)result["CompletionMatches"];
				int replacementIndex = (int)result["ReplacementIndex"];
				int replacementLength = (int)result["ReplacementLength"];
				replacementIndex -= lineOffset;
				if (replacementIndex < 0 || replacementLength < 0)
					return;

				// variables from the current editor
				if (editLine.WindowKind == WindowKind.Editor)
				{
					// replaced text
					var lastWord = inputScript.Substring(lineOffset + replacementIndex, replacementLength);

					//! as TabExpansion.ps1 but ends with \$(\w*)$
					var matchVar = Regex.Match(lastWord, @"^(.*[!;\(\{\|""'']*)\$(global:|script:|private:)?(\w*)$", RegexOptions.IgnoreCase);
					if (matchVar.Success)
					{
						var start = matchVar.Groups[1].Value;
						var scope = matchVar.Groups[2].Value;
						var re = new Regex(@"\$(global:|script:|private:)?(" + scope + matchVar.Groups[3].Value + @"\w+:?)", RegexOptions.IgnoreCase);

						var variables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
						foreach (var line1 in Far.Api.Editor.Lines)
						{
							foreach (Match m in re.Matches(line1.Text))
							{
								var all = m.Value;
								if (all[all.Length - 1] != ':')
								{
									variables.Add(start + all);
									if (scope.Length == 0 && m.Groups[1].Value.Length > 0)
										variables.Add(start + "$" + m.Groups[2].Value);
								}
							}
						}

						// union lists
						foreach (var x in words)
							if (x != null)
								variables.Add(TECompletionText(x));

						// final sorted list
						words = variables.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
					}
				}

				// expand
				ExpandText(editLine, replacementIndex + prefix.Length, replacementLength, words);
			}
			catch (RuntimeException) { }
		}
		public static void ExpandText(ILine editLine, int replacementIndex, int replacementLength, IList words)
		{
			bool isEmpty = words.Count == 0;
			var text = editLine.Text;
			var last = replacementIndex + replacementLength - 1;
			bool custom = last > 0 && text[last] == '=';

			// select a word
			string word;
			if (words.Count == 1)
			{
				// 1 word
				if (words[0] == null)
					return;

				word = TECompletionText(words[0]);
			}
			else
			{
				// make menu
				IListMenu menu = Far.Api.CreateListMenu();
				var cursor = Far.Api.UI.WindowCursor;
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

				menu.Incremental = "*";
				menu.IncrementalOptions = PatternOptions.Substring;

				foreach (var it in words)
				{
					if (it == null) continue;
					var item = new SetItem();
					item.Text = TEListItemText(it);
					item.Data = it;
					menu.Items.Add(item);
				}

				if (menu.Items.Count == 0)
					return;

				if (menu.Items.Count == 1)
				{
					word = TECompletionText(menu.Items[0].Data);
				}
				else
				{
					// show menu
					if (!menu.Show())
						return;
					word = TECompletionText(menu.Items[menu.Selected].Data);
				}
			}

			// replace

			// head before replaced part
			string head = text.Substring(0, replacementIndex);

			// custom pattern
			int index, caret;
			if (custom && (index = word.IndexOf('#')) >= 0)
			{
				word = word.Substring(0, index) + word.Substring(index + 1);
				caret = head.Length + index;
			}
			// standard
			else
			{
				caret = head.Length + word.Length;
			}

			// set new text = old head + expanded + old tail
			editLine.Text = head + word + text.Substring(replacementIndex + replacementLength);

			// set caret
			editLine.Caret = caret;
		}
		public static string ActiveText
		{
			get
			{
				// case: editor
				if (Far.Api.Window.Kind == WindowKind.Editor)
				{
					var editor = Far.Api.Editor;
					if (editor.SelectionExists)
						return editor.GetSelectedText();
					return editor.Line.Text;
				}

				// other lines
				ILine line = Far.Api.Line;
				if (line == null)
					return string.Empty;
				else
					return line.ActiveText;
			}
			set
			{
				// case: editor
				if (Far.Api.Window.Kind == WindowKind.Editor)
				{
					var editor = Far.Api.Editor;
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
				ILine line = Far.Api.Line;
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
				Far.Api.AnyEditor.Opened -= OnEditorOpened1;
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
				editor.Changed += OnChangedPSFile;
			}
		}
		static void OnChangedPSFile(object sender, EditorChangedEventArgs e)
		{
			if (e.Kind == EditorChangeKind.LineChanged)
				return;

			var editor = (IEditor)sender;
			var script = editor.FileName;
			var line = e.Line + 1;

			IEnumerable<LineBreakpoint> bps = null;
			int delta = 0;
			if (e.Kind == EditorChangeKind.LineAdded)
			{
				delta = 1;
				bps = A.Psf.Breakpoints.Where(x => x.Line >= line && x.Script.Equals(script, StringComparison.OrdinalIgnoreCase)).ToArray();
			}
			else
			{
				var bp = A.Psf.Breakpoints.FirstOrDefault(x => x.Line == line && x.Script.Equals(script, StringComparison.OrdinalIgnoreCase));
				if (bp != null)
					A.RemoveBreakpoint(bp);

				delta = -1;
				bps = A.Psf.Breakpoints.Where(x => x.Line > line && x.Script.Equals(script, StringComparison.OrdinalIgnoreCase)).ToArray();
			}

			foreach (var bp in bps)
			{
				A.RemoveBreakpoint(bp);
				A.SetBreakpoint(bp.Script, bp.Line + delta, bp.Action);
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
							if (!editor.SelectionExists && NeedsTabExpansion(editor))
							{
								// TabExpansion
								e.Ignore = true;
								A.Psf.ExpandCode(editor.Line);
								editor.Redraw();
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
			WindowKind wt = Far.Api.Window.Kind;

			if (wt == WindowKind.Editor)
			{
				var editor = Far.Api.Editor;
				code = editor.GetSelectedText();
				if (string.IsNullOrEmpty(code))
					code = editor[editor.Caret.Y].Text;
			}
			else if (wt == WindowKind.Dialog)
			{
				IDialog dialog = Far.Api.Dialog;
				IEdit edit = dialog.Focused as IEdit;
				if (edit == null)
				{
					Far.Api.Message("The current control has to be an edit box", Res.InvokeSelectedCode);
					return;
				}
				code = edit.Line.SelectedText;
				if (string.IsNullOrEmpty(code))
					code = edit.Text;
			}
			else
			{
				ILine cl = Far.Api.CommandLine;
				code = cl.SelectedText;
				if (string.IsNullOrEmpty(code))
				{
					code = cl.Text;
					toCleanCmdLine = true;
				}

				string prefix;
				Entry.SplitCommandWithPrefix(ref code, out prefix);
			}
			if (code.Length == 0)
				return;

			// go
			bool ok = A.Psf.Act(code, null, wt != WindowKind.Editor);

			// clean the command line if ok
			if (ok && toCleanCmdLine && wt != WindowKind.Editor)
				Far.Api.CommandLine.Text = string.Empty;
		}
		// PSF sets the current directory and location to the script directory.
		// This is often useful and consistent with invoking from panels.
		// NOTE: ISE [F5] does not.
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
				Far.Api.Message("The script path is too long.\rInvoking is not supported.");
				return;
			}

			try
			{
				Far.Api.UI.WindowTitle = "Running...";

				// push/set the location; let's ignore issues
				A.Psf.Engine.SessionState.Path.PushCurrentLocation(null);
				A.Psf.Engine.SessionState.Path.SetLocation(Kit.EscapeWildcard(dir1));

				// invoke the script
				A.Psf.Act("& '" + editor.FileName.Replace("'", "''") + "'", null, false);
				Far.Api.UI.WindowTitle = "Done " + DateTime.Now;
			}
			catch
			{
				Far.Api.UI.WindowTitle = "Failed";
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
		// true if there is a solid char anywhere before the caret
		internal static bool NeedsTabExpansion(IEditor editor)
		{
			ILine line = editor.Line;
			string text = line.Text;

			int pos = line.Caret;
			while (--pos >= 0)
				if (text[pos] > ' ')
					return true;

			return false;
		}
	}
}
