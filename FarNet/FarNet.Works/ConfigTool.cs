/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System.Collections.Generic;
using FarNet.Forms;

namespace FarNet.Works
{
	public static class ConfigTool
	{
		public static void Show(List<IModuleTool> tools, string helpTopic, string menuPrefix, IComparer<IModuleTool> comparer)
		{
			IMenu menu = Far.Net.CreateMenu();
			menu.Title = "Menu tools";
			menu.HelpTopic = helpTopic;

			IModuleTool tool = null;
			for (; ; )
			{
				// format
				int widthName = 9; // Name
				int widthAttr = 7; // Options
				foreach (IModuleTool it in tools)
				{
					if (widthName < it.Name.Length)
						widthName = it.Name.Length;
					if (widthAttr < it.Options.ToString().Length)
						widthAttr = it.Options.ToString().Length;
				}
				widthName += 3;
				string format = menuPrefix + "{0,-" + widthName + "} : {1,-" + widthAttr + "} : {2}";

				// reset
				menu.Items.Clear();
				tools.Sort(comparer);

				// fill
				menu.Add(Invariant.Format(format, " & Name", "Options", "Address")).Disabled = true;
				foreach (IModuleTool it in tools)
				{
					// 1) restore the current item, its index vary due to sorting with new hotkeys
					if (tool != null && it == tool)
						menu.Selected = menu.Items.Count;

					// 2) add the item
					menu.Add(Invariant.Format(format, "&" + it.Hotkey + " " + it.Name, it.Options, it.ModuleName + "\\" + it.Id)).Data = it;
				}

				// show
				if (!menu.Show())
					return;

				// the tool
				tool = (IModuleTool)menu.SelectedData;

				// dialog
				IDialog dialog = Far.Net.CreateDialog(-1, -1, 77, 14);
				dialog.HelpTopic = helpTopic;
				dialog.AddBox(3, 1, 0, 0, tool.Name);

				IEdit edHotkey = dialog.AddEditFixed(5, -1, 5, (tool.Hotkey == " " ? string.Empty : tool.Hotkey));
				dialog.AddText(7, 0, 0, "&Hotkey");

				dialog.AddText(5, -1, 0, string.Empty).Separator = 1;

				ModuleToolOptions defaultOptions = tool.DefaultOptions;
				ModuleToolOptions currentOptions = tool.Options;

				ICheckBox cbPanels = AddOption(dialog, "&Panels", ModuleToolOptions.Panels, defaultOptions, currentOptions);
				ICheckBox cbEditor = AddOption(dialog, "&Editor", ModuleToolOptions.Editor, defaultOptions, currentOptions);
				ICheckBox cbViewer = AddOption(dialog, "&Viewer", ModuleToolOptions.Viewer, defaultOptions, currentOptions);
				ICheckBox cbDialog = AddOption(dialog, "&Dialog", ModuleToolOptions.Dialog, defaultOptions, currentOptions);
				ICheckBox cbConfig = AddOption(dialog, "&Config", ModuleToolOptions.Config, defaultOptions, currentOptions);
				ICheckBox cbDisk = AddOption(dialog, "Dis&k", ModuleToolOptions.Disk, defaultOptions, currentOptions);

				dialog.AddText(5, -1, 0, string.Empty).Separator = 1;

				IButton buttonOK = dialog.AddButton(0, -1, "Ok");
				buttonOK.CenterGroup = true;
				dialog.Default = buttonOK;
				dialog.Cancel = dialog.AddButton(0, 0, "Cancel");
				dialog.Cancel.CenterGroup = true;

				if (!dialog.Show())
					continue;

				// new hotkey
				tool.ResetHotkey(edHotkey.Text);

				// new options
				ModuleToolOptions newOptions = ModuleToolOptions.None;
				if (cbPanels.Selected > 0) newOptions = newOptions | ModuleToolOptions.Panels;
				if (cbEditor.Selected > 0) newOptions = newOptions | ModuleToolOptions.Editor;
				if (cbViewer.Selected > 0) newOptions = newOptions | ModuleToolOptions.Viewer;
				if (cbDialog.Selected > 0) newOptions = newOptions | ModuleToolOptions.Dialog;
				if (cbConfig.Selected > 0) newOptions = newOptions | ModuleToolOptions.Config;
				if (cbDisk.Selected > 0) newOptions = newOptions | ModuleToolOptions.Disk;
				tool.ResetOptions(newOptions);
			}
		}

		static ICheckBox AddOption(IDialog dialog, string text, ModuleToolOptions option, ModuleToolOptions defaultOptions, ModuleToolOptions currentOptions)
		{
			ICheckBox result = dialog.AddCheckBox(5, -1, text);
			if (0 == (option & defaultOptions))
				result.Disabled = true;
			else if (0 != (option & currentOptions))
				result.Selected = 1;
			return result;
		}

	}
}
