using FarNet;
using FarNet.Tools;

namespace PowerShellFar;

static class HistoryKit
{
	static readonly HistoryCommands _history = new();
	static HistoryNext? _next;

	/// <summary>
	/// Gets history lines.
	/// </summary>
	public static string[] ReadLines()
	{
		return _history.ReadLines();
	}

	/// <summary>
	/// Removes navigation data.
	/// </summary>
	public static void ResetNavigation()
	{
		_next = null;
	}

	public static string GetNextCommand(bool up, string current)
	{
		_next ??= new(_history.ReadLines(), current);
		return _next.GetNext(up, current);
	}

	/// <summary>
	/// Invokes or inserts code to known targets.
	/// </summary>
	public static void ShowHistory()
	{
		var ui = new UI.CommandHistoryMenu(_history, string.Empty);
		string? code = ui.Show();
		if (code is null)
			return;

		bool run = ui.Menu.Key.Is(KeyCode.Enter);

		// case: panels, preserve the prefix
		if (Far.Api.Window.Kind == WindowKind.Panels)
		{
			if (!HistoryCommands.HasPrefix(code))
				code = Entry.Prefix1 + ' ' + code;

			Far.Api.CommandLine.Text = code;

			if (run)
				Far.Api.PostMacro("Keys 'Enter'");

			return;
		}

		code = HistoryCommands.RemovePrefix(code);
		if (run)
		{
			A.Run(new RunArgs(code));
			return;
		}

		switch (Far.Api.Window.Kind)
		{
			case WindowKind.Editor:
				var editor = Far.Api.Editor!;
				if (editor.Host is not Interactive)
					break;
				editor.GoToEnd(true);
				editor.InsertText(code);
				editor.Redraw();
				return;
			case WindowKind.Dialog:
				var dialog = Far.Api.Dialog!;
				var typeId = dialog.TypeId;
				if (typeId != new Guid(Guids.ReadCommandDialog) && typeId != new Guid(Guids.InputDialog))
					break;
				var line = Far.Api.Line;
				if (line is null || line.IsReadOnly)
					break;
				line.Text = code;
				return;
		}

		Actor.InvokeInputCodePrivate(code);
	}
}
