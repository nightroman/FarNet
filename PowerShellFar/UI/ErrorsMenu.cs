using FarNet;
using FarNet.Works;
using System.Collections;
using System.Management.Automation;

namespace PowerShellFar.UI;

class ErrorsMenu
{
	readonly IMenu _menu;

	public ErrorsMenu()
	{
		_menu = Far.Api.CreateMenu();
		_menu.Title = "PowerShell errors ($Error)";
		_menu.HelpTopic = Entry.Instance.GetHelpTopic(HelpTopic.ErrorsMenu);
		_menu.AddKey(KeyCode.Delete);
		_menu.AddKey(KeyCode.F4);
	}

	static string GetErrorMessage(string message)
	{
		var m = MyRegex.ErrorActionPreference().Match(message);
		var r = m.Success ? m.Groups[1].Value : message;
		return Regexes.TabsAndNewLines().Replace(r, " ");
	}

	public void Show()
	{
		var errors = (ArrayList)A.GetVariableValue("Error")!;
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
			Exception? exception = null;
			if (error is Exception ex1)
			{
				exception = ex1;
			}
			else if (error is ErrorRecord asRecord)
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
				exception = asRecord.Exception;
			}

			if (exception is { })
			{
				if (_menu.Key.VirtualKeyCode != 0)
					continue;

				Far.Api.ShowError(null, exception);
				return;
			}
		}
	}
}
