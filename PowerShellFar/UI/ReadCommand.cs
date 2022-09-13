
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using FarNet.Forms;
using System;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;

namespace PowerShellFar.UI;

class ReadCommand
{
	static ReadCommand? Instance;

	readonly IDialog Dialog;
	readonly IText Text;
	readonly IEdit Edit;
	string? PromptTrimmed;
	string PromptOriginal;
	string? TextFromEditor;

	RunArgs? Out;

	class Layout
	{
		public int DialogLeft, DialogTop, DialogRight, DialogBottom;
		public int TextLeft, TextTop, TextRight;
		public int EditLeft, EditTop, EditRight;
	}

	public ReadCommand()
	{
		PromptOriginal = GetPrompt();
		Far.Api.UI.WindowTitle = PromptOriginal;
		var pos = GetLayoutAndSetPromptTrimmed(PromptOriginal);

		Dialog = Far.Api.CreateDialog(pos.DialogLeft, pos.DialogTop, pos.DialogRight, pos.DialogBottom);
		Dialog.TypeId = new(Guids.ReadCommandDialog);
		Dialog.NoShadow = true;
		Dialog.KeepWindowTitle = true;
		Dialog.Closing += Dialog_Closing;
		Dialog.GotFocus += Dialog_GotFocus;
		Dialog.ConsoleSizeChanged += Dialog_ConsoleSizeChanged;
		Dialog.MouseClicked += Events.MouseClicked_IgnoreOutside;

		Text = Dialog.AddText(pos.TextLeft, pos.TextTop, pos.TextRight, PromptTrimmed);
		Text.Coloring += Events.Coloring_TextAsConsole;

		Edit = Dialog.AddEdit(pos.EditLeft, pos.EditTop, pos.EditRight, string.Empty);
		Edit.IsPath = true;
		Edit.History = Res.History;
		Edit.KeyPressed += Edit_KeyPressed;
		Edit.Coloring += Events.Coloring_EditAsConsole;
	}

	Layout GetLayoutAndSetPromptTrimmed(string prompt)
	{
		var size = Far.Api.UI.WindowSize;
		int maxPromptLength = size.X / 3;

		PromptTrimmed = prompt;
		if (PromptTrimmed.Length > maxPromptLength)
		{
			int i1 = maxPromptLength / 2;
			int i2 = PromptTrimmed.Length - i1 - 1;
			PromptTrimmed = $"{PromptTrimmed[..i1]}\u2026{PromptTrimmed[i2..]}";
		}
		int pos = PromptTrimmed.Length;

		//! make Edit one cell wider to hide the arrow
		return new()
		{
			DialogLeft = 0,
			DialogTop = size.Y - 1,
			DialogRight = size.X - 1,
			DialogBottom = size.Y - 1,
			TextLeft = 0,
			TextTop = 0,
			TextRight = pos - 1,
			EditLeft = pos,
			EditTop = 0,
			EditRight = size.X - 1
		};
	}

	static string GetPrompt()
	{
		try
		{
			A.Psf.SyncPaths();

			using var ps = A.Psf.NewPowerShell();
			var res = ps.AddCommand("prompt").Invoke();

			//! as PS, use not empty res[0]
			string prompt;
			if (res.Count > 0 && res[0] != null && (prompt = res[0].ToString()).Length > 0)
				return prompt;
		}
		catch (RuntimeException)
		{
		}
		return "PS> ";
	}

	public static bool IsActive()
	{
		if (Instance is null)
			return false;

		var from = Far.Api.Window.Kind;
		if (from == WindowKind.Desktop)
			return true;

		return from == WindowKind.Dialog && Far.Api.Dialog.TypeId == new Guid(Guids.ReadCommandDialog);
	}

	public static void Stop()
	{
		Instance?.Dialog.Close(-2);
	}

	void Dialog_Closing(object? sender, ClosingEventArgs e)
	{
		// cancel
		if (e.Control is null)
			return;

		// get code, allow empty to refresh prompt
		bool fromEditor = TextFromEditor != null;
		var code = (fromEditor ? TextFromEditor! : Edit.Text).TrimEnd();
		bool toEcho = !fromEditor || code.IndexOf('\n') < 0;

		//! use original prompt (transcript, analysis, etc.)
		var echo = PromptOriginal + (toEcho ? code : "...");

		// result
		Out = new RunArgs(code) { Writer = new ConsoleOutputWriter(echo) };
	}

