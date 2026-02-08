using System.Reflection;

namespace FarNet.Works;
#pragma warning disable 1591

public static class SettingsUI
{
	internal const string HelpSettings = "settings";

	public static void Show()
	{
		// collect sorted data
		var managers = ModuleLoader.GetModuleManagers();
		var list = managers
			.SelectMany(manager => manager.GetSettingsTypeNames().Select(typeName => new KeyValuePair<IModuleManager, string>(manager, typeName)))
			.OrderBy(x => x.Key.ModuleName, StringComparer.OrdinalIgnoreCase)
			.ThenBy(x => x.Value, StringComparer.OrdinalIgnoreCase)
			.ToList();

		// do menu

		var menu = Far.Api.CreateMenu();
		menu.AutoAssignHotkeys = true;
		menu.Title = "Settings";
		menu.HelpTopic = HelpSettings;

		// add XML settings
		var pad1 = Math.Max(9, list.Max(x => x.Key.ModuleName.Length));
		foreach (var it in list)
			menu.Add($"{it.Key.ModuleName.PadRight(pad1)} {it.Value}").Data = it;

		// add separator
		menu.Add("Actions").IsSeparator = true;

		// add action settings
		int actionIndex = menu.Items.Count;
		List<IModuleCommand> moduleCommands = [];
		List<IModuleDrawer> moduleDrawers = [];
		List<IModuleEditor> moduleEditors = [];
		List<IModuleTool> moduleTools = [];
		foreach (IModuleAction action in Far2.Actions.Values)
		{
			switch (action)
			{
				case IModuleCommand command:
					moduleCommands.Add(command);
					break;
				case IModuleDrawer drawer:
					moduleDrawers.Add(drawer);
					break;
				case IModuleEditor editor:
					moduleEditors.Add(editor);
					break;
				case IModuleTool tool:
					moduleTools.Add(tool);
					break;
			}
		}
		int pad2 = pad1 + 2;
		menu.Add("&Commands".PadRight(pad2) + moduleCommands.Count);
		menu.Add("&Drawers".PadRight(pad2) + moduleDrawers.Count);
		menu.Add("&Editors".PadRight(pad2) + moduleEditors.Count);
		menu.Add("&Tools".PadRight(pad2) + moduleTools.Count);
		menu.Add("Culture").IsSeparator = true;
		menu.Add("&UI culture");

		while (menu.Show())
		{
			switch (menu.Selected - actionIndex)
			{
				case 0:
					ConfigCommand.Show(moduleCommands);
					continue;
				case 1:
					ConfigDrawer.Show(moduleDrawers);
					continue;
				case 2:
					ConfigEditor.Show(moduleEditors);
					continue;
				case 3:
					ConfigTool.Show(moduleTools);
					continue;
				case 5:
					ConfigUICulture.Show();
					continue;
				default:
					var data = (KeyValuePair<IModuleManager, string>)menu.SelectedData!;
					EditSettings(data.Key, data.Value);
					return;
			}
		}
	}

	private static void EditSettings(IModuleManager manager, string typeName)
	{
		// obtain settings
		var assembly = manager.LoadAssembly(false);
		var settingsType = assembly.GetType(typeName) ?? throw new Exception();
		var info = settingsType.GetProperty("Default", BindingFlags.Static | BindingFlags.Public);

		ModuleSettingsBase? instance;
		if (info is null)
		{
			//! no `Default` is fine, use new
			instance = (ModuleSettingsBase)Activator.CreateInstance(settingsType)!;
		}
		else
		{
			//! assert `Default`
			instance = info.GetValue(null, null) as ModuleSettingsBase;
			if (instance is null || instance.GetType() != settingsType)
				throw new ModuleException($"{settingsType.FullName}.Default must be assigned to the same type instance.");
		}

		// open
		instance.Edit();
	}
}
