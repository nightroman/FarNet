
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Linq;
using FarNet.Forms;

namespace FarNet.Works
{
	public static class ConfigTool
	{
		public static string ValidateMask(string mask)
		{
			mask = mask.Trim();
			if (mask.Length == 0 || Far.Api.IsMaskValid(mask))
				return mask;

			Far.Api.Message("Invalid Mask.");
			return null;
		}
		public static void Show(IList<IModuleTool> toolsIn, string helpTopic, Func<IModuleTool, string> getMenuText)
		{
			if (getMenuText == null)
				throw new ArgumentNullException("getMenuText");

			var sorted = toolsIn.OrderBy(getMenuText, StringComparer.OrdinalIgnoreCase).ToList();

			IMenu menu = Far.Api.CreateMenu();
			menu.HelpTopic = helpTopic;
			menu.Title = Res.ModuleTools;

			IModuleTool tool = null;
			for (; ; )
			{
				// format
				int widthName = 9; // Name
				int widthAttr = 7; // Options
				if (sorted.Count > 0)
				{
					widthName = Math.Max(widthName, sorted.Max(x => getMenuText(x).Length));
					widthAttr = Math.Max(widthAttr, sorted.Max(x => x.Options.ToString().Length));
				}
				widthName += 3;
				string format = "{0,-" + widthName + "} : {1,-" + widthAttr + "} : {2}";

				// refill
				menu.Items.Clear();
				menu.Add(string.Format(null, format, "Title", "Options", "Address")).Disabled = true;
				foreach (IModuleTool it in sorted)
				{
					// 1) restore the current item, its index vary due to sorting with new hotkeys
					if (tool != null && it == tool)
						menu.Selected = menu.Items.Count;

					// 2) add the item
					menu.Add(string.Format(null, format, getMenuText(it), it.Options, it.Manager.ModuleName + "\\" + it.Id)).Data = it;
				}

				// show
				if (!menu.Show())
					return;

				// the tool
				tool = (IModuleTool)menu.SelectedData;

				// dialog
				IDialog dialog = Far.Api.CreateDialog(-1, -1, 77, 12);
				dialog.HelpTopic = helpTopic;
				dialog.AddBox(3, 1, 0, 0, tool.Name);

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

				// new options
				ModuleToolOptions newOptions = ModuleToolOptions.None;
				if (cbPanels.Selected > 0) newOptions = newOptions | ModuleToolOptions.Panels;
				if (cbEditor.Selected > 0) newOptions = newOptions | ModuleToolOptions.Editor;
				if (cbViewer.Selected > 0) newOptions = newOptions | ModuleToolOptions.Viewer;
				if (cbDialog.Selected > 0) newOptions = newOptions | ModuleToolOptions.Dialog;
				if (cbConfig.Selected > 0) newOptions = newOptions | ModuleToolOptions.Config;
				if (cbDisk.Selected > 0) newOptions = newOptions | ModuleToolOptions.Disk;
				
				tool.Options = newOptions;
				tool.Manager.SaveSettings();
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
