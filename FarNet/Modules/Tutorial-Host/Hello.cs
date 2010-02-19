
// *) How to create a module host.
// *) How to create a module tool called from the plugin menus.
// *) How to build a localized module and use localized strings.

using FarNet;
using System;
using System.Runtime.InteropServices;

// Menu item "Hello" in all plugin menus.
[ModuleTool(Name = Hello.Name, Options = ModuleToolOptions.F11Menus, Resources = true)]
[Guid("68c56c3a-b832-4e9d-983f-9922ba4e6d9d")]
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
[Guid("45932afa-0eaf-448f-95d8-9433c028f268")]
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
