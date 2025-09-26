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
		const int w1 = 77;
		const int w2 = w1 - 10;
		int nItems = choices.Count + 1;

		List<string> lines = [];
		Works.Kit.FormatMessage(lines, message, w2, 12, Works.FormatMessageMode.NonWord);
		if (lines.Count == 1 && string.IsNullOrWhiteSpace(lines[0]))
			lines = [];

		const int yMessage1 = 2;
		int yBox1 = yMessage1 + lines.Count;
		int yBox2 = yBox1 + nItems + 1;
		int h1 = yBox2 + 4;
		int hMax = Math.Min(25, Far.Api.UI.WindowSize.Y) - 3;
		if (h1 > hMax)
		{
			int delta = h1 - hMax;
			h1 = hMax;
			yBox2 -= delta;
		}

		var dialog = Far.Api.CreateDialog(-1, -1, w1, h1);
		dialog.AddBox(3, 1, 0, 0, caption);

		for (int i = 0, y = yMessage1; i < lines.Count; ++i, ++y)
			dialog.AddText(5, y, 4 + w2, lines[i]);

		var box = dialog.AddListBox(5, yBox1, 71, yBox2, string.Empty);
		box.NoAmpersands = true;

		var buttonSelect = dialog.AddButton(0, yBox2 + 1, many ? "Select many" : "Select one");
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
				var text = choices[i].Label;
				if (result.Count == 0 && defaults.ContainsKey(i))
				{
					text += " (default)";
					if (!many)
						box.Selected = i;
				}
				var item = new SetItem { Text = text };
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
