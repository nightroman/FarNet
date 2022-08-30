
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;

namespace PowerShellFar;

static class History
{
	static readonly HistoryCommands _history = new();

	/// <summary>
	/// Up/Down cache.
	/// </summary>
	static string[] _navCache;

	/// <summary>
	/// Up/Down current index.
	/// </summary>
	static int _navIndex;

	/// <summary>
	/// Removes navigation data.
	/// </summary>
	public static void ResetNavigation()
	{
		_navCache = null;
	}

	/// <summary>
	/// Gets history lines.
	/// </summary>
	public static string[] ReadLines()
	{
		return _history.ReadLines();
	}

	/// <summary>
	/// For Actor. Inserts code to known targets and returns null or returns the code.
	/// </summary>
	public static string ShowHistory()
	{
		var ui = new UI.CommandHistoryMenu(_history, string.Empty);
		string code = ui.Show();
		if (code == null)
			return null;

		// case: panels, preserve the prefix
		if (Far.Api.Window.Kind == WindowKind.Panels)
		{
			if (!_history.HasPrefix(code))
				code = Entry.CommandInvoke1.Prefix + ": " + code;

			Far.Api.CommandLine.Text = code;
			return null;
		}

		code = _history.RemovePrefix(code);
		switch (Far.Api.Window.Kind)
		{
			case WindowKind.Editor:
				var editor = Far.Api.Editor;
				if (editor.Host is not Interactive)
					break;
				editor.GoToEnd(true);
				editor.InsertText(code);
				editor.Redraw();
				return null;
			case WindowKind.Dialog:
				var dialog = Far.Api.Dialog;
				var typeId = dialog.TypeId;
				if (typeId != new Guid(Guids.ReadCommandDialog) && typeId != new Guid(Guids.InputDialog))
					break;
				var line = Far.Api.Line;
				if (line == null || line.IsReadOnly)
					break;
				line.Text = code;
				return null;
		}

		return code;
	}

	public static string GetNextCommand(bool up, string current)
	{
		string lastUsed = null;

		if (_navCache == null)
		{
			lastUsed = current;
			_navCache = ReadLines();
			_navIndex = _navCache.Length;
		}
		else if (_navIndex >= 0 && _navIndex < _navCache.Length)
		{
			lastUsed = _navCache[_navIndex];
		}

		if (up)
		{
			for (; ; )
			{
				if (--_navIndex < 0)
				{
					_navIndex = -1;
					return string.Empty;
				}
				else
				{
					var command = _navCache[_navIndex];
					if (command != lastUsed)
						return command;
				}
			}
		}
		else
		{
			for (; ; )
			{
				if (++_navIndex >= _navCache.Length)
				{
					_navIndex = _navCache.Length;
					return string.Empty;
				}
				else
				{
					var command = _navCache[_navIndex];
					if (command != lastUsed)
						return command;
				}
			}
		}
	}
}
