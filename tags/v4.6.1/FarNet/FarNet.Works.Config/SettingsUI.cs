
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace FarNet.Works.Config
{
	public static class SettingsUI
	{
		internal const string HelpSettings = "ModuleSettings";
		public static void ShowSettings(IEnumerable<IModuleManager> managers)
		{
			if (managers == null) return;
			
			// collect data sorted, sort is needed for 2+ settings in an assembly
			var list = new SortedList<string, Type>(StringComparer.OrdinalIgnoreCase);
			foreach (var manager in managers)
				foreach (Type type in manager.LoadAssembly(false).GetExportedTypes())
					if (!type.IsAbstract && typeof(ApplicationSettingsBase).IsAssignableFrom(type))
						list.Add(manager.ModuleName + "\\" + type.Name, type);

			// do menu
			
			var menu = Far.Net.CreateMenu();
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
			ApplicationSettingsBase settingsInstance;
			if (info == null)
				settingsInstance = (ApplicationSettingsBase)Activator.CreateInstance(settingsType);
			else
				settingsInstance = (ApplicationSettingsBase)info.GetValue(null, null);

			// open the panel
			(new SettingsExplorer(settingsInstance)).OpenPanel();
		}
	}
}
