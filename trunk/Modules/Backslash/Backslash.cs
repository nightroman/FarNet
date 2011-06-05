
// Escapes and unescapes \ and " in editor selection with \
// This module implements two items shown in the editor plugin menu.

using FarNet;
using System;
using System.Text.RegularExpressions;

[System.Runtime.InteropServices.Guid("e3b6663c-d6de-4494-9991-eafb4385fba5")]
[ModuleTool(Name = Escape.Name, Options = ModuleToolOptions.Editor)]
public class Escape : ModuleTool
{
	public const string Name = "Escape selected text";

	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		IEditor editor = Far.Net.Editor;
		if (editor.SelectionExists)
			editor.SetSelectedText(Regex.Replace(editor.GetSelectedText("\r"), @"([\\""])", @"\$1"));
	}
}

[System.Runtime.InteropServices.Guid("3857bfda-96fc-4e98-8203-e8d2f4c934f5")]
[ModuleTool(Name = Unescape.Name, Options = ModuleToolOptions.Editor)]
public class Unescape : ModuleTool
{
	public const string Name = "Unescape selected text";

	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		IEditor editor = Far.Net.Editor;
		if (editor.SelectionExists)
			editor.SetSelectedText(Regex.Replace(editor.GetSelectedText("\r"), @"\\([\\""])", "$1"));
	}
}
