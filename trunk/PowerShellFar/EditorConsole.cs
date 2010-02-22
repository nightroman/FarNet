/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Editor console.
	/// </summary>
	class EditorConsole
	{
		/// <summary>
		/// Creates an editor console.
		/// </summary>
		/// <remarks>
		/// With prompt may return null if a user cancels.
		/// </remarks>
		public static EditorConsole CreateConsole(bool prompt)
		{
			string dir = Path.Combine(A.Psf.AppData, "psfconsole");
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			int mode = 0;
			string name = null;
			if (prompt)
			{
				string[] files = Directory.GetFiles(dir, "*" + Word.ConsoleExtension);
				IMenu menu = Far.Net.CreateMenu();
				menu.AutoAssignHotkeys = true;
				menu.Title = "Open Editor Console";
				menu.Bottom = "[Shift+|Ctrl+]Enter";
				menu.Add("* New console or session *");
				menu.HelpTopic = A.Psf.HelpTopic + "EditorConsoleMenuOpen";

				IAnyPanel panel = null;
				if ((Far.Net.WindowKind == WindowKind.Panels) && (null != (panel = Far.Net.Panel)) && (panel.Kind != PanelKind.File))
					panel = null;
				
				// break keys
				menu.BreakKeys.Add(VKeyCode.Enter | VKeyMode.Shift);
				menu.BreakKeys.Add(VKeyCode.Enter | VKeyMode.Ctrl);
				if (panel != null)
					menu.BreakKeys.Add(VKeyCode.Spacebar);

				if (files.Length > 0)
				{
					menu.Add("Saved Consoles").IsSeparator = true;
					foreach (string file in files)
						menu.Add(Path.GetFileName(file));
				}
				if (!menu.Show())
					return null;

				name = menu.Items[menu.Selected].Text;
				if (name.Length > 0 && name[0] == '*')
					name = null;

				switch (menu.BreakKey)
				{
					case (VKeyCode.Enter | VKeyMode.Shift):
						mode = 1;
						break;
					case (VKeyCode.Enter | VKeyMode.Ctrl):
						mode = 2;
						break;
					case VKeyCode.Spacebar:
						string path = name == null ? dir + "\\" : Path.Combine(dir, name);
						panel.GoToPath(path);
						return null;
				}
			}

			// editor
			IEditor editor = Far.Net.CreateEditor();

			// new file, generate a name, set Unicode, don't history
			if (name == null)
			{
				name = Kit.ToString(DateTime.Now, "_yyMMdd_HHmmss") + Word.ConsoleExtension;
				editor.CodePage = Encoding.Unicode.CodePage;
				editor.DisableHistory = true;
				editor.IsNew = true;
			}

			// do not set code page now
			editor.FileName = Path.Combine(dir, name);

			// create the console and attach it as the host to avoid conflicts
			EditorConsole r = new EditorConsole(editor, mode);
			editor.Host = r;
			return r;
		}

		public IEditor Editor { get; private set; }

		FarUI FarUI;
		FarHost FarHost;
		Runspace Runspace;
		PowerShell PowerShell;

		public EditorConsole(IEditor editor) : this(editor, 0) { }

		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
		public EditorConsole(IEditor editor, int mode)
		{
			Editor = editor;
			Editor.OnKey += OnKey;

			switch (mode)
			{
				case 1:
					OpenLocalSession();
					break;
				case 2:
					OpenRemoteSession();
					break;
			}
		}

		void CloseSession()
		{
			if (Runspace != null)
			{
				Runspace.Close();
				Runspace = null;
			}
		}

		void EnsureHost()
		{
			if (FarHost == null)
			{
				Editor.Closed += delegate { CloseSession(); };
				Editor.CtrlCPressed += OnCtrlCPressed;
				FarUI = new FarUI();
				FarHost = new FarHost(FarUI);
			}
		}

		void OpenLocalSession()
		{
			EnsureHost();

			Runspace = RunspaceFactory.CreateRunspace(FarHost, Runspace.DefaultRunspace.RunspaceConfiguration);
			Runspace.Open();
		}

		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
		void OpenRemoteSession()
		{
			UI.ConnectionDialog dialog = new UI.ConnectionDialog("New Remote Editor Console");
			if (!dialog.Show())
				return;

			string computerName = (dialog.ComputerName.Length == 0 || dialog.ComputerName == ".") ? "localhost" : dialog.ComputerName;
			PSCredential credential = null;
			if (dialog.UserName.Length > 0)
			{
				credential = NativeMethods.PromptForCredential(null, null, dialog.UserName, string.Empty, PSCredentialTypes.Generic | PSCredentialTypes.Domain, PSCredentialUIOptions.Default);
				if (credential == null)
					return;
			}

			WSManConnectionInfo connectionInfo = new WSManConnectionInfo(false, computerName, 0, null, null, credential);

			EnsureHost();

			Runspace = RunspaceFactory.CreateRunspace(FarHost, connectionInfo);
			Runspace.Open();
		}

		//! This method is sync and uses pipeline, that is why we must not null the pipeline async.
		void OnCtrlCPressed(object sender, EventArgs e)
		{
			if (PowerShell != null && PowerShell.InvocationStateInfo.State == PSInvocationState.Running)
			{
				//! Stop() tends to hang.
				PowerShell.BeginStop(AsyncStop, null);
			}
		}

		void OnF1()
		{
			IMenu menu = Far.Net.CreateMenu();
			menu.Title = "Editor Console";
			menu.HelpTopic = A.Psf.HelpTopic + "EditorConsole";
			if (Runspace != null)
				menu.Add("&Global session").Click = OnGlobalSession;
			menu.Add("New &local session").Click = delegate { OpenLocalSession(); };
			menu.Add("New &remote session").Click = delegate { OpenRemoteSession(); };
			menu.Add("&Help").Click = delegate { Far.Net.ShowHelp(A.Psf.AppHome, "EditorConsole", HelpOptions.Path); };
			menu.Show();
		}

		void OnGlobalSession(object sender, EventArgs e)
		{
			CloseSession();
		}

		/// <summary>
		/// Called on key in psfconsole.
		/// </summary>
		void OnKey(object sender, KeyEventArgs e)
		{
			// drop pipeline now, if any
			PowerShell = null;

			// skip some keys
			if (!e.Key.KeyDown)
				return;

			// skip if selected
			if (Editor.Selection.Exists)
				return;

			switch (e.Key.VirtualKeyCode)
			{
				case VKeyCode.Enter:
					{
						if (e.Key.CtrlAltShift == ControlKeyStates.None)
						{
							// [Enter]
							e.Ignore = true;
							Invoke();
						}
						return;
					}
				case VKeyCode.Tab:
					{
						if (e.Key.CtrlAltShift == ControlKeyStates.None)
						{
							// [Tab]
							if (Editor.IsLastLine)
							{
								e.Ignore = true;
								if (Runspace == null)
									EditorKit.ExpandCode(Editor.CurrentLine);
								else
									ExpandCode(Editor.CurrentLine);

								Editor.Redraw();
							}
						}
						return;
					}
				case VKeyCode.Escape:
					{
						if (e.Key.CtrlAltShift == ControlKeyStates.None)
						{
							// [Esc]
							if (Editor.IsLastLine && Editor.CurrentLine.Length > 0)
							{
								e.Ignore = true;
								ILine line = Editor.CurrentLine;
								line.Text = string.Empty;
								line.Pos = 0;
								Editor.Redraw();
							}
						}
						return;
					}
				case VKeyCode.End:
					{
						if (e.Key.CtrlAltShift == ControlKeyStates.None)
						{
							// [End]
							if (!Editor.IsLastLine)
								return;

							ILine curr = Editor.CurrentLine;
							if (curr.Pos != curr.Length)
								return;

							string pref = curr.Text;
							if (pref.Length > 0 && pref[0] != '*')
								pref = "^" + Regex.Escape(pref);
							UI.CommandHistoryMenu m = new UI.CommandHistoryMenu(pref);
							string code = m.Show();
							if (code == null)
								return;

							e.Ignore = true;
							curr.Text = code;
							curr.Pos = -1;
							Editor.Redraw();
						}
						return;
					}
				case VKeyCode.UpArrow:
					goto case VKeyCode.DownArrow;
				case VKeyCode.DownArrow:
					{
						if (e.Key.CtrlAltShift == ControlKeyStates.None)
						{
							// [Up], [Down]
							if (!Editor.IsLastLine)
								return;

							string lastUsedCmd = null;
							if (History.Cache == null)
							{
								// don't lose not empty line!
								if (Editor.CurrentLine.Length > 0)
									return;
								History.Cache = History.GetLines(0);
								History.CacheIndex = History.Cache.Length;
							}
							else if (History.CacheIndex >= 0 && History.CacheIndex < History.Cache.Length)
							{
								lastUsedCmd = History.Cache[History.CacheIndex];
							}
							string code;
							if (e.Key.VirtualKeyCode == 38)
							{
								for (; ; )
								{
									if (--History.CacheIndex < 0)
									{
										code = string.Empty;
										History.CacheIndex = -1;
									}
									else
									{
										code = History.Cache[History.CacheIndex];
										if (code == lastUsedCmd)
											continue;
									}
									break;
								}
							}
							else
							{
								for (; ; )
								{
									if (++History.CacheIndex >= History.Cache.Length)
									{
										code = string.Empty;
										History.CacheIndex = History.Cache.Length;
									}
									else
									{
										code = History.Cache[History.CacheIndex];
										if (code == lastUsedCmd)
											continue;
									}
									break;
								}
							}

							e.Ignore = true;
							ILine curr = Editor.CurrentLine;
							curr.Text = code;
							curr.Pos = -1;
							Editor.Redraw();
						}
						return;
					}
				case VKeyCode.Delete:
					{
						if (e.Key.CtrlAltShift == ControlKeyStates.None)
						{
							// [Del]
							if (!Editor.IsLastLine)
								return;

							ILine curr = Editor.CurrentLine;
							if (curr.Length > 0)
								return;

							e.Ignore = true;

							Editor.Begin();
							Point pt = Editor.Cursor;
							for (int i = pt.Y - 1; i >= 0; --i)
							{
								string text = Editor.Lines[i].Text;
								if (text == "<=")
								{
									Editor.Selection.Select(RegionKind.Stream, 0, i, -1, pt.Y);
									Editor.Selection.Clear();
									break;
								}
								if (text == "=>")
								{
									pt = new Point(-1, i + 1);
									continue;
								}
							}
							Editor.End();

							Editor.Redraw();
						}
						return;
					}
				case VKeyCode.F1:
					{
						if (e.Key.CtrlAltShift == ControlKeyStates.None)
						{
							// [F1]
							e.Ignore = true;
							OnF1();
						}
						else if (e.Key.CtrlAltShift == ControlKeyStates.ShiftPressed)
						{
							// [ShiftF1]
							e.Ignore = true;
							Help.ShowHelp();
						}
						return;
					}
				default:
					{
						if (e.Key.Character != 0)
							History.Cache = null;
						return;
					}
			}
		}

		internal void Invoke()
		{
			// current line and script, skip empty
			ILine curr = Editor.CurrentLine;
			string code = curr.Text;
			if (code.Length == 0)
				return;

			// end?
			if (!Editor.IsLastLine)
			{
				// - no, copy code and exit
				Editor.GoEnd(true);
				Editor.Insert(code);
				Editor.Redraw();
				return;
			}

			// go end
			curr.Pos = -1;

			// go async
			if (Runspace != null)
			{
				InvokePipeline(code);
				return;
			}

			// invoke
			EditorOutputWriter2 writer = new EditorOutputWriter2(Editor);
			Editor.BeginUndo();

			// default runspace
			A.Psf.InvokePipeline(code, writer, true);
			if (Editor != Far.Net.Editor)
			{
				Far.Net.Message(Res.EditorConsoleCannotComplete);
			}
			else
			{
				if (writer.WriteCount > 0)
					Editor.Insert("=>\r");
				else
					Editor.InsertLine();

				Editor.EndUndo();
				Editor.Redraw();
			}
		}

		void InvokePipeline(string code)
		{
			// drop history cache
			History.Cache = null;

			// push writer
			FarUI.PushWriter(new EditorOutputWriter1(Editor));

			// invoke
			try
			{
				// history
				code = code.Trim();
				if (code.Length > 0 && code[code.Length - 1] != '#' && A.Psf._myLastCommand != code)
				{
					History.AddLine(code);
					A.Psf._myLastCommand = code;
				}

				// invoke command
				PowerShell = PowerShell.Create();
				PowerShell.Runspace = Runspace;
				PowerShell.Commands
					.AddScript(code)
					.AddCommand(A.OutCommand);

				Editor.BeginAsync();
				PowerShell.BeginInvoke<PSObject>(null, null, AsyncInvoke, null);
			}
			catch (RuntimeException ex)
			{
				Far.Net.ShowError(Res.Me, ex);
			}
		}

		void AsyncInvoke(IAsyncResult ar)
		{
			// end; it may throw, e.g. on [CtrlC]
			try
			{
				PowerShell.EndInvoke(ar);
			}
			catch (RuntimeException)
			{ }

			// write failure
			if (PowerShell.InvocationStateInfo.State == PSInvocationState.Failed)
			{
				using (PowerShell ps = PowerShell.Create())
				{
					ps.Runspace = Runspace;
					A.OutReason(ps, PowerShell.InvocationStateInfo.Reason);
				}
			}

			// complete output
			{
				EditorOutputWriter1 writer = (EditorOutputWriter1)FarUI.PopWriter();
				if (writer.WriteCount > 0)
					Editor.Insert("=>\r");
				else
					Editor.InsertLine();

				Editor.EndAsync();
			}

			// kill
			PowerShell.Dispose();
		}

		void AsyncStop(IAsyncResult ar)
		{
			PowerShell.EndStop(ar);
		}

		int _initTabExpansion;
		public void ExpandCode(ILine editLine)
		{
			if (_initTabExpansion == 0)
			{
				_initTabExpansion = -1;
				string path = A.Psf.AppHome + @"\TabExpansion.ps1";
				if (!File.Exists(path))
					return;

				string code = File.ReadAllText(path);

				using (PowerShell shell = PowerShell.Create())
				{
					shell.Runspace = Runspace;
					shell.AddScript(code);
					shell.Invoke();
				}

				_initTabExpansion = +1;
			}

			// line and last word
			string text = editLine.Text;
			string line = text.Substring(0, editLine.Pos);
			Match match = Regex.Match(line, @"(?:^|\s)(\S+)$");
			if (!match.Success)
				return;

			text = text.Substring(line.Length);
			string lastWord = match.Groups[1].Value;

			// invoke
			Collection<PSObject> words;
			using (PowerShell shell = PowerShell.Create())
			{
				shell.Runspace = Runspace;
				shell.AddScript("TabExpansion $args[0] $args[1]");
				shell.AddParameters(new object[] { line, lastWord });
				words = shell.Invoke();
			}

			// complete expansion
			EditorKit.ExpandText(editLine, text, line, lastWord, words);
		}

	}
}
