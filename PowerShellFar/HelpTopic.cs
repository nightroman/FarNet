
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System.Runtime.CompilerServices;

namespace PowerShellFar
{
	/// <summary>
	/// Help topics.
	/// </summary>
	static class HelpTopic
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		internal static string Get(string topic)
		{
			return Far.Api.GetHelpTopic(topic);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		internal static void Show(string topic)
		{
			Far.Api.ShowHelpTopic(topic);
		}

		/// <summary>
		/// CAUTION: Use with <see cref="Get"/> and <see cref="Show"/> to avoid inlining.
		/// </summary>
		public const string
			BreakpointDialog = "breakpoint-dialog",
			CommandConsole = "command-console-dialog",
			CommandHistory = "command-history",
			CommandLine = "command-line",
			BackgroundJobsMenu = "background-jobs-menu",
			DataPanel = "data-panel",
			DebuggerDialog = "debugger-dialog",
			DebuggerMenu = "debugger-menu",
			ErrorsMenu = "errors-menu",
			FolderTree = "folder-tree",
			InteractiveMenu = "interactive-menu",
			InvokeCommandsDialog = "invoke-commands-dialog",
			ListPanel = "list-panel",
			MenuCommands = "menu-commands",
			PowerPanel = "power-panel",
			PowerPanelMenu = "power-panel-menu",
			TreePanel = "tree-panel",
			Contents = "Contents";
	}
}
