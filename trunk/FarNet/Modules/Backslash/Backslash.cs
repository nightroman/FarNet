
// Escapes and unescapes \ and " in editor selection with \
// This module implements two items shown in the editor plugin menu.

using System;
using System.Text.RegularExpressions;
using FarNet;

[ModuleTool(Name = "Escape selected text", Options = ModuleToolOptions.Editor)]
public class Escape : ModuleTool
{
	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		IEditor editor = Far.Net.Editor;
		ISelection select = editor.Selection;
		if (select.Exists)
			select.SetText(Regex.Replace(select.GetText("\r"), @"([\\""])", @"\$1"));
	}
}

[ModuleTool(Name = "Unescape selected text", Options = ModuleToolOptions.Editor)]
public class Unescape : ModuleTool
{
	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		IEditor editor = Far.Net.Editor;
		ISelection select = editor.Selection;
		if (select.Exists)
			select.SetText(Regex.Replace(select.GetText("\r"), @"\\([\\""])", "$1"));
	}
}
