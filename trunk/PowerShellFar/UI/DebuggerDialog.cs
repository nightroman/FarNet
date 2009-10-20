/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System;
using System.IO;
using System.Management.Automation;
using System.Text;
using FarNet;
using FarNet.Forms;

namespace PowerShellFar.UI
{
	class DebuggerDialog
	{
		static DebuggerResumeAction _LastAction = DebuggerResumeAction.StepInto;
		InvocationInfo _InvocationInfo;
		IDialog _Dialog;
		IListBox _List1;
		IListBox _List2;
		IButton _Step;
		IButton _Over;
		IButton _Out;
		IButton _Console;
		IButton _Goto;
		IButton _Stack;

		public DebuggerDialog(DebuggerStopEventArgs e)
		{
			_InvocationInfo = e.InvocationInfo;
			int maxLine = 0;
			string[] lines = null;
			if (!string.IsNullOrEmpty(e.InvocationInfo.ScriptName) && File.Exists(e.InvocationInfo.ScriptName))
			{
				try 
				{
					lines = File.ReadAllLines(e.InvocationInfo.ScriptName, Encoding.Default);
					foreach (string s in lines)
					{
						if (s.Length > maxLine)
							maxLine = s.Length;
					}
				}
				catch (IOException) { }
			}

			int dw = Math.Max(Math.Min(Console.WindowWidth - 7, maxLine + 12), 73);
			int dh = 22;

			string title;
			int h1;
			if (e.Breakpoints.Count > 0)
			{
				title = "DEBUG: Hit breakpoint(s)";
				h1 = e.Breakpoints.Count + 2;
			}
			else
			{
				title = "DEBUG: Step";
				h1 = 2;
			}

			_Dialog = A.Far.CreateDialog(-1, -1, dw, dh);
			_Dialog.HelpTopic = A.Psf.HelpTopic + "DebuggerDialog";
			_Dialog.AddBox(3, 1, dw - 4, dh - 2, title);

			_List1 = _Dialog.AddListBox(4, 2, dw - 5, h1 + 1, null);
			_List1.Disabled = true;
			_List1.NoBox = true;
			_List1.NoClose = true;
			_List1.NoFocus = true;
			if (e.Breakpoints.Count > 0)
			{
				foreach (Breakpoint bp in e.Breakpoints)
					_List1.Add(bp.ToString());
			}
			foreach (string s in e.InvocationInfo.PositionMessage.Trim().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
				_List1.Add(s);

			_Dialog.AddText(0, -_List1.Rect.Height, 0, null).Separator = 1;

			_List2 = _Dialog.AddListBox(4, _List1.Rect.Bottom + 2, dw - 5, dh - 5, null);
			_List2.NoBox = true;
			_List2.NoClose = true;
			if (lines != null)
			{
				foreach (string s in lines)
					_List2.Add(s);
				int i = e.InvocationInfo.ScriptLineNumber - 1;
				_List2.Items[i].Checked = true;
			}

			_Dialog.AddText(0, -_List2.Rect.Height, 0, null).Separator = 1;

			_Step = _Dialog.AddButton(0, -1, "&Step");
			_Over = _Dialog.AddButton(0, 0, "O&ver");
			_Out = _Dialog.AddButton(0, 0, "&Out");
			_Console = _Dialog.AddButton(0, 0, "Conso&le..");
			_Goto = _Dialog.AddButton(0, 0, "&Goto..");
			_Stack = _Dialog.AddButton(0, 0, "Stac&k..");
			_Step.CenterGroup = _Over.CenterGroup = _Out.CenterGroup = _Console.CenterGroup = _Goto.CenterGroup = _Stack.CenterGroup = true;
			_Console.NoBrackets = _Goto.NoBrackets = _Stack.NoBrackets = true;

			_Dialog.Initialized += OnInitialized;
		}

		void SetFrame()
		{
			int i = _InvocationInfo.ScriptLineNumber - 1;
			_List2.SetFrame(i, i - _List2.Rect.Height / 2);
		}

		void OnInitialized(object sender, EventArgs e)
		{
			// set listbox frame
			if (_List2.Items.Count > 0)
				SetFrame();
		}

		public DebuggerResumeAction Show()
		{
			switch (_LastAction)
			{
				case DebuggerResumeAction.StepInto: _Dialog.Focused = _Step; break;
				case DebuggerResumeAction.StepOver: _Dialog.Focused = _Over; break;
			}

			while (_Dialog.Show())
			{
				if (_Dialog.Selected == _Step)
				{
					_LastAction = DebuggerResumeAction.StepInto;
					return DebuggerResumeAction.StepInto;
				}

				if (_Dialog.Selected == _Over)
				{
					_LastAction = DebuggerResumeAction.StepOver;
					return DebuggerResumeAction.StepOver;
				}

				if (_Dialog.Selected == _Out)
					return DebuggerResumeAction.StepOut;

				if (_Dialog.Selected == _Console)
				{
					A.Psf.ShowConsole(OpenMode.Modal);
					continue;
				}

				if (_Dialog.Selected == _Goto)
				{
					if (_List2.Items.Count > 0)
					{
						if (!_List2.Items[_List2.Selected].Checked)
						{
							SetFrame();
						}
						else
						{
							IEditor editor = A.Far.CreateEditor();
							editor.FileName = _InvocationInfo.ScriptName;
							editor.GoToLine(_InvocationInfo.ScriptLineNumber - 1);
							editor.Open(OpenMode.Modal);
						}
					}
					continue;
				}

				if (_Dialog.Selected == _Stack)
				{
					A.Psf.ShowCallStack();
					continue;
				}
			}

			return DebuggerResumeAction.Continue;
		}
	}
}
