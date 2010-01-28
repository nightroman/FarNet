
// Two lessons:
// 1) How to create a module tool called from Far plugin menus.
// 2) How to build a localized module and use localized strings.

using System;
using FarNet;

public class Hello : ModuleTool
{
	public override void Invoke(object sender, ToolEventArgs e)
	{
		Far.Msg(string.Format(GetString("Format"), GetString("Hello"), GetString("World")));
	}
}
