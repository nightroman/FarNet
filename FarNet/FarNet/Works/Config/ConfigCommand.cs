namespace FarNet.Works;
#pragma warning disable 1591

public static class ConfigCommand
{
	const string HelpTopic = "configure-commands";

	public static void Show(List<IModuleCommand> commands)
	{
		var menu = Far.Api.CreateMenu();
		menu.AutoAssignHotkeys = true;
		menu.HelpTopic = HelpTopic;
		menu.Title = "Commands (prefix, name, module)";

		for (; ; )
		{
			int max1 = commands.Max(x => x.Prefix.Length);
			int max2 = commands.Max(x => x.Name.Length);
			int max3 = commands.Max(x => x.Manager.ModuleName.Length);

			menu.Items.Clear();
			foreach (var it in commands)
				menu.Add($"{it.Prefix.PadRight(max1)} {it.Name.PadRight(max2)} {it.Manager.ModuleName.PadRight(max3)} {it.Id}").Data = it;

			if (!menu.Show())
				return;

			var command = (IModuleCommand)menu.SelectedData!;

			var ib = Far.Api.CreateInputBox();
			ib.EmptyEnabled = true;
			ib.HelpTopic = HelpTopic;
			ib.Prompt = "Prefix";
			ib.Text = command.Prefix;
			ib.Title = command.Name;

			string? prefix = null;
			while (ib.Show())
			{
				prefix = ib.Text;
				if (prefix.Contains(' ') || prefix.Contains(':'))
				{
					Far.Api.Message("Prefix must not contain ' ' or ':'.");
					prefix = null;
					continue;
				}
				break;
			}
			if (prefix == null)
				continue;

			command.Prefix = prefix;
			command.Manager.SaveConfig();
		}
	}
}
