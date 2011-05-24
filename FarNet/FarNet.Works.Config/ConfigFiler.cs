
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System.Collections.Generic;

namespace FarNet.Works
{
	public static class ConfigFiler
	{
		public static void Show(IList<IModuleFiler> filers, string helpTopic)
		{
			if (filers == null)
				return;

			IMenu menu = Far.Net.CreateMenu();
			menu.AutoAssignHotkeys = true;
			menu.HelpTopic = helpTopic;
			menu.Title = Res.ModuleFilers;

			foreach(IModuleFiler it in filers)
				menu.Add(Utility.FormatConfigMenu(it)).Data = it;

			while(menu.Show())
			{
				FarItem mi = menu.Items[menu.Selected];
				IModuleFiler filer = (IModuleFiler)mi.Data;

				IInputBox ib = Far.Net.CreateInputBox();
				ib.EmptyEnabled = true;
				ib.HelpTopic = helpTopic;
				ib.History = "Masks";
				ib.Prompt = "New mask for " + filer.Name;
				ib.Text = filer.Mask;
				ib.Title = "Default mask: " + filer.DefaultMask;
				if (!ib.Show())
					continue ;

				string mask = ib.Text.Trim();

				// restore original on empty
				if (mask.Length == 0)
					mask = filer.DefaultMask;

				// set
				filer.ResetMask(mask);
				filer.Manager.SaveSettings();
			}
		}
	}
}
