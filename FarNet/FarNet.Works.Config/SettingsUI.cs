
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace FarNet.Works.Config
{
	public static class SettingsUI
	{
		internal const string HelpSettings = "module-settings";
		public static void ShowSettings(IEnumerable<IModuleManager> managers)
		{
			if (managers is null)
				return;

			// collect data sorted, sort is needed for 2+ settings in an assembly
			var list = new SortedList<string, Type>(StringComparer.OrdinalIgnoreCase);
			foreach (var manager in managers)
			{
				foreach (Type type in manager.LoadAssembly(false).GetExportedTypes())
				{
					if (type.IsAbstract)
						continue;

					if (typeof(ModuleSettingsBase).IsAssignableFrom(type))
					{
						var browsable = type.GetCustomAttribute<BrowsableAttribute>();
						if (browsable is null || browsable.Browsable)
							list.Add(manager.ModuleName + "\\" + type.Name, type);
						continue;
					}
				}
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

			var settingsType = (Type)menu.SelectedData;

			// obtain settings
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
					throw new InvalidOperationException($"{settingsType.FullName} property 'Default' must have the same type.");
			}

			// open the editor
			settingsInstance.Edit();
		}
	}
}
