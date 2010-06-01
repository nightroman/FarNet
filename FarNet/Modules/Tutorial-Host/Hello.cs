
// *) How to create a module host.
// *) How to create a module tool called from the plugin menus.
// *) How to build a localized module and use localized strings.

using FarNet;
using System;

// Menu item "Hello" in all areas.
[System.Runtime.InteropServices.Guid("68c56c3a-b832-4e9d-983f-9922ba4e6d9d")]
[ModuleTool(Name = Hello.Name, Options = ModuleToolOptions.AllAreas, Resources = true)]
public class Hello : ModuleTool
{
	public const string Name = "Hello";

	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		Far.Net.Message(string.Format(GetString("Format"), GetString("Hello"), GetString("World")));
	}
}

// The host is loaded and connected only on "Hello" menus.
[ModuleHost(Load = false)]
public class Host : ModuleHost
{
	// This is called once before the first call of "Hello".
	public override void Connect()
	{
		Far.Net.Message("Connect()");
	}

	// This is called once on Far exit. UI is not allowed.
	public override void Disconnect()
	{
		Far.Net.UI.WindowTitle = "Disconnect()";
	}
}
