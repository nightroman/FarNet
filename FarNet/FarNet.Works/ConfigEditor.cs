/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System.Collections.Generic;

namespace FarNet.Works
{
	public static class ConfigEditor
	{
		public static void Show(IList<IModuleEditor> editors, string helpTopic)
		{
			IMenu menu = Far.Net.CreateMenu();
			menu.AutoAssignHotkeys = true;
			menu.HelpTopic = helpTopic;
			menu.Title = Res.ModuleEditors;

			foreach(IModuleEditor it in editors)
				menu.Add(it.ModuleName + "\\" + it.Id).Data = it;

			while(menu.Show())
			{
				FarItem mi = menu.Items[menu.Selected];
				IModuleEditor editor = (IModuleEditor)mi.Data;

				IInputBox ib = Far.Net.CreateInputBox();
				ib.EmptyEnabled = true;
				ib.HelpTopic = helpTopic;
				ib.History = "Masks";
				ib.Prompt = "New mask for " + editor.Name;
				ib.Text = editor.Mask;
				ib.Title = "Default mask: " + editor.DefaultMask;

				if (!ib.Show())
					return;
				string mask = ib.Text.Trim();

				// restore original on empty
				if (mask.Length == 0)
					mask = editor.DefaultMask;

				// set
				editor.ResetMask(mask);
			}
		}
	}
}
