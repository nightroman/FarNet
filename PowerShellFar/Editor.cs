/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
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

				A.Psf.InvokeCode(". $args[0]", path);
				_initTabExpansion = +1;
			}

			// hot line
			if (editLine == null)
			{
				editLine = Far.Net.Line;
				if (editLine == null)
				{
					A.Msg("There is no current editor line.");
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
				IListMenu m = Far.Net.CreateListMenu();
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
			editLine.Caret = line.Length;
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
					return editor[-1].Text;
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

					editor[-1].Text = value;
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
				string code = A.Psf.Settings.StartupEdit;
				if (!string.IsNullOrEmpty(code))
					A.Psf.InvokeCode(code);
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
				case VKeyCode.F1:
					{
						if (e.Key.CtrlAltShift == ControlKeyStates.ShiftPressed)
						{
							// [ShiftF1]
							e.Ignore = true;
							Help.ShowHelp();
						}
						return;
					}
				case VKeyCode.F5:
					{
						if (e.Key.CtrlAltShift == ControlKeyStates.None)
						{
							// [F5]
							e.Ignore = true;
							InvokeScriptFromEditor(editor);
						}
						return;
					}
				case VKeyCode.Tab:
					{
						if (e.Key.CtrlAltShift == ControlKeyStates.None)
						{
							// [Tab]
							if (!editor.SelectionExists)
							{
								ILine line = editor[-1];
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
			bool ok = A.Psf.InvokePipeline(code, null, wt != WindowKind.Editor);

			// clean the command line if ok
			if (ok && toCleanCmdLine && wt != WindowKind.Editor)
				Far.Net.CommandLine.Text = string.Empty;
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
		readonly IEditor _Editor = Far.Net.CreateEditor();

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

				Editor.BeginAccess();
				foreach (ILine line in Editor.Lines)
					lines.Add(line.Text);
				Editor.EndAccess();
	
				if (lines[lines.Count - 1].ToString().Length == 0)
					lines.RemoveAt(lines.Count - 1);
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
				Far.Net.Message(ex.Message, "Setting property");
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

					Editor.BeginAccess();
					foreach (ILine line in Editor.Lines)
						lines.Add(line.Text);
					Editor.EndAccess();

					if (lines[lines.Count - 1].ToString().Length == 0)
						lines.RemoveAt(lines.Count - 1);
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
