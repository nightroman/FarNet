
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2011 Roman Kuzmin
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
		readonly Regex _regex = new Regex(@"ErrorActionPreference.*Stop:\s*(.*)");
		readonly IMenu _menu;
		public ErrorsMenu()
		{
			_menu = Far.Net.CreateMenu();
			_menu.Title = "PowerShell errors ($Error)";
			_menu.HelpTopic = Far.Net.GetHelpTopic("MenuErrors");
			_menu.BreakKeys.Add(VKeyCode.Delete);
			_menu.BreakKeys.Add(VKeyCode.F4);
		}
		string GetErrorMessage(string message)
		{
			Match m = _regex.Match(message);
			return m.Success ? m.Groups[1].Value : message;
		}
		public void Show()
		{
			ArrayList errors = A.Psf.Engine.SessionState.PSVariable.GetValue("Error") as ArrayList;
			foreach (object error in errors)
			{
				// exception:
				var asException = error as Exception;
				if (asException != null)
				{
					_menu.Add(GetErrorMessage(asException.Message)).Data = error;
					continue;
				}

				// record:
				var asRecord = error as ErrorRecord;
				if (asRecord != null)
				{
					var item = _menu.Add(GetErrorMessage(asRecord.ToString()));
					item.Data = error;

					// set checked an item with a source script
					//_110611_091139 InvocationInfo can be null.
					if (asRecord.InvocationInfo != null && !string.IsNullOrEmpty(asRecord.InvocationInfo.ScriptName) && File.Exists(asRecord.InvocationInfo.ScriptName))
						item.Checked = true;

					continue;
				}

				// others:
				_menu.Add(error == null ? string.Empty : error.ToString());
			}

			while (_menu.Show())
			{
				if (_menu.BreakKey == VKeyCode.Delete)
				{
					errors.Clear();
					return;
				}

				var error = _menu.SelectedData;

				var asException = error as Exception;
				if (asException != null)
				{
					if (_menu.BreakKey != 0)
						continue;

					Far.Net.ShowError(null, asException);
					continue;
				}

				var asRecord = error as ErrorRecord;
				if (asRecord != null)
				{
					if (_menu.BreakKey == VKeyCode.F4)
					{
						if (!string.IsNullOrEmpty(asRecord.InvocationInfo.ScriptName) && File.Exists(asRecord.InvocationInfo.ScriptName))
						{
							IEditor editor = Far.Net.CreateEditor();
							editor.FileName = asRecord.InvocationInfo.ScriptName;
							editor.GoTo(0, asRecord.InvocationInfo.ScriptLineNumber - 1);
							editor.Open(OpenMode.None);
							return;
						}
					}

					Far.Net.ShowError(null, asRecord.Exception);
					continue;
				}
			}
		}
	}
}
