
// FarNet module Drawer
// Copyright (c) Roman Kuzmin

namespace FarNet.Drawer
{
	[System.Runtime.InteropServices.Guid(Settings.FixedColumnGuid)]
	[ModuleDrawer(Name = Settings.FixedColumnName, Priority = 1)]
	public class FixedColumnDrawer : ModuleDrawer
	{
		public override void Invoke(IEditor editor, ModuleDrawerEventArgs e)
		{
			var sets = Settings.Default.GetData().FixedColumn;
			foreach (var line in e.Lines)
			{
				foreach (var columnNumber in sets.ColumnNumbers)
				{
					e.Colors.Add(new EditorColor(
						line.Index,
						editor.ConvertColumnScreenToEditor(line.Index, columnNumber - 1),
						editor.ConvertColumnScreenToEditor(line.Index, columnNumber),
						sets.ColorForeground, sets.ColorBackground));
				}
			}
		}
	}
}
