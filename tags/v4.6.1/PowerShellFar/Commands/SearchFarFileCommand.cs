
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System.Management.Automation;
using FarNet;
using FarNet.Tools;

namespace PowerShellFar.Commands
{
	class SearchFarFileCommand : BaseCmdlet
	{
		[Parameter(Position = 0, ParameterSetName = "Mask")]
		public string Mask { get; set; }
		[Parameter(Position = 0, ParameterSetName = "Script")]
		public ScriptBlock Script { get; set; }
		[Parameter()]
		public string XPath { get; set; }
		[Parameter()]
		public string XFile { get; set; }
		[Parameter()]
		public int Depth { get; set; }
		[Parameter()]
		public SwitchParameter Directory { get; set; }
		[Parameter()]
		public SwitchParameter Recurse { get; set; }
		[Parameter()]
		public SwitchParameter Asynchronous { get; set; }
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
