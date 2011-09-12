
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2011 Roman Kuzmin
*/

using System.Management.Automation;
using FarNet;

namespace FarMacro
{
	public class BaseFarMacroCmdlet : BaseCmdlet
	{
		[Parameter(Position = 0, Mandatory = true, ParameterSetName = "Properties")]
		public MacroArea Area { get; set; }
		[Parameter(Position = 1, Mandatory = true)]
		public string Name { get; set; }
		[Parameter(Position = 2)]
		public string Sequence { get; set; }
		[Parameter(Position = 3)]
		public string Description { get; set; }
		[Parameter]
		public string CommandLine { get; set; }
		[Parameter]
		public string SelectedText { get; set; }
		[Parameter]
		public string SelectedItems { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
		[Parameter]
		public string PanelIsPlugin { get; set; }
		[Parameter]
		public string ItemIsDirectory { get; set; }
		[Parameter]
		public string SelectedItems2 { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
		[Parameter]
		public string PanelIsPlugin2 { get; set; }
		[Parameter]
		public string ItemIsDirectory2 { get; set; }
		[Parameter]
		public SwitchParameter EnableOutput { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
		[Parameter]
		public SwitchParameter DisablePlugins { get; set; }
		[Parameter]
		public SwitchParameter RunAfterFarStart { get; set; }
		protected Macro CreateMacro()
		{
			Macro macro = new Macro();

			macro.Area = Area;
			macro.Name = Name;
			macro.Sequence = Sequence;
			macro.Description = Description;
			macro.EnableOutput = EnableOutput;
			macro.DisablePlugins = DisablePlugins;
			macro.RunAfterFarStart = RunAfterFarStart;
			macro.CommandLine = CommandLine;
			macro.SelectedText = SelectedText;
			macro.SelectedItems = SelectedItems;
			macro.PanelIsPlugin = PanelIsPlugin;
			macro.ItemIsDirectory = ItemIsDirectory;
			macro.SelectedItems2 = SelectedItems2;
			macro.PanelIsPlugin2 = PanelIsPlugin2;
			macro.ItemIsDirectory2 = ItemIsDirectory2;

			return macro;
		}
	}
}
