
using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("FarNet configuration")]
[assembly: AssemblyDescription("FarNet configuration")]
[assembly: AssemblyCopyright("Copyright (c) 2006-2016 Roman Kuzmin")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

static class Res
{
	public const string
		ModuleCommands = "Commands",
		ModuleDrawers = "Drawers",
		ModuleEditors = "Editors",
		ModuleTools = "Tools";
}
