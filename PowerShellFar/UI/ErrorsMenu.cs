
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
using System.IO;
using System.Management.Automation;
using System.Text.RegularExpressions;
using FarNet;

namespace PowerShellFar.UI
{
	class ErrorsMenu
	{
		IMenu _menu;

		public ErrorsMenu()
		{
			_menu = Far.Net.CreateMenu();
			_menu.Title = "PowerShell errors ($Error)";
			_menu.HelpTopic = A.Psf.HelpTopic + "MenuErrors";
			_menu.BreakKeys.Add(VKeyCode.Delete);
			_menu.BreakKeys.Add(VKeyCode.F4);
		}

		public void Show()
		{
			ArrayList errors = A.Psf.Engine.SessionState.PSVariable.GetValue("Error") as ArrayList;

			Regex re = new Regex(@"ErrorActionPreference.*Stop:\s*(.*)");
			foreach (object eo in errors)
			{
				Exception ex = eo as Exception;
				if (ex != null)
				{
					string message = ex.Message;
					Match m = re.Match(message);
					if (m.Success)
						message = m.Groups[1].Value;
					_menu.Add(message).Data = eo;
					continue;
				}

				ErrorRecord er = eo as ErrorRecord;
				if (er != null)
				{
					string message = er.ToString();
					Match m = re.Match(message);
					if (m.Success)
						message = m.Groups[1].Value;
					FarItem mi = _menu.Add(message);
					mi.Data = eo;
					if (!string.IsNullOrEmpty(er.InvocationInfo.ScriptName) && File.Exists(er.InvocationInfo.ScriptName))
						mi.Checked = true;
					continue;
				}
			}

			while (_menu.Show())
			{
				if (_menu.BreakKey == VKeyCode.Delete)
				{
					errors.Clear();
					return;
				}

				object eo = _menu.SelectedData;

				Exception ex = eo as Exception;
				if (ex != null)
				{
					if (_menu.BreakKey != 0)
						continue;
					Far.Net.ShowError(null, ex);
					continue;
				}

				ErrorRecord er = eo as ErrorRecord;
				if (er != null)
				{
					if (_menu.BreakKey == VKeyCode.F4)
					{
						if (!string.IsNullOrEmpty(er.InvocationInfo.ScriptName) && File.Exists(er.InvocationInfo.ScriptName))
						{
							IEditor editor = Far.Net.CreateEditor();
							editor.FileName = er.InvocationInfo.ScriptName;
							editor.GoTo(0, er.InvocationInfo.ScriptLineNumber - 1);
							editor.Open(OpenMode.None);
							return;
						}
					}

					Far.Net.ShowError(null, er.Exception);
					continue;
				}
			}
		}
	}
}
