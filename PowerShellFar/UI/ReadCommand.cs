using FarNet;
using FarNet.Forms;
using System.Management.Automation;

namespace PowerShellFar.UI;
#pragma warning disable CA1416

internal class ReadCommand
{
	private const string ErrorCannotSetPanels = "Cannot set area 'Panels'.";
	private static readonly Guid MyTypeId = new(Guids.ReadCommandDialog);

	private static ReadCommand? Instance;

	private readonly IDialog Dialog;
	private readonly IText Text;
	private readonly IEdit Edit;
	private readonly string PromptOriginal;
	private string? PromptTrimmed;
	private string? TextFromEditor;
	private RunArgs? Out;

	private class Layout
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
		Dialog.TypeId = MyTypeId;
		Dialog.NoClickOutside = true;
		Dialog.NoShadow = true;
		Dialog.Closing += Dialog_Closing;
		Dialog.LosingFocus += OnLosingFocus;
		Dialog.ConsoleSizeChanged += OnConsoleSizeChanged;

		Text = Dialog.AddText(pos.TextLeft, pos.TextTop, pos.TextRight, PromptTrimmed);
		Text.Coloring += Events.Coloring_TextAsConsole;

		Edit = Dialog.AddEdit(pos.EditLeft, pos.EditTop, pos.EditRight, string.Empty);
		Edit.IsPath = true;
		Edit.History = Res.History;
		Edit.KeyPressed += OnKeyPressed;
		Edit.Coloring += Events.Coloring_EditAsConsole;

