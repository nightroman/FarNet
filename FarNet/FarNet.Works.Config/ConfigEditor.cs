
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

using System.Collections.Generic;

namespace FarNet.Works
{
	static class Utility
	{
		internal static string FormatConfigMenu(IModuleAction action)
		{
			return string.Format(null, "{0} {1}\\{2}", action.Name, action.Manager.ModuleName, action.Id);
		}
	}

	public static class ConfigEditor
	{
		public static void Show(IList<IModuleEditor> editors, string helpTopic)
		{
			if (editors == null)
				return;

			IMenu menu = Far.Net.CreateMenu();
			menu.AutoAssignHotkeys = true;
			menu.HelpTopic = helpTopic;
			menu.Title = Res.ModuleEditors;

			foreach (IModuleEditor it in editors)
				menu.Add(Utility.FormatConfigMenu(it)).Data = it;

			while (menu.Show())
			{
				FarItem mi = menu.Items[menu.Selected];
				IModuleEditor editor = (IModuleEditor)mi.Data;

				IInputBox ib = Far.Net.CreateInputBox();
				ib.EmptyEnabled = true;
				ib.HelpTopic = helpTopic;
				ib.History = "Masks";
				ib.Prompt = "Mask";
				ib.Text = editor.Mask;
				ib.Title = editor.Name;
				if (!ib.Show())
					continue;

				var mask = ConfigTool.ValidateMask(ib.Text);
				if (mask == null)
					continue;

				// set
				editor.Mask = mask;
				editor.Manager.SaveSettings();
			}
		}
	}
}
