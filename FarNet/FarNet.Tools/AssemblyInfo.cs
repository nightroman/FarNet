using System;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

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
