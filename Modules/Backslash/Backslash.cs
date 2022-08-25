
// Escapes and unescapes \ and " in editor selection with \
// This module implements two items shown in the editor plugin menu.

using FarNet;
using System.Text.RegularExpressions;

[ModuleTool(Name = "Escape selected text", Options = ModuleToolOptions.Editor, Id = "e3b6663c-d6de-4494-9991-eafb4385fba5")]
public class Escape : ModuleTool
{
	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		IEditor editor = Far.Api.Editor;
		if (editor.SelectionExists)
			editor.SetSelectedText(Regex.Replace(editor.GetSelectedText("\r"), @"([\\""])", @"\$1"));
	}
}

[ModuleTool(Name = "Unescape selected text", Options = ModuleToolOptions.Editor, Id = "3857bfda-96fc-4e98-8203-e8d2f4c934f5")]
public class Unescape : ModuleTool
{
	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		IEditor editor = Far.Api.Editor;
		if (editor.SelectionExists)
			editor.SetSelectedText(Regex.Replace(editor.GetSelectedText("\r"), @"\\([\\""])", "$1"));
	}
}
