/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;
using System.Globalization;

namespace FarNet.Works
{
	public static class ConfigUICulture
	{
		public static void Show(IList<IModuleManager> managers, string helpTopic)
		{
			IMenu menu = Far.Net.CreateMenu();
			menu.Title = "Module UI culture";
			menu.HelpTopic = helpTopic;
			menu.AutoAssignHotkeys = true;

			int width = 0;
			{
				foreach (IModuleManager it in managers)
					if (width < it.ModuleName.Length)
						width = it.ModuleName.Length;
			}

			for (; ; )
			{
				menu.Items.Clear();
				{
					foreach (IModuleManager it in managers)
						menu.Add(Invariant.Format("{0} : {1}", it.ModuleName.PadRight(width), it.StoredUICulture)).Data = it;
				}

				if (!menu.Show())
					return;

				IModuleManager manager = (IModuleManager)menu.SelectedData;

				// show the input box
				IInputBox ib = Far.Net.CreateInputBox();
				ib.Title = manager.ModuleName;
				ib.Prompt = "Culture name (empty = the Far culture)";
				ib.Text = manager.StoredUICulture;
				ib.History = "Culture";
				ib.HelpTopic = helpTopic;
				ib.EmptyEnabled = true;
				if (!ib.Show())
					continue;

				// set the culture (even the same, to refresh)
				string cultureName = ib.Text.Trim();
				CultureInfo ci;
				try
				{
					// get the culture by name, it may throw
					ci = CultureInfo.GetCultureInfo(cultureName);

					// save the name from the culture, not from a user
					manager.StoredUICulture = ci.Name;

					// use the current Far culture instead of invariant
					if (ci.Name.Length == 0)
						ci = Far.Net.GetCurrentUICulture(true);

					// update the module
					manager.CurrentUICulture = ci;
				}
				catch (ArgumentException)
				{
					Far.Net.Message("Unknown culture name.");
				}
			}
		}
	}
}
