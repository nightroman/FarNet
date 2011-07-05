
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
		/// XPath expression text.
		/// </summary>
		[Parameter(HelpMessage = "XPath expression text.")]
		public string XPath { get; set; }
		/// <summary>
		/// XPath expression file.
		/// </summary>
		[Parameter(HelpMessage = "XPath expression file.")]
		public string XFile { get; set; }
		/// <summary>
		/// Search depth. 0: ignored; negative: unlimited.
		/// </summary>
		[Parameter(HelpMessage = "Search depth. 0: ignored; negative: unlimited.")]
		public int Depth { get; set; }
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
		/// <summary>
		/// Tells to performs the search in the background and to open the result panel immediately.
		/// </summary>
		[Parameter(HelpMessage = "Tells to performs the search in the background and to open the result panel immediately.")]
		public SwitchParameter Asynchronous { get; set; }
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
			var search = new SearchFileCommand(panel.Explorer);
			search.XPath = XPath;
			search.XFile = XFile;
			search.Depth = Depth;
			search.Recurse = Recurse;
			search.Directory = Directory;
			if (Mask != null)
			{
				search.Filter = delegate(Explorer explorer, FarFile file)
				{
					return Far.Net.MatchPattern(file.Name, Mask);
				};
			}
			else if (Script != null)
			{
				search.Filter = delegate(Explorer explorer, FarFile file)
				{
					return LanguagePrimitives.IsTrue(A.InvokeScriptReturnAsIs(Script, explorer, file));
				};
			}
			
			// go
			if (Asynchronous)
				search.InvokeAsync(panel);
			else
				search.Invoke(panel);
		}
	}
}
