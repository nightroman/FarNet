
// FarNet module Drawer
// Copyright (c) Roman Kuzmin

using System;

namespace FarNet.Drawer;

[ModuleDrawer(Name = Settings.TabsName, Priority = 1, Id = Settings.TabsGuid)]
public class TabsDrawer : ModuleDrawer
{
	public override void Invoke(IEditor editor, ModuleDrawerEventArgs e)
	{
		foreach (var line in e.Lines)
		{
			var text = line.Text;
			for (int index = line.Length - 1; index >= 0;)
			{
				// skip not tabs
				while (index >= 0 && text[index] != '\t')
					--index;

				if (index < 0)
					break;

				// index -> last tab
				int end = index + 1;
				--index;

				// skip tabs
				while (index >= 0 && text[index] == '\t')
					--index;

				// index -> last not tab
				e.Colors.Add(new EditorColor(
					line.Index,
					index + 1,
					end,
					ConsoleColor.Black, ConsoleColor.Yellow));

				--index;
			}
		}
	}
}