	bool _Dialog_GotFocus;
	void Dialog_GotFocus(object? sender, EventArgs e)
	{
		if (_Dialog_GotFocus)
			return;

		_Dialog_GotFocus = true;
		try
		{
			// ensure dialog over panels
			Far.Api.Window.SetCurrentAt(-1);
			Dialog.Activate();

			// refresh prompt and layout
			var prompt = GetPrompt();
			if (prompt != PromptOriginal)
			{
				PromptOriginal = prompt;
				Far.Api.UI.WindowTitle = prompt;

				var pos = GetLayoutAndSetPromptTrimmed(prompt);
				Text.Text = PromptTrimmed!;
				Text.Rect = new Place(pos.TextLeft, pos.TextTop, pos.TextRight, pos.TextTop);
				Edit.Rect = new Place(pos.EditLeft, pos.EditTop, pos.EditRight, pos.EditTop);
			}
		}
		finally
		{
			_Dialog_GotFocus = false;
		}
	}

	void Dialog_ConsoleSizeChanged(object? sender, SizeEventArgs e)
	{
		var pos = GetLayoutAndSetPromptTrimmed(PromptOriginal);
		Dialog.Rect = new Place(pos.DialogLeft, pos.DialogTop, pos.DialogRight, pos.DialogBottom);
		Text.Rect = new Place(pos.TextLeft, pos.TextTop, pos.TextRight, pos.TextTop);
		Edit.Rect = new Place(pos.EditLeft, pos.EditTop, pos.EditRight, pos.EditTop);
		Text.Text = PromptTrimmed!;
	}

	void Edit_KeyPressedCtrl(KeyPressedEventArgs e)
	{
		switch (e.Key.VirtualKeyCode)
		{
			case KeyCode.O:
			case KeyCode.F1:
			case KeyCode.F2:
				{
					// hide/show panels
					e.Ignore = true;
					RunKeyInPanelsAsync(e.Key, true);
				}
				return;
			case KeyCode.Enter:
				{
					// insert current file name
					e.Ignore = true;
					var file = Far.Api.Panel.CurrentFile;
					if (file != null)
						Edit.Line.InsertText(file.Name);
				}
				return;
			case KeyCode.F:
				{
					// insert current file path
					e.Ignore = true;
					var file = Far.Api.Panel.CurrentFile;
					if (file != null)
						Edit.Line.InsertText(Path.Combine(Far.Api.Panel.CurrentDirectory, file.Name));
				}
				return;
		}
	}

