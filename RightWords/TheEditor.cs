
/*
FarNet module RightWords
Copyright (c) 2011-2012 Roman Kuzmin
*/

namespace FarNet.RightWords
{
	[System.Runtime.InteropServices.Guid("0f1db61f-0cf8-4859-8ee6-46b567ee21ad")]
	[ModuleEditor(Name = Settings.ModuleName, Mask = Settings.AutoHighlightingMask)]
	public class TheEditor : ModuleEditor
	{
		public override void Invoke(object sender, ModuleEditorEventArgs e)
		{
			var editor = (IEditor)sender;
			editor.Data[My.Guid] = new Highlighter(editor);
		}
	}
}
