/*
PowerShellFar plugin for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
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
		static int _initTabExpansion;
		static ScriptBlock _TabExpansion;

		/// <summary>
		/// Expands PowerShell code in an edit line.
		/// </summary>
		/// <param name="editLine">Editor line, command line or dialog edit box line; if null then <see cref="IFar.Line"/> is used.</param>
		/// <remarks>
		/// It implements so called TabExpansion using a menu and inserting a selected text into a current line being edited.
		/// The edit line can belong to the internal editor, the command line or a dialogs.
		/// <para>
		/// When it is called the first time it loads the script TabExpansion.ps1 from the plugin directory
		/// which installs the global function TabExpansion. After that this function is always called and
		/// returned selected text is inserted into the edit line.
		/// </para>
		/// </remarks>
		public static void ExpandCode(ILine editLine)
		{
			if (_initTabExpansion == 0)
			{
				_initTabExpansion = -1;
				string path = A.Psf.AppHome + @"\TabExpansion.ps1";
				if (!File.Exists(path))
					return;

				A.Psf.InvokeCode(". $args[0]", path);
				_initTabExpansion = +1;
			}

			// hot line
			if (editLine == null)
			{
				editLine = A.Far.Line;
				if (editLine == null)
				{
					A.Msg("There is no current editor line.");
					return;
				}
			}

			// line and last word
			string text = editLine.Text;
			string line = text.Substring(0, editLine.Pos);
			Match match = Regex.Match(line, @"(?:^|\s)(\S+)$");
			if (!match.Success)
				return;
			
			text = text.Substring(line.Length);
			string lastWord = match.Groups[1].Value;

			// compile once
			if (_TabExpansion == null)
				_TabExpansion = A.Psf.Engine.InvokeCommand.NewScriptBlock("TabExpansion $args[0] $args[1]");

			// invoke
			Collection<PSObject> words = _TabExpansion.Invoke(line, lastWord);

			// complete expansion
			ExpandText(editLine, text, line, lastWord, words);
		}

		public static void ExpandText(ILine editLine, string text, string line, string lastWord, Collection<PSObject> words)
		{
			if (words.Count == 0)
				return;

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
				IListMenu m = A.Far.CreateListMenu();
				m.X = Console.CursorLeft;
				m.Y = Console.CursorTop;
				m.Incremental = lastWord + "*";
				m.IncrementalOptions = PatternOptions.Prefix;
				A.Psf.Settings.Intelli(m);
				foreach (PSObject o in words)
				{
					if (o != null)
						m.Add(o.ToString());
				}

				if (m.Items.Count == 0)
					return;

				if (m.Items.Count == 1)
				{
					word = m.Items[0].Text;
				}
				else
				{
					// show menu
					if (!m.Show())
						return;
					word = m.Items[m.Selected].Text;
				}
			}

			// expand last word
			line = line.Substring(0, line.Length - lastWord.Length) + word;
			editLine.Text = line + text;
			editLine.Pos = line.Length;
		}

		public static ILines HotLines
		{
			get
			{
				if (A.Far.WindowType != WindowType.Editor)
					return null;

				IEditor editor = A.Far.Editor;
				if (editor.Selection.Type == SelectionType.Stream)
					return editor.Selection;
				return editor.Lines;
			}
		}

		public static string HotText
		{
			get
			{
				ILine line = null;

				if (A.Far.WindowType == WindowType.Editor)
				{
					IEditor editor = A.Far.Editor;
					ISelection selection1 = editor.Selection;
					if (selection1.Exists)
						return selection1.GetText();

					line = editor.CurrentLine;
				}

				if (line == null)
					line = A.Far.Line;

				if (line == null)
					return string.Empty;

				ILineSelection selection2 = line.Selection;
				if (selection2.Start >= 0)
					return selection2.Text;

				return line.Text;
			}
			set
			{
				ILine line = null;

				if (A.Far.WindowType == WindowType.Editor)
				{
					IEditor editor = A.Far.Editor;
					ISelection selection1 = editor.Selection;
					if (selection1.Type == SelectionType.Rect)
						throw new NotSupportedException("Rectangular selection is not supported.");

					if (selection1.Type == SelectionType.Stream)
					{
						selection1.SetText(value);
						return;
					}

					line = editor.CurrentLine;
				}

				if (line == null)
					line = A.Far.Line;

				if (line == null)
					throw new InvalidOperationException("There is no current text to set.");

				ILineSelection selection2 = line.Selection;
				if (selection2.Start >= 0)
					selection2.Text = value;
				else
					line.Text = value;
			}
		}

		public static void OnEditorOpened1(object sender, EventArgs e)
		{
			A.Psf.Invoking();

			try
			{
				string code = A.Psf.Settings.PluginStartupEdit;
				if (!string.IsNullOrEmpty(code))
					A.Psf.InvokeCode(code);
			}
			catch (RuntimeException ex)
			{
				throw new RuntimeException("Editor startup code failed (see configuration).", ex);
			}
			finally
			{
				A.Far.AnyEditor.Opened -= OnEditorOpened1;
			}
		}

		public static void OnEditorOpened2(object sender, EventArgs e)
		{
			IEditor editor = (IEditor)sender;
			string fileName = editor.FileName;
			if (editor.Host == null && fileName.EndsWith(".psfconsole", StringComparison.OrdinalIgnoreCase))
			{
				editor.Host = new EditorConsole(editor);
			}
			else if (My.PathEx.IsPSFile(fileName))
			{
				editor.OnKey += OnKeyPSFile;
			}
		}

		/// <summary>
		/// Called on key in *.ps1.
		/// </summary>
		static void OnKeyPSFile(object sender, KeyEventArgs e)
		{
			// skip some keys
			if (!e.Key.KeyDown || e.Key.CtrlAltShift != ControlKeyStates.None)
				return;

			// editor; skip if selected
			IEditor editor = (IEditor)sender;

			switch (e.Key.VirtualKeyCode)
			{
				case VKeyCode.F5:
					{
						InvokeScriptFromEditor(editor);
						break;
					}
				case VKeyCode.Tab:
					{
						if (editor.Selection.Exists)
							return;

						ILine line = editor.CurrentLine;
						string text = line.Text;
						int pos = line.Pos - 1;
						if (pos >= 0 && pos < line.Length && text[pos] != ' ' && text[pos] != '\t')
						{
							e.Ignore = true;
							A.Psf.ExpandCode(line);
							editor.Redraw();
						}
						return;
					}
			}
		}

		public static void InvokeScriptFromEditor(IEditor editor)
		{
			// editor
			if (editor == null)
				editor = A.Psf.Editor();

			// modified? save
			if (editor.IsModified)
				editor.Save();

			// sync location to file
			string dir = Path.GetDirectoryName(editor.FileName);
			if (dir.Length < 260)
			{
				Environment.CurrentDirectory = dir;
				A.Psf.Engine.SessionState.Path.SetLocation(Kit.EscapeWildcard(dir));
			}

			// command
			string code = "& '" + editor.FileName.Replace("'", "''") + "'";

			// invoke
			Console.Title = "Running...";
			try
			{
				using (ExternalOutputWriter writer = new ExternalOutputWriter())
					A.Psf.InvokePipeline(code, writer, false);
				
				Console.Title = "Done " + DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
			}
			catch
			{
				Console.Title = "Failed";
				throw;
			}
		}

		public static void InvokeSelectedCode()
		{
			string code;
			bool toCleanCmdLine = false;
			WindowType wt = A.Far.WindowType;

			if (wt == WindowType.Editor)
			{
				IEditor editor = A.Far.Editor;
				code = editor.Selection.GetText();
				if (string.IsNullOrEmpty(code))
					code = editor.Lines[editor.Cursor.Y].Text;
			}
			else if (wt == WindowType.Dialog)
			{
				IDialog dialog = A.Far.Dialog;
				IEdit edit = dialog.Focused as IEdit;
				if (edit == null)
				{
					A.Far.Msg("The current control has to be an edit box", Res.InvokeSelectedCode);
					return;
				}
				code = edit.Line.Selection.Text;
				if (string.IsNullOrEmpty(code))
					code = edit.Text;
			}
			else
			{
				ILine cl = A.Far.CommandLine;
				code = cl.Selection.Text.Trim();
				if (code.Length == 0)
				{
					code = A.Far.CommandLine.Text;
					toCleanCmdLine = true;
				}
				code = Regex.Replace(code, @"^\s*>:\s*", "");
			}
			if (code.Length == 0)
				return;

			// go
			bool ok = A.Psf.InvokePipeline(code, null, wt != WindowType.Editor);

			// clean the command line if ok
			if (ok && toCleanCmdLine && wt != WindowType.Editor)
				A.Far.CommandLine.Text = string.Empty;
		}

	}

	/// <summary>
	/// Non-modal data editor with assumed post-processing on saving.
	/// </summary>
	class DataEditor
	{
		/// <summary>
		/// Editor interface.
		/// </summary>
		public IEditor Editor
		{
			get { return _Editor; }
		}
		readonly IEditor _Editor = A.Far.CreateEditor();

		public DataEditor()
		{
			_Editor.CodePage = 1200;
			_Editor.DeleteSource = DeleteSource.File;
			_Editor.DisableHistory = true;
			_Editor.IsNew = true;
		}
	}

	class MemberEditor : DataEditor
	{
		object _instance;
		PSPropertyInfo _info;

		public void Open(string filePath, bool delete, object instance, PSPropertyInfo info)
		{
			_instance = instance;
			_info = info;

			Editor.DeleteSource = delete ? DeleteSource.File : DeleteSource.None;
			Editor.FileName = filePath;
			Editor.Title = _info.Name;
			Editor.Saving += Saving;

			Editor.Open(OpenMode.None);
		}

		void Saving(object sender, EventArgs e)
		{
			bool isPSObject;
			object value;
			string type;
			if (_info.Value is PSObject && (_info.Value as PSObject).BaseObject != null)
			{
				isPSObject = true;
				type = (_info.Value as PSObject).BaseObject.GetType().FullName;
			}
			else
			{
				isPSObject = false;
				type = _info.TypeNameOfValue;
			}

			if (type == "System.Collections.ArrayList" || type.EndsWith("]", StringComparison.Ordinal))
			{
				ArrayList lines = new ArrayList();
				Editor.Begin();
				foreach (string s in Editor.TrueLines.Strings)
					lines.Add(s);
				Editor.End();
				value = lines;
			}
			else
			{
				value = Editor.GetText().TrimEnd();
			}

			if (isPSObject)
				value = PSObject.AsPSObject(value);

			try
			{
				A.SetMemberValue(_info, value);
				MemberPanel.WhenMemberChanged(_instance);
			}
			catch (RuntimeException ex)
			{
				A.Far.Msg(ex.Message, "Setting property");
			}
		}
	}

	class PropertyEditor : DataEditor
	{
		string _itemPath;
		PSPropertyInfo _info;

		public void Open(string filePath, bool delete, string itemPath, PSPropertyInfo info)
		{
			_itemPath = itemPath;
			_info = info;

			Editor.DeleteSource = delete ? DeleteSource.File : DeleteSource.None;
			Editor.FileName = filePath;
			Editor.Title = _itemPath + "." + _info.Name;
			Editor.Saving += Saving;

			Editor.Open(OpenMode.None);
		}

		void Saving(object sender, EventArgs e)
		{
			try
			{
				object value;
				if (_info.TypeNameOfValue.EndsWith("]", StringComparison.Ordinal))
				{
					ArrayList lines = new ArrayList();
					Editor.Begin();
					foreach (string s in Editor.TrueLines.Strings)
						lines.Add(s);
					Editor.End();
					value = lines;
				}
				else
				{
					value = Editor.GetText().TrimEnd();
				}

				A.SetPropertyValue(_itemPath, _info.Name, Converter.Parse(_info, value));
				PropertyPanel.WhenPropertyChanged(_itemPath);
			}
			catch (RuntimeException ex)
			{
				A.Msg(ex);
			}
		}
	}

	class ItemEditor : DataEditor
	{
		string _itemPath;
		AnyPanel _panel;

		public void Open(string filePath, bool delete, string itemPath, AnyPanel panel)
		{
			_itemPath = itemPath;
			_panel = panel;

			Editor.DeleteSource = delete ? DeleteSource.File : DeleteSource.None;
			Editor.FileName = filePath;
			Editor.Title = _itemPath;
			Editor.Saving += Saving;

			Editor.Open(OpenMode.None);
		}

		void Saving(object sender, EventArgs e)
		{
			try
			{
				// read
				string text = Editor.GetText().TrimEnd();

				// set
				if (!A.SetContentUI(_itemPath, text))
					return;

				// update a panel
				if (_panel != null)
					_panel.UpdateRedraw(false);
			}
			catch (RuntimeException ex)
			{
				A.Msg(ex.Message);
			}
		}
	}

}
