
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.Linq;
using FarNet.Forms;

namespace FarNet.Works;
#pragma warning disable 1591

public static class ConfigTool
{
	const string HelpTopic = "configure-tools";

	public static string? ValidateMask(string mask)
	{
		mask = mask.Trim();
		if (mask.Length == 0 || Far.Api.IsMaskValid(mask))
			return mask;

		Far.Api.Message("Invalid Mask.");
		return null;
	}

	public static void Show(List<IModuleTool> tools)
	{
		var list = tools.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();

		var menu = Far.Api.CreateMenu();
		menu.AutoAssignHotkeys = true;
		menu.HelpTopic = HelpTopic;
		menu.Title = "Tools (name, options, module)";

		for (; ; )
		{
			int max1 = list.Max(x => x.Name.Length);
			int max2 = list.Max(x => x.Options.ToString().Length);
			int max3 = list.Max(x => x.Manager.ModuleName.Length);

			menu.Items.Clear();
			foreach (var it in list)
				menu.Add($"{it.Name.PadRight(max1)} {it.Options.ToString().PadRight(max2)} {it.Manager.ModuleName.PadRight(max3)} {it.Id}").Data = it;

			if (!menu.Show())
				return;

			var tool = (IModuleTool)menu.SelectedData!;

			var dialog = Far.Api.CreateDialog(-1, -1, 77, 12);
			dialog.HelpTopic = HelpTopic;
			dialog.AddBox(3, 1, 0, 0, tool.Name);

			var defaultOptions = tool.DefaultOptions;
			var currentOptions = tool.Options;

			var cbPanels = AddOption(dialog, "&Panels", ModuleToolOptions.Panels, defaultOptions, currentOptions);
			var cbEditor = AddOption(dialog, "&Editor", ModuleToolOptions.Editor, defaultOptions, currentOptions);
			var cbViewer = AddOption(dialog, "&Viewer", ModuleToolOptions.Viewer, defaultOptions, currentOptions);
			var cbDialog = AddOption(dialog, "&Dialog", ModuleToolOptions.Dialog, defaultOptions, currentOptions);
			var cbConfig = AddOption(dialog, "&Config", ModuleToolOptions.Config, defaultOptions, currentOptions);
			var cbDisk = AddOption(dialog, "Dis&k", ModuleToolOptions.Disk, defaultOptions, currentOptions);

			dialog.AddText(5, -1, 0, string.Empty).Separator = 1;

			var buttonOK = dialog.AddButton(0, -1, "OK");
			buttonOK.CenterGroup = true;
			dialog.Default = buttonOK;
			dialog.Cancel = dialog.AddButton(0, 0, "Cancel");
			dialog.Cancel.CenterGroup = true;

			if (!dialog.Show())
				continue;

			var newOptions = ModuleToolOptions.None;
			if (cbPanels.Selected > 0) newOptions |= ModuleToolOptions.Panels;
			if (cbEditor.Selected > 0) newOptions |= ModuleToolOptions.Editor;
			if (cbViewer.Selected > 0) newOptions |= ModuleToolOptions.Viewer;
			if (cbDialog.Selected > 0) newOptions |= ModuleToolOptions.Dialog;
			if (cbConfig.Selected > 0) newOptions |= ModuleToolOptions.Config;
			if (cbDisk.Selected > 0) newOptions |= ModuleToolOptions.Disk;

			tool.Options = newOptions;
			tool.Manager.SaveConfig();
		}
	}

	static ICheckBox AddOption(IDialog dialog, string text, ModuleToolOptions option, ModuleToolOptions defaultOptions, ModuleToolOptions currentOptions)
	{
		var result = dialog.AddCheckBox(5, -1, text);
		if (0 == (option & defaultOptions))
			result.Disabled = true;
		else if (0 != (option & currentOptions))
			result.Selected = 1;
		return result;
	}
}
