
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
			var menu = Far.Net.CreateMenu();
			menu.AutoAssignHotkeys = true;
			menu.Title = "Module settings";
			menu.HelpTopic = HelpSettings;

			foreach (var manager in managers)
			{
				foreach (Type type in manager.LoadAssembly().GetExportedTypes())
				{
					if (type.IsAbstract || !typeof(ApplicationSettingsBase).IsAssignableFrom(type))
						continue;

					menu.Add(manager.ModuleName + "\\" + type.Name).Data = type;
				}
			}

			if (!menu.Show())
				return;

			var settingsType = (Type)menu.SelectedData;
			var info = settingsType.GetProperty("Default", BindingFlags.Static | BindingFlags.Public);
			ApplicationSettingsBase settingsInstance;
			if (info == null)
				settingsInstance = (ApplicationSettingsBase)Activator.CreateInstance(settingsType);
			else
				settingsInstance = (ApplicationSettingsBase)info.GetValue(null, null);
			
			(new SettingsExplorer(settingsInstance)).OpenPanel();
		}
	}
}
