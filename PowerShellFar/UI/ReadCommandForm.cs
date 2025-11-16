using FarNet;
using FarNet.Forms;
using System.Management.Automation;

namespace PowerShellFar.UI;
#pragma warning disable CA1416

internal sealed class ReadCommandForm
{
	internal static readonly Guid MyTypeId = new(Guids.ReadCommandDialog);

	internal readonly IDialog Dialog;
	internal readonly IEdit Edit;
	private readonly IText Text;
	private string PromptOriginal;
	private string? PromptTrimmed;
	private string? TextFromEditor;
	private RunArgs? ArgsToRun;

	private readonly bool _isKeyBar = Far.Api.GetSetting(FarSetting.Screen, "KeyBar").ToString() == "1";

	private class Layout
	{
		public int DialogLeft, DialogTop, DialogRight, DialogBottom;
		public int TextLeft, TextTop, TextRight;
		public int EditLeft, EditTop, EditRight;
	}

	public ReadCommandForm()
	{
		PromptOriginal = GetPrompt();
		Far.Api.UI.WindowTitle = PromptOriginal;
		var pos = GetLayoutAndSetPromptTrimmed(PromptOriginal);

		Dialog = Far.Api.CreateDialog(pos.DialogLeft, pos.DialogTop, pos.DialogRight, pos.DialogBottom);
		Dialog.TypeId = MyTypeId;
		Dialog.NoClickOutside = true;
		Dialog.NoShadow = true;
		Dialog.Closing += OnClosing;
		Dialog.GotFocus += OnGotFocus;
		Dialog.LosingFocus += OnLosingFocus;
		Dialog.ConsoleSizeChanged += OnConsoleSizeChanged;

		Text = Dialog.AddText(pos.TextLeft, pos.TextTop, pos.TextRight, PromptTrimmed);
		Text.Coloring += Events.Coloring_TextAsConsole;

		Edit = Dialog.AddEdit(pos.EditLeft, pos.EditTop, pos.EditRight, string.Empty);
		Edit.IsPath = true;
		Edit.History = Res.History;
		Edit.Coloring += Events.Coloring_EditAsConsole;
	}

	public static bool IsOpen()
	{
		return Far.Api.Window.Kind == WindowKind.Dialog && Far.Api.Dialog!.TypeId == MyTypeId;
	}

	public void Close()
	{
		Dialog.Close(-2);
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
		int formY = size.Y - (_isKeyBar ? 2 : 1);
		return new()
		{
			DialogLeft = 0,
			DialogTop = formY,
			DialogRight = size.X - 1,
			DialogBottom = formY,
			TextLeft = 0,
			TextTop = 0,
			TextRight = pos - 1,
			EditLeft = pos,
			EditTop = 0,
			EditRight = size.X - 1
		};
	}

	//! Use not empty res[0], as PS does.
	private static bool _GetPromptCalled;
	private static string GetPrompt()
	{
		_GetPromptCalled = true;
		try
		{
			A.SyncPaths();

			var res = A.InvokeCode("prompt");

			string prompt;
			if (res.Count > 0 && res[0] is { } obj && (prompt = obj.ToString()).Length > 0)
				return prompt;
		}
		catch (RuntimeException)
		{
			_GetPromptCalled = false;
		}
		return "PS> ";
	}

	private void OnClosing(object? sender, ClosingEventArgs e)
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
		ArgsToRun = new RunArgs(code) { Writer = new ConsoleOutputWriter(echo), UseTeeResult = true };
	}

	// Why post? On `cd X` from user menu it gets focus before the panel changes dir.
	private bool _OnGotFocus;
	private void OnGotFocus(object? sender, EventArgs e)
	{
		if (_GetPromptCalled)
			return;

		if (!_OnGotFocus)
		{
			_OnGotFocus = true;
			return;
		}

		Far.Api.PostJob(() =>
		{
			var prompt = GetPrompt();
			if (prompt != PromptOriginal)
			{
				PromptOriginal = prompt;
				OnConsoleSizeChanged(null, null!);
			}
		});
	}

	private bool _OnLosingFocus;
	private void OnLosingFocus(object? sender, EventArgs e)
	{
		if (_GetPromptCalled)
			return;

		if (_OnLosingFocus)
			return;

		_OnLosingFocus = true;

		_ = Tasks.ExecuteAndCatch(async () =>
		{
			await Tasks.Wait(50, 0, () =>
			{
				if (IsOpen())
					return true;

				if (Far.Api.Window.IsModal)
					return false;

				Close();
				return true;
			});
		},
		null,
		() =>
		{
			_OnLosingFocus = false;
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

	// common tool?
	internal static void DoScroll(KeyPressedEventArgs e)
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

	internal void DoEditor()
	{
		for (var text = Edit.Text; ; text = TextFromEditor)
		{
			var args = new EditTextArgs
			{
				Text = text,
				Extension = "ps1",
				Title = PromptOriginal,
				EditorOpened = (editor, _) => ((IEditor)editor!).GoTo(Edit.Line.Caret, 0)
			};

			TextFromEditor = Far.Api.AnyEditor.EditText(args).TrimEnd();
			if (TextFromEditor.Length == 0)
				return;

			if (!TextFromEditor.Contains('\n'))
			{
				Edit.Text = TextFromEditor;
				return;
			}

			var message = $"""
				Invoke multiline code without keeping history?
				Yes: Invoke; No: Edit again; Cancel: Discard.

				{TextFromEditor}
				""";

			switch (Far.Api.Message(message, Res.TextCommandConsole, MessageOptions.YesNoCancel))
			{
				case 0:
					Dialog.Close();
					return;
				case 1:
					continue;
				default:
					return;
			}
		}
	}

	internal Task<RunArgs?> ReadAsync()
	{
		return Task.Run(async () =>
		{
			await Tasks.Dialog(Dialog);
			HistoryKit.ResetNavigation();
			return ArgsToRun;
		});
	}
}
