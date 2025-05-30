
using FarNet;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace PowerShellFar.UI;

class DebuggerMenu
{
	readonly IListMenu _menu;
	IEditor? _editor;
	Collection<PSObject>? _breakpoints;
	bool _toStop;

	public DebuggerMenu()
	{
		_menu = Far.Api.CreateListMenu();
		_menu.Title = "PowerShell debugger tools";
		_menu.HelpTopic = Entry.Instance.GetHelpTopic(HelpTopic.DebuggerMenu);
		_menu.NoInfo = true;
		_menu.ScreenMargin = Settings.Default.ListMenuScreenMargin;
		_menu.UsualMargins = Settings.Default.ListMenuUsualMargins;

		_menu.AddKey(KeyCode.Delete, ControlKeyStates.None, OnDelete);
		_menu.AddKey(KeyCode.Delete, ControlKeyStates.ShiftPressed, OnDeleteAll);
		_menu.AddKey(KeyCode.F4, ControlKeyStates.None, OnEdit);
		_menu.AddKey(KeyCode.Backspace, ControlKeyStates.ShiftPressed, OnDisableAll);
		_menu.AddKey(KeyCode.Spacebar, ControlKeyStates.None, OnToggle);
	}

	public void Show()
	{
		// active editor
		if (Far.Api.Window.Kind == WindowKind.Editor)
			_editor = Far.Api.Editor;

		// menu loop
		for (_toStop = false; ; _menu.Items.Clear())
		{
			// new breakpoint by types
			_menu.Add("&Line breakpoint...", OnLineBreakpoint);
			_menu.Add("&Command breakpoint...", OnCommandBreakpoint);
			_menu.Add("&Variable breakpoint...", OnVariableBreakpoint);

			// breakpoint collection
			_breakpoints = A.InvokeCode("Get-PSBreakpoint");
			if (_breakpoints.Count > 0)
			{
				// separator
				_menu.Add("Breakpoints").IsSeparator = true;

				// breakpoints
				int n = 0;
				foreach (PSObject o in _breakpoints)
				{
					Breakpoint bp = (Breakpoint)o.BaseObject;

					++n;
					string text = bp.ToString()!;
					if (n <= 9)
						text = "&" + Kit.ToString(n) + " " + text;

					FarItem mi = _menu.Add(text);
					mi.Checked = bp.Enabled;
					mi.Data = bp;
				}
			}

			// go
			if (!_menu.Show() || _toStop)
				return;
		}
	}

	void OnLineBreakpoint(object? sender, EventArgs e)
	{
		if (_menu.Key.VirtualKeyCode == 0)
			DebuggerKit.OnLineBreakpoint(_editor, _breakpoints!);
	}

	void OnCommandBreakpoint(object? sender, EventArgs e)
	{
		if (_menu.Key.VirtualKeyCode != 0)
			return;

		string? file = null;
		if (_editor != null)
			file = _editor.FileName;

		var ui = new BreakpointDialog(1, file, 0);
		if (!ui.Show())
			return;

		string code = "Set-PSBreakpoint -Command $args[0]";
		if (ui.Script!.Length > 0)
			code += " -Script $args[1]";
		if (ui.Action != null)
			code += " -Action $args[2]";
		A.InvokeCode(code, ui.Matter, ui.Script, ui.Action);
	}

	void OnVariableBreakpoint(object? sender, EventArgs e)
	{
		if (_menu.Key.VirtualKeyCode != 0)
			return;

		string? file = null;
		if (_editor != null)
			file = _editor.FileName;

		var ui = new BreakpointDialog(2, file, 0);
		if (!ui.Show())
			return;

		string code = "Set-PSBreakpoint -Variable $args[0] -Mode $args[1]";
		if (ui.Script!.Length > 0)
			code += " -Script $args[2]";
		if (ui.Action != null)
			code += " -Action $args[3]";
		A.InvokeCode(code, ui.Matter, ui.Mode, ui.Script, ui.Action);
	}

	void OnDelete(object? sender, MenuEventArgs e)
	{
		if (_menu.SelectedData is not Breakpoint bp)
		{
			e.Ignore = true;
			return;
		}

		A.RemoveBreakpoint(bp);
	}

	void OnDeleteAll(object? sender, MenuEventArgs e)
	{
		if (_breakpoints!.Count > 0)
			A.RemoveBreakpoint(_breakpoints);
		else
			e.Ignore = true;
	}

	void OnDisableAll(object? sender, MenuEventArgs e)
	{
		if (_breakpoints!.Count > 0)
			A.DisableBreakpoint(_breakpoints);
		else
			e.Ignore = true;
	}

	void OnToggle(object? sender, MenuEventArgs e)
	{
		if (_menu.SelectedData is not Breakpoint bp)
		{
			e.Ignore = true;
			return;
		}

		if (bp.Enabled)
			A.DisableBreakpoint(bp);
		else
			A.InvokeCode("Enable-PSBreakpoint -Breakpoint $args[0]", bp);
	}

	void OnEdit(object? sender, MenuEventArgs e)
	{
		if (_menu.SelectedData is not Breakpoint bp || string.IsNullOrEmpty(bp.Script))
		{
			e.Ignore = true;
			return;
		}

		_toStop = true;
		var editor = _editor;
		var lbp = bp as LineBreakpoint;

		// the target script is opened now
		if (editor != null && Kit.Equals(editor.FileName, bp.Script))
		{
			// it is a line breakpoint, go to line
			if (lbp != null)
			{
				editor.GoToLine(lbp.Line - 1);
				return;
			}

			return;
		}

		editor = Far.Api.CreateEditor();
		editor.FileName = bp.Script;
		if (lbp != null)
			editor.GoToLine(lbp.Line - 1);

		editor.Open();
	}
}
