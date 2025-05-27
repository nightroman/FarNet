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
	/// For Actor. Inserts code to known targets and returns null or returns the code.
	/// </summary>
	public static string? ShowHistory()
	{
		var ui = new UI.CommandHistoryMenu(_history, string.Empty);
		string? code = ui.Show();
		if (code is null)
			return null;

		// case: panels, preserve the prefix
		if (Far.Api.Window.Kind == WindowKind.Panels)
		{
			bool isEnterMode = Far.Api.CommandLine.Text2.StartsWith(Entry.PrefixEnterMode, StringComparison.OrdinalIgnoreCase);

			if (!HistoryCommands.HasPrefix(code))
			{
				code = (isEnterMode ? Entry.PrefixEnterMode : Entry.Prefix1) + " " + code;
			}
			else if (isEnterMode)
			{
				code = Entry.PrefixEnterMode + " " + HistoryCommands.RemovePrefix(code);
			}

			Far.Api.CommandLine.Text = code;
			return null;
		}

		code = HistoryCommands.RemovePrefix(code);
		switch (Far.Api.Window.Kind)
		{
			case WindowKind.Editor:
				var editor = Far.Api.Editor!;
				if (editor.Host is not Interactive)
					break;
				editor.GoToEnd(true);
				editor.InsertText(code);
				editor.Redraw();
				return null;
			case WindowKind.Dialog:
				var dialog = Far.Api.Dialog!;
				var typeId = dialog.TypeId;
				if (typeId != new Guid(Guids.ReadCommandDialog) && typeId != new Guid(Guids.InputDialog))
					break;
				var line = Far.Api.Line;
				if (line is null || line.IsReadOnly)
					break;
				line.Text = code;
				return null;
		}

		return code;
	}
}
