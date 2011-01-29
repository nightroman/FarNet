/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System.Collections.Generic;

namespace FarNet.Works
{
	public static class ConfigCommand
	{
		public static void Show(IList<IModuleCommand> commands, string helpTopic)
		{
			if (commands == null)
				return;

			IMenu menu = Far.Net.CreateMenu();
			menu.AutoAssignHotkeys = true;
			menu.HelpTopic = helpTopic;
			menu.Title = Res.ModuleCommands;

			for (; ; )
			{
				int widthName = 9; // Name
				int widthPref = 6; // Prefix
				foreach (IModuleCommand it in commands)
				{
					if (widthName < it.Name.Length)
						widthName = it.Name.Length;
					if (widthPref < it.Prefix.Length)
						widthPref = it.Prefix.Length;
				}
				string format = "{0,-" + widthPref + "} : {1,-" + widthName + "} : {2}";

				menu.Items.Clear();
				menu.Add(string.Format(null, format, "Prefix", "Name", "Address")).Disabled = true;
				foreach (IModuleCommand it in commands)
					menu.Add(string.Format(null, format, it.Prefix, it.Name, it.ModuleName + "\\" + it.Id)).Data = it;

				if (!menu.Show())
					return;

				FarItem mi = menu.Items[menu.Selected];
				IModuleCommand command = (IModuleCommand)mi.Data;

				IInputBox ib = Far.Net.CreateInputBox();
				ib.EmptyEnabled = true;
				ib.HelpTopic = helpTopic;
				ib.Prompt = "New prefix for: " + command.Name;
				ib.Text = command.Prefix;
				ib.Title = "Default prefix: " + command.DefaultPrefix;

				string prefix = null;
				while (ib.Show())
				{
					prefix = ib.Text.Trim();
					if (prefix.IndexOf(' ') >= 0 || prefix.IndexOf(':') >= 0)
					{
						Far.Net.Message("Prefix must not contain ' ' or ':'.");
						prefix = null;
						continue;
					}
					break;
				}
				if (prefix == null)
					continue;

				// restore original on empty
				if (prefix.Length == 0)
					prefix = command.DefaultPrefix;

				// reset
				command.ResetPrefix(prefix);
			}
		}
	}
}