	void Edit_KeyPressed(object? sender, KeyPressedEventArgs e)
	{
		if (e.Key.IsCtrl())
		{
			Edit_KeyPressedCtrl(e);
			if (e.Ignore)
				return;
		}

		switch (e.Key.VirtualKeyCode)
		{
			case KeyCode.Escape:
				// clear text if not empty
				if (e.Key.Is() && Edit.Line.Length > 0)
				{
					e.Ignore = true;
					Edit.Text = string.Empty;
				}
				return;
			case KeyCode.Tab:
				// complete code
				if (e.Key.Is())
				{
					e.Ignore = true;
					if (Edit.Line.Length > 0)
						EditorKit.ExpandCode(Edit.Line, null);
					else
						RunKeyInPanelsAsync(e.Key, true);
				}
				return;
			case KeyCode.UpArrow:
			case KeyCode.DownArrow:
				// panel or history navigation
				if (e.Key.Is())
				{
					e.Ignore = true;
					if (Far.Api.Panel.IsVisible || Far.Api.Panel2.IsVisible)
						RunKeyInPanelsAsync(e.Key, true);
					else
						Edit.Text = History.GetNextCommand(e.Key.VirtualKeyCode == KeyCode.UpArrow, Edit.Text);
				}
				return;
			case KeyCode.E:
				// history navigation up
				if (e.Key.IsCtrl())
				{
					e.Ignore = true;
					Edit.Text = History.GetNextCommand(true, Edit.Text);
				}
				return;
			case KeyCode.X:
				// history navigation down, mind selected ~ CtrlX Cut
				if (e.Key.IsCtrl() && Edit.Line.SelectionSpan.Length < 0)
				{
					e.Ignore = true;
					Edit.Text = History.GetNextCommand(false, Edit.Text);
				}
				return;
			case KeyCode.F1:
				// show help
				if (e.Key.Is())
				{
					e.Ignore = true;
					Help.ShowHelpForText(Edit.Text, Edit.Line.Caret, HelpTopic.CommandConsole);
				}
				return;
			case KeyCode.F3:
				if (e.Key.Is())
				{
					// view panel file
					e.Ignore = true;
					Task.Run(async () =>
					{
						await RunKeyInPanelsAsync(e.Key, false);
						var viewer = Far.Api.Viewer;
						if (viewer != null)
						{
							await Tasks.Viewer(viewer);
							await ActivateAsync();
						}
					});
				}
				return;
			case KeyCode.F4:
				if (e.Key.Is())
				{
					// edit panel file
					e.Ignore = true;
					Task.Run(async () =>
					{
						await RunKeyInPanelsAsync(e.Key, false);
						var editor = Far.Api.Editor;
						if (editor != null)
						{
							await Tasks.Editor(editor);
							await ActivateAsync();
						}
					});
				}
				return;
			case KeyCode.F5:
				if (e.Key.Is())
				{
					// modal edit script
					e.Ignore = true;
					DoEditor();
				}
				return;
			case KeyCode.F2:
			case KeyCode.PageDown:
			case KeyCode.PageUp:
				// panel keys
				if (e.Key.Is())
				{
					e.Ignore = true;
					RunKeyInPanelsAsync(e.Key, true);
				}
				return;
		}
	}

	void DoEditor()
	{
		var args = new EditTextArgs
		{
			Text = Edit.Text,
			Extension = "ps1",
			Title = PromptOriginal,
			EditorOpened = (editor, _) => ((IEditor)editor!).GoTo(Edit.Line.Caret, 0)
		};
		TextFromEditor = Far.Api.AnyEditor.EditText(args);
		Dialog.Close();
	}

	Task RunKeyInPanelsAsync(KeyInfo key, bool activate)
	{
		var name = Far.Api.KeyInfoToName(key);
		return RunMacroInPanelsAsync($"Keys'{name}'", activate);
	}

	async Task RunMacroInPanelsAsync(string macro, bool activate)
	{
		await Tasks.Job(() => Far.Api.Window.SetCurrentAt(-1));
		await Tasks.Macro(macro);
		if (activate)
			await ActivateAsync();
	}

	public Task ActivateAsync()
	{
		return Tasks.Job(Dialog.Activate);
	}

	public Task<RunArgs?> ReadAsync()
	{
		return Task.Run(async () =>
		{
			await Tasks.Dialog(Dialog);
			History.ResetNavigation();
			return Out;
		});
	}

	public static async Task StartAsync()
	{
		try
		{
			// already started? activate
			if (Instance != null)
			{
				await Instance.ActivateAsync();
				return;
			}

			// must be panels
			if (Far.Api.Window.Kind != WindowKind.Panels)
				throw new ModuleException("Command console should start from panels.");

			// hide key bar (hack)
			bool visibleKeyBar = Console.CursorTop - Console.WindowTop == Console.WindowHeight - 2;
			if (visibleKeyBar)
				await Tasks.Macro("Keys'CtrlB'");

			try
			{
				// REPL
				for (; ; )
				{
					// read
					Instance = await Tasks.Job(() => new ReadCommand());
					var res = await Instance.ReadAsync();
					if (res is null)
						return;

					// run
					var obj = await Tasks.Command(() => A.Psf.Run(res));

					// switch to opened panel, come back on exit
					if (obj is Panel panel)
					{
						panel.Closed += (_, _) => _ = StartAsync();
						return;
					}
				}
			}
			finally
			{
				Instance = null;

				// restore key bar (may not work with jobs)
				if (visibleKeyBar)
					await Tasks.Macro("Keys'CtrlB'");
			}
		}
		catch (Exception ex)
		{
			_ = Tasks.Job(() => Far.Api.ShowError("Command console", ex));
		}
	}
}
