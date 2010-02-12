/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.UI
{
	class DebuggerMenu
	{
		IListMenu _menu;
		IEditor _editor;
		Collection<PSObject> _breakpoints;
		bool _toStop;

		public DebuggerMenu()
		{
			_menu = Far.Host.CreateListMenu();
			_menu.Title = "PowerShell debugger tools";
			_menu.HelpTopic = A.Psf.HelpTopic + "MenuDebugger";
			_menu.FilterOptions = PatternOptions.None;
			_menu.NoInfo = true;
			_menu.ScreenMargin = A.Psf.Settings.ListMenuScreenMargin;
			_menu.UsualMargins = A.Psf.Settings.ListMenuUsualMargins;

			_menu.AddKey(KeyCode.Del, OnDelete);
			_menu.AddKey(KeyCode.Del | KeyMode.Shift, OnDeleteAll);
			_menu.AddKey(KeyCode.F4, OnEdit);
			_menu.AddKey(KeyCode.BS | KeyMode.Shift, OnDisableAll);
			_menu.AddKey(KeyCode.Space, OnToggle);
		}

		public void Show()
		{
			// active editor
			if (Far.Host.WindowType == WindowType.Editor)
				_editor = Far.Host.Editor;

			// menu loop
			for (_toStop = false; ; _menu.Items.Clear())
			{
				// new breakpoint by types
				_menu.Add("&Line breakpoint...", OnLineBreakpoint);
				_menu.Add("&Command breakpoint...", OnCommandBreakpoint);
				_menu.Add("&Variable breakpoint...", OnVariableBreakpoint);

				// breakpoint collection
				_breakpoints = A.Psf.InvokeCode("Get-PSBreakpoint");
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
						string text = bp.ToString();
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

		void OnLineBreakpoint(object sender, EventArgs e)
		{
			if (_menu.BreakKey != 0)
				return;

			string file = null;
			int line = 0;

			LineBreakpoint bpFound = null;
			if (_editor != null)
			{
				// location
				file = _editor.FileName;
				line = _editor.Cursor.Y + 1;

				// find
				foreach (PSObject o in _breakpoints)
				{
					LineBreakpoint lbp = o.BaseObject as LineBreakpoint;
					if (lbp != null && lbp.Action == null && line == lbp.Line && 0 == Kit.Compare(file, lbp.Script))
					{
						bpFound = lbp;
						break;
					}
				}

				// found?
				if (bpFound != null)
				{
					switch(Far.Host.Message("Breakpoint exists",
						"Line breakpoint",
						MsgOptions.None,
						new string[] {
							"&Remove",
							bpFound.Enabled ? "&Disable" : "&Enable",
							"&Modify",
							"&Add",
							"&Cancel"
						}))
					{
						case 0:
							A.Psf.InvokeCode("Remove-PSBreakpoint -Breakpoint $args[0]", bpFound);
							return;
						case 1:
							if (bpFound.Enabled)
								A.Psf.InvokeCode("Disable-PSBreakpoint -Breakpoint $args[0]", bpFound);
							else
								A.Psf.InvokeCode("Enable-PSBreakpoint -Breakpoint $args[0]", bpFound);
							return;
						case 2:
							break;
						case 3:
							bpFound = null;
							break;
						default:
							return;
					}
				}
			}

			// go
			BreakpointDialog ui = new BreakpointDialog(0, file, line);
			if (!ui.Show())
				return;

			// set new
			string code = "Set-PSBreakpoint -Script $args[0] -Line $args[1]";
			if (ui.Action != null)
				code += " -Action $args[2]";
			A.Psf.InvokeCode(code, ui.Script, ui.Matter, ui.Action);

			// remove the old
			if (bpFound != null)
				A.Psf.InvokeCode("Remove-PSBreakpoint -Breakpoint $args[0]", bpFound);
		}

		void OnCommandBreakpoint(object sender, EventArgs e)
		{
			if (_menu.BreakKey != 0)
				return;
			
			string file = null;
			if (_editor != null)
				file = _editor.FileName;

			BreakpointDialog ui = new BreakpointDialog(1, file, 0);
			if (!ui.Show())
				return;

			string code = "Set-PSBreakpoint -Command $args[0]";
			if (ui.Script.Length > 0)
				code += " -Script $args[1]";
			if (ui.Action != null)
				code += " -Action $args[2]";
			A.Psf.InvokeCode(code, ui.Matter, ui.Script, ui.Action);
		}

		void OnVariableBreakpoint(object sender, EventArgs e)
		{
			if (_menu.BreakKey != 0)
				return;

			string file = null;
			if (_editor != null)
				file = _editor.FileName;

			BreakpointDialog ui = new BreakpointDialog(2, file, 0);
			if (!ui.Show())
				return;

			string code = "Set-PSBreakpoint -Variable $args[0] -Mode $args[1]";
			if (ui.Script.Length > 0)
				code += " -Script $args[2]";
			if (ui.Action != null)
				code += " -Action $args[3]";
			A.Psf.InvokeCode(code, ui.Matter, ui.Mode, ui.Script, ui.Action);
		}

		void OnDelete(object sender, MenuEventArgs e)
		{
			Breakpoint bp = _menu.SelectedData as Breakpoint;
			if (bp == null)
			{
				e.Ignore = true;
				return;
			}

			A.Psf.InvokeCode("Remove-PSBreakpoint -Breakpoint $args[0]", bp);
		}

		void OnDeleteAll(object sender, MenuEventArgs e)
		{
			if (_breakpoints.Count > 0)
				A.Psf.InvokeCode("Remove-PSBreakpoint -Breakpoint $args[0]", _breakpoints);
			else
				e.Ignore = true;
		}

		void OnDisableAll(object sender, MenuEventArgs e)
		{
			if (_breakpoints.Count > 0)
				A.Psf.InvokeCode("Disable-PSBreakpoint -Breakpoint $args[0]", _breakpoints);
			else
				e.Ignore = true;
		}

		void OnToggle(object sender, MenuEventArgs e)
		{
			Breakpoint bp = _menu.SelectedData as Breakpoint;
			if (bp == null)
			{
				e.Ignore = true;
				return;
			}

			if (bp.Enabled)
				A.Psf.InvokeCode("Disable-PSBreakpoint -Breakpoint $args[0]", bp);
			else
				A.Psf.InvokeCode("Enable-PSBreakpoint -Breakpoint $args[0]", bp);
		}

		void OnEdit(object sender, MenuEventArgs e)
		{
			Breakpoint bp = _menu.SelectedData as Breakpoint;
			if (bp == null || string.IsNullOrEmpty(bp.Script))
			{
				e.Ignore = true;
				return;
			}

			_toStop = true;
			IEditor editor = _editor;
			LineBreakpoint lbp = bp as LineBreakpoint;

			// the target script is opened now
			if (editor != null && Kit.Compare(editor.FileName, bp.Script) == 0)
			{
				// it is a line breakpoint, go to line
				if (lbp != null)
				{
					editor.GoToLine(lbp.Line - 1);
					return;
				}

				return;
			}

			editor = Far.Host.CreateEditor();
			editor.FileName = bp.Script;
			if (lbp != null)
				editor.GoToLine(lbp.Line - 1);

			editor.Open();
		}

	}
}
