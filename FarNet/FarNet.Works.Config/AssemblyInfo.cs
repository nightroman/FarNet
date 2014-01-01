
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

[assembly: AssemblyTitle("FarNet configuration")]
[assembly: AssemblyDescription("FarNet configuration")]
[assembly: AssemblyCopyright("Copyright (c) 2006-2014 Roman Kuzmin")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]

static class Res
{
	public const string
		ModuleCommands = "Commands",
		ModuleDrawers = "Drawers",
		ModuleEditors = "Editors",
		ModuleTools = "Tools";
}
