
// *) How to create a module host.
// *) How to create a module tool called from the plugin menus.
// *) How to build a localized module and use localized strings.

using System;
using FarNet;

// Menu item "Hello" in all plugin menus.
[ModuleTool(Name = Hello.Name, Options = ModuleToolOptions.F11Menus)]
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
		Console.Title = "Disconnect()";
	}
}
