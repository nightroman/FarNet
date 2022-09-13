
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections;
using System.IO;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace PowerShellFar.UI;

class ErrorsMenu
{
	readonly Regex _regexErrorActionPreference = new(@"ErrorActionPreference.*Stop:\s*(.*)");
	readonly Regex _regexNewLines = new(@"[\r\n\t]+");
	readonly IMenu _menu;

	public ErrorsMenu()
	{
		_menu = Far.Api.CreateMenu();
		_menu.Title = "PowerShell errors ($Error)";
		_menu.HelpTopic = Entry.Instance.GetHelpTopic(HelpTopic.ErrorsMenu);
		_menu.AddKey(KeyCode.Delete);
		_menu.AddKey(KeyCode.F4);
	}

	string GetErrorMessage(string message)
	{
		Match m = _regexErrorActionPreference.Match(message);
		var r = m.Success ? m.Groups[1].Value : message;
		return _regexNewLines.Replace(r, " ");
	}

	public void Show()
	{
		var errors = (ArrayList)A.Psf.Engine.SessionState.PSVariable.GetValue("Error")!;
		foreach (object error in errors)
		{
			// exception:
			if (error is Exception asException)
			{
				_menu.Add(GetErrorMessage(asException.Message)).Data = error;
				continue;
			}

			// record:
			if (error is ErrorRecord asRecord)
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
			_menu.Add(error is null ? string.Empty : error.ToString()!);
		}

		while (_menu.Show())
		{
			if (_menu.Key.Is(KeyCode.Delete))
			{
				errors.Clear();
				return;
			}

			var error = _menu.SelectedData;

			if (error is Exception asException)
			{
				if (_menu.Key.VirtualKeyCode != 0)
					continue;

				Far.Api.ShowError(null, asException);
				continue;
			}

			if (error is ErrorRecord asRecord)
			{
				if (_menu.Key.Is(KeyCode.F4))
				{
					if (!string.IsNullOrEmpty(asRecord.InvocationInfo.ScriptName) && File.Exists(asRecord.InvocationInfo.ScriptName))
					{
						IEditor editor = Far.Api.CreateEditor();
						editor.FileName = asRecord.InvocationInfo.ScriptName;
						editor.GoTo(0, asRecord.InvocationInfo.ScriptLineNumber - 1);
						editor.Open(OpenMode.None);
						return;
					}
				}

				Far.Api.ShowError(null, asRecord.Exception);
				continue;
			}
		}
	}
}