		// with no panels, set area Desktop to hide editor / viewer
		if (!Far.Api.HasPanels && Far.Api.Window.Kind != WindowKind.Desktop)
			Far.Api.Window.SetCurrentAt(0);
	}

	public static bool IsActive()
	{
		if (Instance is null)
			return false;

		var from = Far.Api.Window.Kind;
		if (from == WindowKind.Desktop)
			return true;

		return from == WindowKind.Dialog && Far.Api.Dialog!.TypeId == MyTypeId;
	}

	public static bool IsOpen()
	{
		return Far.Api.Window.Kind == WindowKind.Dialog && Far.Api.Dialog!.TypeId == MyTypeId;
	}

	public static void Stop()
	{
		Instance?.Dialog.Close(-2);
	}

	private Layout GetLayoutAndSetPromptTrimmed(string prompt)
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

	private static string GetPrompt()
	{
		try
		{
			using var ps = A.NewPowerShell();
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

	void Dialog_Closing(object? sender, ClosingEventArgs e)
	{
		// cancel
		if (e.Control is null)
			return;

		// get code, allow empty to refresh prompt
		bool fromEditor = TextFromEditor != null;
		var code = (fromEditor ? TextFromEditor! : Edit.Text).TrimEnd();

		string echo()
		{
			//! use original prompt (transcript, analysis, etc.)
			bool showCode = !fromEditor || code.IndexOf('\n') < 0;
			return PromptOriginal + (showCode ? code : "...");
		}

		// result
		Out = new RunArgs(code) { Writer = new ConsoleOutputWriter(echo), UseTeeResult = true };
	}

	private bool _Dialog_LosingFocus;
	private void OnLosingFocus(object? sender, EventArgs e)
	{
		if (_Dialog_LosingFocus)
			return;

		_Dialog_LosingFocus = true;

		_ = Tasks.ExecuteAndCatch(async () =>
		{
			await Tasks.Wait(50, 0, () =>
			{
				if (IsOpen())
					return true;

				if (Far.Api.Window.IsModal)
					return false;

				Stop();
				return true;
			});
		},
		null,
		() =>
		{
			_Dialog_LosingFocus = false;
		});
	}

	private void OnConsoleSizeChanged(object? sender, SizeEventArgs e)
	{
		var pos = GetLayoutAndSetPromptTrimmed(PromptOriginal);
		Dialog.Rect = new Place(pos.DialogLeft, pos.DialogTop, pos.DialogRight, pos.DialogBottom);
		Text.Rect = new Place(pos.TextLeft, pos.TextTop, pos.TextRight, pos.TextTop);
		Edit.Rect = new Place(pos.EditLeft, pos.EditTop, pos.EditRight, pos.EditTop);
		Text.Text = PromptTrimmed!;
	}

	private static void DoScroll(KeyPressedEventArgs e)
	{
		var (up, size) = e.Key.VirtualKeyCode switch
		{
			KeyCode.PageUp => (true, Console.WindowHeight - 1),
			KeyCode.PageDown => (false, Console.WindowHeight - 1),
			KeyCode.UpArrow => (true, 1),
			KeyCode.DownArrow => (false, 1),
			KeyCode.Home => (true, 1234567),
			KeyCode.End => (false, 1234567),
			_ => (true, 0)
		};

		int top = Console.WindowTop;
		if (up)
			top = Math.Max(0, top - size);
		else
			top = Math.Min(Console.BufferHeight - Console.WindowHeight, top + size);

		Console.SetWindowPosition(0, top);
	}

	private void OnKeyPressed(object? sender, KeyPressedEventArgs e)
	{
		switch (e.Key.VirtualKeyCode)
		{
			case KeyCode.PageUp when e.Key.Is():
			case KeyCode.PageDown when e.Key.Is():
			case KeyCode.UpArrow when e.Key.IsShift():
			case KeyCode.DownArrow when e.Key.IsShift():
			case KeyCode.Home when e.Key.IsCtrl():
			case KeyCode.End when e.Key.IsCtrl():
				e.Ignore = true;
				DoScroll(e);
				return;

			case KeyCode.Escape when e.Key.Is() && Edit.Line.Length > 0:
				// clear text if not empty
				e.Ignore = true;
				Edit.Text = string.Empty;
				return;

			case KeyCode.Tab when e.Key.Is():
				// complete code
				e.Ignore = true;
				EditorKit.ExpandCode(Edit.Line, null);
				return;

			case KeyCode.UpArrow when e.Key.Is():
			case KeyCode.E when e.Key.IsCtrl():
				// history navigation up
				e.Ignore = true;
				Edit.Text = HistoryKit.GetNextCommand(true, Edit.Text);
				return;

			case KeyCode.DownArrow when e.Key.Is():
			case KeyCode.X when e.Key.IsCtrl() && Edit.Line.SelectionSpan.Length < 0:
				// history navigation down, mind selected ~ CtrlX Cut
				e.Ignore = true;
				Edit.Text = HistoryKit.GetNextCommand(false, Edit.Text);
				return;

			case KeyCode.F1 when e.Key.Is():
				// show help
				e.Ignore = true;
				Help.ShowHelpForText(Edit.Text, Edit.Line.Caret, HelpTopic.CommandConsole);
				return;

			case KeyCode.F4 when e.Key.Is():
			case KeyCode.F5 when e.Key.Is():
				// modal edit script
				e.Ignore = true;
				DoEditor();
				return;

			case KeyCode.Enter when e.Key.IsCtrl():
				// insert current file name
				if (Far.Api.HasPanels)
				{
					e.Ignore = true;
					if (Far.Api.Panel!.CurrentFile is { } file)
						Edit.Line.InsertText(file.Name);
				}
				return;

			case KeyCode.F when e.Key.IsCtrl() && Far.Api.HasPanels:
				// insert current file path
				if (Far.Api.HasPanels)
				{
					e.Ignore = true;
					if (Far.Api.Panel!.CurrentFile is { } file)
						Edit.Line.InsertText(Path.Combine(Far.Api.CurrentDirectory, file.Name));
				}
				return;
		}
	}

	private void DoEditor()
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

	private Task<RunArgs?> ReadAsync()
	{
		return Task.Run(async () =>
		{
			await Tasks.Dialog(Dialog);
			HistoryKit.ResetNavigation();
			return Out;
		});
	}

	private static async Task DoFinallyAsync(WindowKind area1, bool visibleKeyBar, int visiblePanels)
	{
		// close running
		if (Instance is { })
		{
			await Tasks.Job(() =>
			{
				Instance?.Dialog.Close(-2);

				// with no panels save user screen printed during this REPL
				if (!Far.Api.HasPanels && Far.Api.Window.Kind == WindowKind.Desktop)
					Far.Api.UI.SaveUserScreen();
			});
			Instance = null;
		}

		// set panels
		var area2 = Far.Api.Window.Kind;
		if (area2 != WindowKind.Panels && Far.Api.HasPanels)
		{
			try
			{
				await Tasks.Job(() => Far.Api.Window.SetCurrentAt(-1));
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(ErrorCannotSetPanels, ex);
			}
		}

		// restore key bar (may not work with jobs)
		if (visibleKeyBar)
			await Tasks.Macro("Keys'CtrlB'");

		// show panels
		if (visiblePanels > 0)
		{
			int visiblePanels2 = Far.Api.Window.CountVisiblePanels();
			if (visiblePanels != visiblePanels2)
			{
				if (visiblePanels == 2 && visiblePanels2 == 0)
				{
					await Tasks.Macro("Keys'CtrlO'");
				}

				if (visiblePanels == 2 && visiblePanels2 == 1)
				{
					if (Far.Api.Panel!.IsLeft)
						await Tasks.Macro("Keys'CtrlF2'");
					else
						await Tasks.Macro("Keys'CtrlF1'");
				}

				if (visiblePanels == 1 && visiblePanels2 == 0)
				{
					if (Far.Api.Panel!.IsLeft)
						await Tasks.Macro("Keys'CtrlF1'");
					else
						await Tasks.Macro("Keys'CtrlF2'");
				}
			}
		}

		if (area2 != area1)
		{
			try
			{
				await Tasks.Job(() => Far.Api.Window.SetCurrentAt(Far.Api.Window.Count - 2));
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Cannot set area '{area1}'.", ex);
			}
		}

		// last, post quit
		if (Environment.GetEnvironmentVariable("FAR_START_COMMAND") is string cmd && cmd.Contains(nameof(Actor.StartCommandConsole)))
			Far.Api.PostJob(Far.Api.Quit);
	}

	public static async Task StartAsync()
	{
		if (Instance is { })
			return;

		try
		{
			// sync part

			var area1 = Far.Api.Window.Kind;
			if (area1 != WindowKind.Panels && Far.Api.HasPanels)
			{
				try
				{
					Far.Api.Window.SetCurrentAt(-1);
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException(ErrorCannotSetPanels, ex);
				}
			}

			// hide key bar (hack)
			bool visibleKeyBar = Console.CursorTop - Console.WindowTop == Console.WindowHeight - 2;
			if (visibleKeyBar)
				await Tasks.Macro("Keys'CtrlB'");

			// hide panels
			int visiblePanels = Far.Api.Window.CountVisiblePanels();
			if (visiblePanels > 0)
				await Tasks.Macro("Keys'CtrlO'");

			A.Invoking();

			// async part

			// REPL
			try
			{
				for (bool run = true; run;)
				{
					// read
					Instance = await Tasks.Job(() => new ReadCommand());
					var args = await Instance.ReadAsync();
					if (args is null)
						return;

					// run
					var newPanel = await Tasks.Command(() => A.Run(args));
					if (newPanel is { })
					{
						Stop();
						run = false;
					}
				}
			}
			finally
			{
				await DoFinallyAsync(area1, visibleKeyBar, visiblePanels);
			}
		}
		catch (Exception ex)
		{
			_ = Tasks.Job(() => Far.Api.ShowError("Command console", ex));
		}
	}
}
