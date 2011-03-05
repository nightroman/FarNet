
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
	/// Search-FarFile command.
	/// Searches files in the panel opens the result panel.
	/// </summary>
	[Description("Searches files in the panel opens the result panel.")]
	public class SearchFarFileCommand : BaseCmdlet
	{
		/// <summary>
		/// Classic Far Manager file mask including exclude and regex forms.
		/// </summary>
		[Parameter(Position = 0, ParameterSetName = "Mask",
			HelpMessage = "Classic Far Manager file mask including exclude and regex forms.")]
		public string Mask { get; set; }
		/// <summary>
		/// Search script. Variables: <c>$this</c> is the explorer providing the file, <c>$_</c> is the file.
		/// </summary>
		[Parameter(Position = 0, ParameterSetName = "Script",
			HelpMessage = "Search script. Variables: $this is the explorer providing the file, $_ is the file.")]
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
			Panel panel = Far.Net.Panel as Panel;
			if (panel == null)
			{
				WriteWarning("This is not a module panel.");
				return;
			}

			// setup the search
			var search = new FileSearchExplorer(panel.Explorer);
			search.Directory = Directory;
			search.Recurse = Recurse;
			if (Mask != null)
			{
				search.Process = delegate(Explorer explorer, FarFile file)
				{
					return Far.Net.MatchPattern(file.Name, Mask);
				};
			}
			else if (Script != null)
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
			search.OpenPanelChild(panel);
		}
	}
}
