
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.ComponentModel;
using System.Management.Automation;
using FarNet;
using FarNet.Tools;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Starts search in the explorer panel and opens the result panel.
	/// </summary>
	[Description("Starts search in the explorer panel and opens the result panel.")]
	public class StartFarSearchCommand : BaseCmdlet
	{
		/// <summary>
		/// Search script. Variables: <c>$this</c> is the explorer providing the file, <c>$_</c> is the file.
		/// </summary>
		[Parameter(Position = 0, HelpMessage = "Search script. Variables: $this is the explorer providing the file, $_ is the file.")]
		public ScriptBlock Script { get; set; }
		/// <summary>
		/// Tells to include directories into the search process and results.
		/// </summary>
		[Parameter(HelpMessage = "Tells to include directories into the search process and results.")]
		public SwitchParameter Directory { get; set; }
		/// <summary>
		/// Tells to search through all directories and sub-directories.
		/// </summary>
		[Parameter(HelpMessage = "Tells to search through all directories and sub-directories.")]
		public SwitchParameter Recurse { get; set; }
		///
		protected override void BeginProcessing()
		{
			Panel mpanel;
			if ((mpanel = Far.Net.Panel as Panel) == null || mpanel.Explorer == null)
			{
				WriteWarning("This is not an explorer panel.");
				return;
			}

			// setup the search
			var search = new FileSearchExplorer(mpanel.Explorer);
			search.Directory = Directory;
			search.Recurse = Recurse;
			if (Script != null)
			{
				search.Process = delegate(Explorer explorer, FarFile file)
				{
					return LanguagePrimitives.IsTrue(A.InvokeScriptReturnAsIs(Script, explorer, file));
				};
			}
			
			// go
			search.Invoke();
			if (search.ResultFiles.Count == 0)
				return;

			// panel
			var newPanel = search.CreatePanel();
			newPanel.OpenChild(mpanel);
		}
	}
}
