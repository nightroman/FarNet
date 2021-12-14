
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;

namespace FarNet.Works
{
	public static class ConfigEditor
	{
		const string HelpTopic = "configure-editors";

		public static void Show(List<IModuleEditor> editors)
		{
			var menu = Far.Api.CreateMenu();
			menu.AutoAssignHotkeys = true;
			menu.HelpTopic = HelpTopic;
			menu.Title = "Editors";
			menu.AddSimpleConfigItems(editors);

			while (menu.Show())
			{
				var editor = (IModuleEditor)menu.Items[menu.Selected].Data;

				var ib = Far.Api.CreateInputBox();
				ib.EmptyEnabled = true;
				ib.HelpTopic = HelpTopic;
				ib.History = "Masks";
				ib.Prompt = "Mask";
				ib.Text = editor.Mask;
				ib.Title = editor.Name;
				if (!ib.Show())
					continue;

				var mask = ConfigTool.ValidateMask(ib.Text);
				if (mask == null)
					continue;

				editor.Mask = mask;
				editor.Manager.SaveConfig();
			}
		}
	}
}
