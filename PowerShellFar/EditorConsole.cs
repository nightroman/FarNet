
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
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
		public IEditor Editor { get; private set; }
		FarUI FarUI;
		FarHost FarHost;
		Runspace Runspace;
		PowerShell PowerShell;
		static string GetFolderPath()
		{
			return A.Psf.Manager.GetFolderPath(SpecialFolder.LocalData, true);
		}
		static string GetFilePath()
		{
			return Path.Combine(GetFolderPath(), Kit.ToString(DateTime.Now, "_yyMMdd_HHmmss") + Word.ConsoleExtension);
		}
		/// <summary>
		/// Creates an editor console.
		/// </summary>
		/// <remarks>
		/// With prompt may return null if a user cancels.
		/// </remarks>
		public static EditorConsole CreateConsole(bool prompt)
		{
			int mode = 0;
			if (prompt)
			{
				IMenu menu = Far.Net.CreateMenu();
				menu.Title = "Open Editor Console";
				menu.Add("&1. Main session");
				menu.Add("&2. New local session");
				menu.Add("&3. New remote session");
				menu.HelpTopic = Far.Net.GetHelpTopic("EditorConsoleMenuOpen");

				if (!menu.Show())
					return null;

				mode = menu.Selected;
			}

			// editor
			IEditor editor = Far.Net.CreateEditor();
			editor.FileName = GetFilePath();
			editor.CodePage = Encoding.Unicode.CodePage;
			editor.DisableHistory = true;

			// create the console and attach it as the host to avoid conflicts
			EditorConsole r = new EditorConsole(editor, mode);
			editor.Host = r;
			return r;
		}
		public EditorConsole(IEditor editor) : this(editor, 0) { }
		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
		public EditorConsole(IEditor editor, int mode)
		{
			Editor = editor;
			Editor.KeyDown += OnKeyDown;

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

			Editor.Title = Editor.FileName;
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
		void RunspaceOpen()
		{
			Runspace.Open();
		}
		void OpenLocalSession()
		{
			EnsureHost();

			Runspace = RunspaceFactory.CreateRunspace(FarHost, Runspace.DefaultRunspace.RunspaceConfiguration);
			RunspaceOpen();

			Editor.Title = "Local session: " + Path.GetFileName(Editor.FileName);
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
			RunspaceOpen();

			Editor.Title = computerName + " session: " + Path.GetFileName(Editor.FileName);
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
		/// <summary>
		/// Called on key in psfconsole.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void OnKeyDown(object sender, KeyEventArgs e)
		{
			// drop pipeline now, if any
			PowerShell = null;

			// skip if selected
			if (Editor.SelectionExists)
				return;

			switch (e.Key.VirtualKeyCode)
			{
				case KeyCode.Enter:
					{
						if (e.Key.Is())
						{
							// [Enter]
							e.Ignore = true;
							Invoke();
						}
						return;
					}
				case KeyCode.Tab:
					{
						if (e.Key.Is())
						{
							// [Tab]
							if (IsLastLineCurrent)
							{
								e.Ignore = true;
								if (Runspace == null)
									EditorKit.ExpandCode(Editor.Line);
								else
									ExpandCode(Editor.Line);

								Editor.Redraw();
							}
						}
						return;
					}
				case KeyCode.Escape:
					{
						if (e.Key.Is())
						{
							// [Esc]
							if (IsLastLineCurrent && Editor.Line.Length > 0)
							{
								e.Ignore = true;
								ILine line = Editor.Line;
								line.Text = string.Empty;
								line.Caret = 0;
								Editor.Redraw();
							}
						}
						return;
					}
				case KeyCode.End:
					{
						if (e.Key.Is())
						{
							// [End]
							if (!IsLastLineCurrent)
								return;

							ILine curr = Editor.Line;
							if (curr.Caret != curr.Length)
								return;

							string pref = curr.Text;
							UI.CommandHistoryMenu m = new UI.CommandHistoryMenu(pref);
							string code = m.Show();
							if (code == null)
								return;

							e.Ignore = true;
							curr.Text = code;
							curr.Caret = -1;
							Editor.Redraw();
						}
						return;
					}
				case KeyCode.UpArrow:
					goto case KeyCode.DownArrow;
				case KeyCode.DownArrow:
					{
						if (e.Key.Is())
						{
							// [Up], [Down]
							if (!IsLastLineCurrent)
								return;

							string lastUsedCmd = null;
							if (History.Cache == null)
							{
								// don't lose not empty line!
								if (Editor.Line.Length > 0)
									return;
								History.Cache = History.ReadLines();
								History.CacheIndex = History.Cache.Length;
							}
							else if (History.CacheIndex >= 0 && History.CacheIndex < History.Cache.Length)
							{
								lastUsedCmd = History.Cache[History.CacheIndex];
							}
							string code;
							if (e.Key.VirtualKeyCode == KeyCode.UpArrow)
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
							ILine curr = Editor.Line;
							curr.Text = code;
							curr.Caret = -1;
							Editor.Redraw();
						}
						return;
					}
				case KeyCode.Delete:
					{
						if (e.Key.Is())
						{
							// [Del]
							if (!IsLastLineCurrent)
								return;

							ILine curr = Editor.Line;
							if (curr.Length > 0)
								return;

							e.Ignore = true;

							Point pt = Editor.Caret;
							for (int i = pt.Y - 1; i >= 0; --i)
							{
								string text = Editor[i].Text;
								if (text == "<=")
								{
									Editor.SelectText(0, i, -1, pt.Y, PlaceKind.Stream);
									Editor.DeleteText();
									break;
								}
								if (text == "=>")
								{
									pt = new Point(-1, i + 1);
									continue;
								}
							}

							Editor.Redraw();
						}
						return;
					}
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
			ILine curr = Editor.Line;
			string code = curr.Text;
			if (code.Length == 0)
				return;

			// end?
			if (!IsLastLineCurrent)
			{
				// - no, copy code and exit
				Editor.GoToEnd(true);
				Editor.InsertText(code);
				Editor.Redraw();
				return;
			}

			// go end
			curr.Caret = -1;

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
			A.Psf.Act(code, writer, true);
			if (Editor != Far.Net.Editor)
			{
				Far.Net.Message(Res.EditorConsoleCannotComplete);
			}
			else
			{
				// complete output
				EndOutput(writer);
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
			EndOutput((EditorOutputWriter1)FarUI.PopWriter());
			Editor.EndAsync();

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
			string line = text.Substring(0, editLine.Caret);
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
		bool IsLastLineCurrent
		{
			get
			{
				return Editor.Caret.Y == Editor.Count - 1;
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void EndOutput(EditorOutputWriter1 writer)
		{
			// custom extra output
			var endCode = Settings.Default.EditorConsoleEndOutputScript;
			if (!string.IsNullOrEmpty(endCode))
			{
				try
				{
					using (PowerShell ps = PowerShell.Create())
					{
						ps.Runspace = Runspace;
						ps.AddScript(endCode);

						foreach (var it in ps.Invoke())
							if (it != null)
								writer.WriteLine(it.ToString());
					}
				}
				catch (Exception e)
				{
					writer.WriteErrorLine("EditorConsoleEndOutputScript: " + e.Message);
				}
			}

			// last line
			if (writer.WriteCount == 0)
				Editor.InsertLine();
			else
				Editor.InsertText("=>\r");
		}
	}
}
