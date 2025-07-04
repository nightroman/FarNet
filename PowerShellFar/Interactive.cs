
using FarNet;
using FarNet.Tools;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PowerShellFar;

/// <summary>
/// PowerShell interactive.
/// </summary>
class Interactive : InteractiveEditor
{
	FarUI? FarUI;
	FarHost? FarHost;
	Runspace? Runspace;
	PowerShell? PowerShell;
	readonly bool _isNestedPrompt;

	static readonly HistoryLog _history = new(Entry.LocalData + "\\InteractiveHistory.log", Settings.Default.MaximumHistoryCount);

	static string GetFilePath()
	{
		return Path.Join(Path.GetTempPath(), DateTime.Now.ToString("_yyMMdd_HHmmss") + Word.InteractiveSuffix);
	}

	/// <summary>
	/// Creates an interactive.
	/// </summary>
	public static Interactive Create(bool isNestedPrompt = false)
	{
		// editor
		var editor = Far.Api.CreateEditor();
		editor.FileName = GetFilePath();
		editor.CodePage = 65001;
		editor.DisableHistory = true;
		editor.Switching = Switching.Disabled;
		editor.DeleteSource = DeleteSource.File;

		// create interactive and attach it as the host to avoid conflicts
		Interactive interactive = new(editor, 0, isNestedPrompt) { AutoSave = true };
		editor.Host = interactive;
		return interactive;
	}

	public Interactive(IEditor editor) : this(editor, 0, false) { }

	public Interactive(IEditor editor, int mode, bool isNestedPrompt) : base(editor, _history, "<#<", ">#>", "<##>")
	{
		_isNestedPrompt = isNestedPrompt;

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

		if (isNestedPrompt)
			Editor.Opened += (_, _) => DoPrompt();
	}

	void DoPrompt()
	{
		var rs = Runspace ?? A.Psf.Runspace;

		using var ps = rs.CreateNestedPipeline("prompt", false);
		var res = ps.Invoke();

		var text = string.Join('\n', res.Select(x => x.ToString()));
		var lines = FarNet.Works.Kit.SplitLines(text);
		var n = lines.Length;

		while (n > 0)
		{
			var line = lines[n - 1].Trim();
			if (line.Length == 0 || line == ">")
				--n;
			else
				break;
		}

		if (n > 0)
		{
			for (int i = 0; i < n; ++i)
				Editor.Add($"# {lines[i]}");

			Editor.Save();
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
		if (FarHost is null)
		{
			Editor.Closed += delegate { CloseSession(); };
			Editor.CtrlCPressed += OnCtrlCPressed;
			FarUI = new FarUI();
			FarHost = new FarHost(FarUI);
		}
	}

	void RunspaceOpen()
	{
		Runspace!.Open();
	}

	void OpenMainSession()
	{
		Editor.Title = "PS main session " + Path.GetFileName(Editor.FileName);
	}

	void OpenLocalSession()
	{
		EnsureHost();

		Runspace = RunspaceFactory.CreateRunspace(FarHost, Runspace.DefaultRunspace.InitialSessionState);
		Runspace.ThreadOptions = PSThreadOptions.ReuseThread;
		RunspaceOpen();

		Editor.Title = "PS local session " + Path.GetFileName(Editor.FileName);

		InvokeProfile("Profile-Local.ps1", false);
	}

	void OpenRemoteSession()
	{
		UI.ConnectionDialog dialog = new("New remote interactive");
		if (!dialog.Show())
			return;

		string computerName = (dialog.ComputerName.Length == 0 || dialog.ComputerName == ".") ? "localhost" : dialog.ComputerName;
		PSCredential? credential = null;
		if (dialog.UserName.Length > 0)
		{
			credential = NativeMethods.PromptForCredential(null, null, dialog.UserName, string.Empty, PSCredentialTypes.Generic | PSCredentialTypes.Domain, PSCredentialUIOptions.Default);
			if (credential is null)
				return;
		}

		WSManConnectionInfo connectionInfo = new(false, computerName, 0, null, null, credential);

		EnsureHost();

		Runspace = RunspaceFactory.CreateRunspace(FarHost, connectionInfo);
		RunspaceOpen();

		Editor.Title = "PS " + computerName + " session " + Path.GetFileName(Editor.FileName);

		InvokeProfile("Profile-Remote.ps1", true);
	}

	void InvokeProfile(string fileName, bool remote)
	{
		var profile = Entry.RoamingData + "\\" + fileName;
		if (!File.Exists(profile))
			return;

		try
		{
			using var ps = PowerShell.Create();
			ps.Runspace = Runspace;
			if (remote)
				ps.AddScript(File.ReadAllText(profile), false);
			else
				ps.AddCommand(profile, false);
			ps.Invoke();
		}
		catch (RuntimeException ex)
		{
			Far.Api.Message(
				string.Format(null, "Error in {0}, see $Error for details. Message: {1}", fileName, ex.Message),
				Res.Me, MessageOptions.Warning | MessageOptions.LeftAligned);
		}
	}

	//! This method is sync and uses pipeline, that is why we must not null the pipeline async.
	void OnCtrlCPressed(object? sender, EventArgs e)
	{
		if (PowerShell != null && PowerShell.InvocationStateInfo.State == PSInvocationState.Running)
		{
			try
			{
				PowerShell.Stop();
			}
			catch (Exception ex)
			{
				Log.TraceException(ex);
			}
		}
	}

	/// <summary>
	/// Called on key in interactive.
	/// </summary>
	protected override bool KeyPressed(KeyInfo key)
	{
		ArgumentNullException.ThrowIfNull(key);

		// drop pipeline now, if any
		PowerShell = null;

		// current line
		var currentLine = Editor.Line;

		switch (key.VirtualKeyCode)
		{
			case KeyCode.Tab:
				{
					if (key.Is())
					{
						if (CommandArea() != null && EditorKit.NeedsTabExpansion(Editor))
						{
							EditorKit.ExpandCode(currentLine, Runspace);
							Editor.Redraw();
							return true;
						}
					}
					break;
				}
			case KeyCode.F1:
				{
					if (key.IsShift())
					{
						Help.ShowHelpForContext();
						return true;
					}
					break;
				}
		}
		return base.KeyPressed(key);
	}

	protected override bool IsAsync => Runspace != null;

	protected override void Invoke(string code, InteractiveArea area)
	{
		if (Runspace is null)
		{
			EditorOutputWriter2 writer = new(Editor);
			A.Psf.Run(new RunArgs(code) { Writer = writer });
			if (_isNestedPrompt)
				DoPrompt();
			return;
		}

		// begin editor
		FarUI!.PushWriter(new EditorOutputWriter3(Editor));

		// begin command
		PowerShell = PowerShell.Create();
		PowerShell.Runspace = Runspace;
		PowerShell.Commands.AddScript(code).AddCommand(A.OutHostCommand);
		_ = Task.Run(() =>
		{
			try
			{
				PowerShell.Invoke();
			}
			catch (Exception ex)
			{
				using var ps = PowerShell.Create();
				ps.Runspace = Runspace;
				A.OutReason(ps, ex);
			}

			// complete output
			FarUI.PopWriter();
			EndInvoke();

			// kill
			PowerShell.Dispose();
		});
	}
}
