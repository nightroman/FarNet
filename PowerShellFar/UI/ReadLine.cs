
using FarNet;
using FarNet.Forms;

namespace PowerShellFar.UI;

class ReadLine
{
	public readonly Args In;
	public string Out => Edit.Text;

	readonly IDialog Dialog;
	readonly IText? Text;
	readonly IEdit Edit;

	public class Args
	{
		public string? Prompt;
		public string? History;
		public string? HelpMessage;
		public bool Password;
	}

	class Layout
	{
		public int DialogLeft, DialogTop, DialogRight, DialogBottom;
		public int TextLeft, TextTop, TextRight;
		public int EditLeft, EditTop, EditRight;
	}

	public ReadLine(Args args)
	{
		In = args;

		var prompt = args.Prompt ?? string.Empty;
		var pos = GetLayout();

		Dialog = Far.Api.CreateDialog(pos.DialogLeft, pos.DialogTop, pos.DialogRight, pos.DialogBottom);
		Dialog.TypeId = new Guid(Guids.ReadLineDialog);
		Dialog.NoShadow = true;
		Dialog.KeepWindowTitle = true;
		Dialog.ConsoleSizeChanged += Dialog_ConsoleSizeChanged;
		Dialog.MouseClicked += Events.MouseClicked_IgnoreOutside;

		if (prompt.Length > 0)
		{
			Text = Dialog.AddText(pos.TextLeft, pos.TextTop, pos.TextRight, prompt);
			Text.Coloring += Events.Coloring_TextAsConsole;
		}

		if (args.Password)
		{
			Edit = Dialog.AddEditPassword(pos.EditLeft, pos.EditTop, pos.EditRight, string.Empty);
		}
		else
		{
			Edit = Dialog.AddEdit(pos.EditLeft, pos.EditTop, pos.EditRight, string.Empty);
			Edit.History = args.History!;
			Edit.DropDownOpening += Edit_DropDownOpening;
			Edit.DropDownClosed += Edit_DropDownClosed;
		}
		Edit.Coloring += Events.Coloring_EditAsConsole;
		Edit.KeyPressed += Edit_KeyPressed;
	}

	/// <summary>
	/// Shows the console like read line UI, with ShowUserScreen and SaveUserScreen.
	/// </summary>
	// Why user screen. This is called by several methods of PSHostUserInterface,
	// in console mode. But the window may be panels if writing has not started.
	// If we do not hide panels the dialog is not prominent enough.
	public bool Show()
	{
		Far.Api.UI.ShowUserScreen2();
		try
		{
			return Dialog.Show();
		}
		finally
		{
			Far.Api.UI.SaveUserScreen2();
		}
	}

	Layout GetLayout()
	{
		var prompt = In.Prompt ?? string.Empty;
		var size = Far.Api.UI.WindowSize;

		//! make one cell wider to hide the arrow
		return new()
		{
			DialogLeft = 0,
			DialogTop = size.Y - 1,
			DialogRight = size.X - 1,
			DialogBottom = size.Y - 1,
			TextLeft = 0,
			TextTop = 0,
			TextRight = prompt.Length - 1,
			EditLeft = prompt.Length,
			EditTop = 0,
			EditRight = size.X - 1
		};
	}

	void Dialog_ConsoleSizeChanged(object? sender, SizeEventArgs e)
	{
		var pos = GetLayout();
		Dialog.Rect = new Place(pos.DialogLeft, pos.DialogTop, pos.DialogRight, pos.DialogBottom);
		Edit.Rect = new Place(pos.EditLeft, pos.EditTop, pos.EditRight, pos.EditTop);
		if (Text != null)
			Text.Rect = new Place(pos.TextLeft, pos.TextTop, pos.TextRight, pos.TextTop);
	}

	void Edit_DropDownOpening(object? sender, DropDownOpeningEventArgs e)
	{
		Far.Api.UI.ShowUserScreen();
	}

	void Edit_DropDownClosed(object? sender, DropDownClosedEventArgs e)
	{
		Far.Api.UI.SaveUserScreen();
	}

	void Edit_KeyPressed(object? sender, KeyPressedEventArgs e)
	{
		switch (e.Key.VirtualKeyCode)
		{
			case KeyCode.Escape:
				// clear the text or exit
				if (e.Key.Is() && Edit.Line.Length > 0)
				{
					e.Ignore = true;
					Edit.Text = string.Empty;
				}
				return;
			case KeyCode.F1:
				// show the help message
				if (e.Key.Is())
				{
					e.Ignore = true;
					if (!string.IsNullOrEmpty(In.HelpMessage))
						Far.Api.Message(In.HelpMessage);
				}
				return;
			case KeyCode.C when e.Key.IsCtrl() && Edit.Line.SelectionSpan.Length < 0:
				{
					// exit
					e.Ignore = true;
					Dialog.Close(-2);
				}
				return;
		}
	}
}
