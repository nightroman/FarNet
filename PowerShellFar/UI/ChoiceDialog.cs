using FarNet;
using System.Collections.ObjectModel;
using System.Management.Automation.Host;
using Works = FarNet.Works;

namespace PowerShellFar.UI;

static class ChoiceDialog
{
	public static Collection<int>? SelectMany(string caption, string message, Collection<ChoiceDescription> choices, Dictionary<int, bool> defaults)
	{
		return Select(true, caption, message, choices, defaults);
	}

	public static int SelectOne(string caption, string message, Collection<ChoiceDescription> choices, Dictionary<int, bool> defaults)
	{
		var r = Select(false, caption, message, choices, defaults);
		return r is null ? -1 : r[0];
	}

	private static void ShowChoiceHelp(Collection<ChoiceDescription> choices, string[,] hotkeysAndPlainLabels)
	{
		var text = FarUI.GetChoiceHelp(choices, hotkeysAndPlainLabels);
		Far.Api.AnyViewer.ViewText(text, "Help", OpenMode.Modal);
	}

	private static Collection<int>? Select(bool many, string caption, string message, Collection<ChoiceDescription> choices, Dictionary<int, bool> defaults)
	{
		int w1 = 77;
		int w2 = w1 - 10;
		int nItems = choices.Count + 1;

		List<string> lines = [];
		Works.Kit.FormatMessage(lines, message, w2, 12, Works.FormatMessageMode.NonWord);
		if (lines.Count == 1 && string.IsNullOrWhiteSpace(lines[0]))
			lines = [];

		var dialog = Far.Api.CreateDialog(-1, -1, w1, 8 + lines.Count + nItems);
		dialog.AddBox(3, 1, 0, 0, caption);

		for (int i = 0; i < lines.Count; ++i)
			dialog.AddText(5, -1, 4 + w2, lines[i]);

		var box = dialog.AddListBox(5, -1, 71, 1 + nItems, string.Empty);
		box.NoAmpersands = true;

		dialog.AddText(5, box.Rect.Bottom + 1, 0, string.Empty).Separator = 1;

		var buttonSelect = dialog.AddButton(0, -1, many ? "Select many" : "Select one");
		buttonSelect.CenterGroup = true;
		if (!many)
			dialog.Default = buttonSelect;

		var buttonCancel = dialog.AddButton(0, 0, Res.Cancel);
		buttonCancel.CenterGroup = true;

		dialog.KeyPressed += (s, e) =>
		{
			switch (e.Key.VirtualKeyCode)
			{
				case KeyCode.Enter when e.Key.IsCtrl():
					e.Ignore = true;
					dialog.Close(buttonSelect.Id);
					break;
				case KeyCode.F1:
					e.Ignore = true;
					Entry.Instance.ShowHelpTopic(HelpTopic.ChoiceDialog);
					break;
			}
		};

		FarUI.BuildHotkeysAndPlainLabels(choices, out string[,] hotkeysAndPlainLabels);

		Collection<int> result = [];
		while (true)
		{
			// populate box
			box.Items.Clear();
			// items
			for (int i = 0; i < choices.Count; ++i)
			{
				var defaultSuffix = many && result.Count == 0 && defaults.ContainsKey(i) ? " (default)" : string.Empty;
				var item = new SetItem { Text = $"{choices[i].Label}{defaultSuffix}" };
				box.Items.Add(item);
				item.Checked = many && result.Contains(i);
			}
			// help
			box.Items.Add(new SetItem { Text = "&? Help" });
			if (!many && defaults.Count > 0)
				box.Selected = defaults.Keys.First();

			// show
			if (!dialog.Show())
				return null;

			var selected = dialog.Selected;

			if (selected == buttonSelect)
			{
				if (many)
				{
					// nothing? add defaults
					if (result.Count == 0)
					{
						foreach (int key in defaults.Keys)
							result.Add(key);
					}
					return result;
				}

				if (box.Selected == choices.Count)
				{
					ShowChoiceHelp(choices, hotkeysAndPlainLabels);
					continue;
				}

				return [box.Selected];
			}

			if (selected == box)
			{
				if (box.Selected == choices.Count)
				{
					ShowChoiceHelp(choices, hotkeysAndPlainLabels);
				}
				else
				{
					if (result.Contains(box.Selected))
						result.Remove(box.Selected);
					else
						result.Add(box.Selected);
				}
				continue;
			}

			return null;
		}
	}
}
