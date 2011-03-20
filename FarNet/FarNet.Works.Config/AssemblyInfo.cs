
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

[assembly: AssemblyVersion("4.4.5.0")]
[assembly: AssemblyTitle("FarNet configuration")]
[assembly: AssemblyDescription("FarNet configuration")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("http://code.google.com/p/farnet/")]
[assembly: AssemblyProduct("FarNet")]
[assembly: AssemblyCopyright("Copyright (c) 2005 FarNet Team")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]

static class Res
{
	public const string
		ModuleCommands = "Commands",
		ModuleEditors = "Editors",
		ModuleFilers = "Filers";
}
