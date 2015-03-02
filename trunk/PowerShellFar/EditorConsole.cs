
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security.Permissions;
using System.Text;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Editor console.
	/// </summary>
	class EditorConsole
	{
		const string OutputMark1 = "<=";
		const string OutputMark2 = "=>";
		const string OutputMark3 = "<>";

		public IEditor Editor { get; private set; }
		FarUI FarUI;
		FarHost FarHost;
		Runspace Runspace;
		PowerShell PowerShell;
		bool _doneTabExpansion;

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
			if (Far.Api.UI.IsCommandMode)
			{
				A.Message("Cannot start editor console from command console.");
				return null;
			}

			int mode = 0;
			if (prompt)
			{
				IMenu menu = Far.Api.CreateMenu();
				menu.Title = "Open Editor Console";
				menu.Add("&1. Main session");
				menu.Add("&2. New local session");
				menu.Add("&3. New remote session");
				menu.HelpTopic = Far.Api.GetHelpTopic("EditorConsoleMenuOpen");

				if (!menu.Show())
					return null;

				mode = menu.Selected;
			}

			// editor
			IEditor editor = Far.Api.CreateEditor();
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
				case 0:
					OpenMainSession();
					break;
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
		void OpenMainSession()
		{
			Editor.Title = "Main session: " + Path.GetFileName(Editor.FileName);
		}
		void OpenLocalSession()
		{
			EnsureHost();

			Runspace = RunspaceFactory.CreateRunspace(FarHost, Runspace.DefaultRunspace.InitialSessionState);
			RunspaceOpen();

			Editor.Title = "Local session: " + Path.GetFileName(Editor.FileName);

			InvokeProfile("Profile-Local.ps1");
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

			InvokeProfile("Profile-Remote.ps1");
		}
		void InvokeProfile(string fileName)
		{
			var profile = Path.Combine(A.Psf.Manager.GetFolderPath(SpecialFolder.RoamingData, true), fileName);
			if (!File.Exists(profile))
				return;

			try
			{
				using (var ps = PowerShell.Create())
				{
					ps.Runspace = Runspace;
					ps.AddCommand(profile, false).Invoke();
				}
			}
			catch (RuntimeException ex)
			{
				Far.Api.Message(
					string.Format(null, "Error in {0}, see $Error for defails. Message: {1}", fileName, ex.Message),
					Res.Me, MessageOptions.Warning | MessageOptions.LeftAligned);
			}
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
		void InitTabExpansion()
		{
			if (!_doneTabExpansion)
			{
				_doneTabExpansion = true;
				EditorKit.InitTabExpansion(Runspace);
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

			// current line
			var currentLine = Editor.Line;

			switch (e.Key.VirtualKeyCode)
			{
				case KeyCode.Enter:
					{
						if (e.Key.Is())
						{
							// invoke, copy, or pass
							e.Ignore = Invoke();
						}
						else if (e.Key.IsShift())
						{
							// similar to ISE
							e.Ignore = true;
							Editor.InsertLine();
							Editor.Redraw();
						}
						return;
					}
				case KeyCode.Tab:
					{
						if (e.Key.Is())
						{
							if (GetCommandArea() != null && EditorKit.NeedsTabExpansion(Editor))
							{
								e.Ignore = true;
								InitTabExpansion();
								EditorKit.ExpandCode(currentLine, Runspace);
								Editor.Redraw();
							}
						}
						return;
					}
				case KeyCode.Escape:
					{
						if (e.Key.Is())
						{
							if (IsLastLineCurrent && currentLine.Length > 0)
							{
								e.Ignore = true;
								currentLine.Text = string.Empty;
								currentLine.Caret = 0;
								Editor.Redraw();
							}
						}
						return;
					}
				case KeyCode.End:
					{
						if (e.Key.Is())
						{
							if (!IsLastLineCurrent)
								return;

							if (currentLine.Caret != currentLine.Length)
								return;

							UI.CommandHistoryMenu m = new UI.CommandHistoryMenu(currentLine.Text);
							string code = m.Show();
							if (code == null)
								return;

							e.Ignore = true;
							currentLine.Text = code;
							currentLine.Caret = -1;
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
							if (!IsLastLineCurrent)
								return;

							var command = History.GetNextCommand(e.Key.VirtualKeyCode == KeyCode.UpArrow, currentLine.Text);
							
							e.Ignore = true;
							currentLine.Text = command;
							currentLine.Caret = -1;
							Editor.Redraw();
						}
						return;
					}
				case KeyCode.Delete:
					{
						if (e.Key.Is())
						{
							if (!IsLastLineCurrent)
								return;

							if (currentLine.Length > 0)
								return;

							e.Ignore = true;

							Point pt = Editor.Caret;
							for (int i = pt.Y - 1; i >= 0; --i)
							{
								string text = Editor[i].Text;
								if (text == OutputMark1)
								{
									Editor.SelectText(0, i, -1, pt.Y, PlaceKind.Stream);
									Editor.DeleteText();
									Editor.GoTo(0, i);
									Editor.InsertText(OutputMark3 + "\r");
									Editor.GoToEnd(false);
									break;
								}
								if (text == OutputMark2 || text == OutputMark3)
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
		internal class Area
		{
			public int FirstLineIndex;
			public int LastLineIndex;
			public Point Caret;
			public bool Active;
		}
		internal Area GetCommandArea()
		{
			var r = new Area();
			r.Caret = Editor.Caret;

			// first line
			for (int y = r.Caret.Y; --y >= 0; )
			{
				var text = Editor[y].Text;
				if (text == OutputMark2 || text == OutputMark3)
				{
					r.FirstLineIndex = y + 1;
					break;
				}

				if (text == OutputMark1)
					return null;
			}

			// last line
			r.LastLineIndex = Editor.Count - 1;
			for (int y = r.Caret.Y; ++y <= r.LastLineIndex; )
			{
				var text = Editor[y].Text;
				if (text == OutputMark1 || text == OutputMark3)
				{
					r.LastLineIndex = y - 1;
					return r;
				}

				if (text == OutputMark2)
					return null;
			}

			r.Active = true;
			return r;
		}
		internal bool Invoke()
		{
			var area = GetCommandArea();
			if (area == null)
				return false;

			// script, skip empty
			var sb = new StringBuilder();
			for (int y = area.FirstLineIndex; y < area.LastLineIndex; ++y)
				sb.AppendLine(Editor[y].Text);
			var lastText = Editor[area.LastLineIndex].Text;
			sb.Append(lastText);

			string code = sb.ToString();
			if (code.Length == 0)
				return true;
			if (code == OutputMark3)
				return false;

			// copy to the end
			if (!area.Active)
			{
				Editor.GoToEnd(true);
				Editor.BeginUndo();
				Editor.InsertText(code);
				Editor.EndUndo();
				Editor.Redraw();
				return true;
			}

			// history
			bool addHistory = area.FirstLineIndex == area.LastLineIndex;

			// go end
			Editor.GoToEnd(false);

			// go async
			if (Runspace != null)
			{
				InvokePipeline(code, addHistory);
				return true;
			}

			// invoke
			EditorOutputWriter2 writer = new EditorOutputWriter2(Editor);
			Editor.BeginUndo();

			// default runspace
			A.Psf.Act(code, writer, addHistory);
			if (Editor != Far.Api.Editor)
			{
				Far.Api.Message(Res.EditorConsoleCannotComplete);
			}
			else
			{
				// complete output
				EndOutput(writer);
				Editor.EndUndo();
				Editor.Redraw();
			}

			return true;
		}
		void InvokePipeline(string code, bool addHistory)
		{
			// drop history cache
			History.Cache = null;

			// push writer
			FarUI.PushWriter(new EditorOutputWriter1(Editor));

			// invoke
			try
			{
				// history
				if (addHistory)
				{
					code = code.Trim();
					if (code.Length > 0 && code[code.Length - 1] != '#' && A.Psf._myLastCommand != code)
					{
						History.AddLine(code);
						A.Psf._myLastCommand = code;
					}
				}

				// invoke command
				PowerShell = PowerShell.Create();
				PowerShell.Runspace = Runspace;
				PowerShell.Commands
					.AddScript(code)
					.AddCommand(A.OutHostCommand);

				Editor.BeginAsync();
				PowerShell.BeginInvoke<PSObject>(null, null, AsyncInvoke, null);
			}
			catch (RuntimeException ex)
			{
				Far.Api.ShowError(Res.Me, ex);
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
			if (writer.WriteCount > 0)
				Editor.InsertText(OutputMark2 + "\r");
			else
				Editor.InsertText("\r" + OutputMark3 + "\r");
		}
	}
}
