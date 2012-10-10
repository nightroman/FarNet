
/*
FarNet module Drawer
Copyright (c) 2012 Roman Kuzmin
*/

using System;

namespace FarNet.Drawer
{
	[System.Runtime.InteropServices.Guid(Settings.FixedColumnGuid)]
	[ModuleDrawer(Name = Settings.FixedColumnName, Priority = 1)]
	public class FixedColumnDrawer : ModuleDrawer
	{
		int _columnNumber = Settings.Default.FixedColumnNumber;
		readonly ConsoleColor _foreground = Settings.Default.FixedColumnColorForeground;
		readonly ConsoleColor _background = Settings.Default.FixedColumnColorBackground;
		public override void Invoke(object sender, ModuleDrawerEventArgs e)
		{
			var editor = (IEditor)sender;

			foreach (var line in e.Lines)
			{
				
				e.Colors.Add(new EditorColor(
					line.Index,
					editor.ConvertColumnScreenToEditor(line.Index, _columnNumber - 1),
					editor.ConvertColumnScreenToEditor(line.Index, _columnNumber),
					_foreground, _background));
			}
		}
	}
}
