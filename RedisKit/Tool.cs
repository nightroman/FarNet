﻿using FarNet;
using RedisKit.Panels;

namespace RedisKit;

[ModuleTool(Name = Host.MyName, Options = ModuleToolOptions.Panels, Id = "06f748aa-e597-4ea8-933f-88fc89e450d9")]
public class Tool : ModuleTool
{
	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		IMenu menu = Far.Api.CreateMenu();
		menu.Title = Host.MyName;
		menu.HelpTopic = GetHelpTopic("menu");

		if (Far.Api.Panel is AbcPanel panel)
			menu.Add("Copy key to clipboard", (s, e) => CopyKey(panel));

		menu.Add("Configuration", (s, e) => SelectConfiguration());

		menu.Add("Help", (s, e) => Host.Instance.ShowHelpTopic(string.Empty));

		menu.Show();
	}

	public static string? SelectConfiguration()
	{
		var menu = Far.Api.CreateMenu();
		menu.Title = "Select configuration";

		var configurations = Settings.Default.GetData().Configurations;
		if (configurations.Length == 0)
			throw new ModuleException("Settings must have at least one configuration.");

		var workings = Workings.Default;
		var workingsData = workings.GetData();

		var pad1 = configurations.Select(x => x.Name.Length).Max();
		foreach (var it in configurations)
		{
			var item = new SetItem { Text = $"{it.Name.PadRight(pad1)} | {Environment.ExpandEnvironmentVariables(it.Text)}" };
			menu.Items.Add(item);
			if (it.Name.Equals(workingsData.Configuration, StringComparison.OrdinalIgnoreCase))
				item.Checked = true;
		}

		if (!menu.Show())
			return null;

		var configurationName = configurations[menu.Selected].Name;

		workingsData.Configuration = configurationName;
		workings.Save();

		return configurationName;
	}

	static void CopyKey(AbcPanel panel)
	{
		var file = panel.CurrentFile;
		switch (panel)
		{
			case KeysPanel:
				if (file is { })
				{
					switch (file.Data)
					{
						case Files.FileDataFolder folder:
							Far.Api.CopyToClipboard(folder.Prefix);
							break;
						case Files.FileDataKey key:
							Far.Api.CopyToClipboard((string)key.Key!);
							break;
					}
				}
				break;
			case HashPanel hashPanel:
				{
					Far.Api.CopyToClipboard((string)hashPanel.Explorer.Key!);
				}
				break;
			case ListPanel listPanel:
				{
					Far.Api.CopyToClipboard((string)listPanel.Explorer.Key!);
				}
				break;
			case SetPanel setPanel:
				{
					Far.Api.CopyToClipboard((string)setPanel.Explorer.Key!);
				}
				break;
		}
	}
}
