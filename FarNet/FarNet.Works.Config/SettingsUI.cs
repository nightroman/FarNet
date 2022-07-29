
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FarNet.Works;

public static class SettingsUI
{
	internal const string HelpSettings = "module-settings";

	public static void ShowSettings(IEnumerable<IModuleManager> managers)
	{
		// collect sorted data
		var list = managers
			.SelectMany(manager => manager.SettingsTypeNames.Select(typeName => new KeyValuePair<IModuleManager, string>(manager, typeName)))
			.OrderBy(x => x.Key.ModuleName, StringComparer.OrdinalIgnoreCase)
			.ThenBy(x => x.Value, StringComparer.OrdinalIgnoreCase)
			.ToList();

		// do menu

		var menu = Far.Api.CreateMenu();
		menu.AutoAssignHotkeys = true;
		menu.Title = "Module settings";
		menu.HelpTopic = HelpSettings;

		var maxModuleName = list.Max(x => x.Key.ModuleName.Length);
		foreach (var it in list)
			menu.Add($"{it.Key.ModuleName.PadRight(maxModuleName)} {it.Value}").Data = it;

		if (!menu.Show())
			return;

		var data = (KeyValuePair<IModuleManager, string>)menu.SelectedData;

		// obtain settings
		var assembly = data.Key.LoadAssembly(false);
		var settingsType = assembly.GetType(data.Value);
		var info = settingsType.GetProperty("Default", BindingFlags.Static | BindingFlags.Public);

		ModuleSettingsBase instance;
		if (info is null)
		{
			//! no `Default` is fine, use new
			instance = (ModuleSettingsBase)Activator.CreateInstance(settingsType);
		}
		else
		{
			//! assert `Default`
			instance = (ModuleSettingsBase)info.GetValue(null, null);
			if (instance is null || instance.GetType() != settingsType)
				throw new ModuleException($"{settingsType.FullName}.Default must be assigned to the same type instance.");
		}

		// open
		instance.Edit();
	}
}
