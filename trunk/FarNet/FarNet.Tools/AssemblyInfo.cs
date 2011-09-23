
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

[assembly: AssemblyTitle("FarNet Tools")]
[assembly: AssemblyDescription("FarNet Tools")]
[assembly: AssemblyCopyright("Copyright (c) 2006-2011 Roman Kuzmin")]

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
