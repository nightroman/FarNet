
// Escapes and unescapes \ and " in editor selection with \
// This module implements two items shown in the editor plugin menu.

using System;
using System.Text.RegularExpressions;
using FarNet;

[ModuleTool(Name = Escape.Name, Options = ModuleToolOptions.Editor)]
public class Escape : ModuleTool
{
	public const string Name = "Escape selected text";

	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		IEditor editor = Far.Net.Editor;
		ISelection select = editor.Selection;
		if (select.Exists)
			select.SetText(Regex.Replace(select.GetText("\r"), @"([\\""])", @"\$1"));
	}
}

[ModuleTool(Name = Unescape.Name, Options = ModuleToolOptions.Editor)]
public class Unescape : ModuleTool
{
	public const string Name = "Unescape selected text";

	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		IEditor editor = Far.Net.Editor;
		ISelection select = editor.Selection;
		if (select.Exists)
			select.SetText(Regex.Replace(select.GetText("\r"), @"\\([\\""])", "$1"));
	}
}
