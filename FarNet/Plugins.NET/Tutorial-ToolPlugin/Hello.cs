// Two lessons:
// 1) How to create a plugin called from Far plugin menus.
// 2) How to build a localized plugin and use localized strings.

using System;
using FarNet;

public class Hello : ToolPlugin
{
	public override void Invoke(object sender, ToolEventArgs e)
	{
		Far.Msg(string.Format(GetString("Format"), GetString("Hello"), GetString("World")));
	}
}
