
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

[assembly: AssemblyVersion("4.4.20.0")]
[assembly: AssemblyTitle("FarNet Tools")]
[assembly: AssemblyDescription("FarNet Tools")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("http://code.google.com/p/farnet/")]
[assembly: AssemblyProduct("FarNet")]
[assembly: AssemblyCopyright("Copyright (c) 2005 Roman Kuzmin")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]

static class Res
{
	public const string
		Search = "Search",
		SearchActivityDeep = "Found {0} items in {1} directories.\r{2:n2} directory/second.",
		SearchActivityWide = "Found {0} items in {1} directories, {2} in the queue.\r{3:n2} directory/second.",
		Searching = "Searching...",
		SearchTitle = "Found {0} items in {1} directories. {2}",
		StateCompleted = "Completed.",
		StateStopped = "Stopped.",
		StopSearch = "Stop search.";
}
