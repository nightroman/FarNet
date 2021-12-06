
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Reflection;

namespace FarNet.Works.Config
{
	public static class SettingsUI
	{
		internal const string HelpSettings = "module-settings";
		public static void ShowSettings(IEnumerable<IModuleManager> managers)
		{
			// collect sorted data
			var list = new SortedList<string, KeyValuePair<string, IModuleManager>>(StringComparer.OrdinalIgnoreCase);
			foreach (var manager in managers)
			{
				foreach (var typeName in manager.SettingsTypeNames)
					list.Add($"{manager.ModuleName} {typeName}", new KeyValuePair<string, IModuleManager>(typeName, manager));
			}

			// do menu

			var menu = Far.Api.CreateMenu();
			menu.AutoAssignHotkeys = true;
			menu.Title = "Module settings";
			menu.HelpTopic = HelpSettings;

			foreach (var it in list)
				menu.Add(it.Key).Data = it.Value;

			if (!menu.Show())
				return;

			var data = (KeyValuePair<string, IModuleManager>)menu.SelectedData;

			// obtain settings
			var assembly = data.Value.LoadAssembly(false);
			var settingsType = assembly.GetType(data.Key);
			var info = settingsType.GetProperty("Default", BindingFlags.Static | BindingFlags.Public);

			ModuleSettingsBase settingsInstance;
			if (info is null)
			{
				//! `Default` does not have to be defined, e.g. to always use fresh data.
				settingsInstance = (ModuleSettingsBase)Activator.CreateInstance(settingsType);
			}
			else
			{
				//! `Default` must be set
				settingsInstance = (ModuleSettingsBase)info.GetValue(null, null);
				if (settingsInstance is null)
					throw new InvalidOperationException($"{settingsType.FullName} property 'Default' must be not null.");

				//! `Default` must be its type
				if (settingsInstance.GetType() != settingsType)
					throw new InvalidOperationException($"{settingsType.FullName} property 'Default' must have this type.");
			}

			// open the editor
			settingsInstance.Edit();
		}
	}
}
