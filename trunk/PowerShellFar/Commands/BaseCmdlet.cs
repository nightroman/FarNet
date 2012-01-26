
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// PowerShellFar base cmdlet.
	/// </summary>
	class BaseCmdlet : PSCmdlet
	{
		/// <summary>
		/// Adds cmdlets to a configuration.
		/// </summary>
		internal static void AddCmdlets(RunspaceConfiguration configuration)
		{
			const string Help = "PowerShellFar.dll-Help.xml";
			//! add cmdlets; Append() locks, so add all at once
			configuration.Cmdlets.Append(new CmdletConfigurationEntry[] {
new CmdletConfigurationEntry(AssertFarCommand.MyName, typeof(Commands.AssertFarCommand), Help),
new CmdletConfigurationEntry("Find-FarFile", typeof(Commands.FindFarFileCommand), Help),
new CmdletConfigurationEntry("Get-FarFile", typeof(Commands.GetFarFileCommand), Help),
new CmdletConfigurationEntry("Get-FarItem", typeof(Commands.GetFarItemCommand), Help),
new CmdletConfigurationEntry("Get-FarPath", typeof(Commands.GetFarPathCommand), Help),
new CmdletConfigurationEntry("Invoke-FarStepper", typeof(Commands.InvokeFarStepperCommand), Help),
new CmdletConfigurationEntry("New-FarEditor", typeof(Commands.NewFarEditorCommand), Help),
new CmdletConfigurationEntry("New-FarFile", typeof(Commands.NewFarFileCommand), Help),
new CmdletConfigurationEntry("New-FarItem", typeof(Commands.NewFarItemCommand), Help),
new CmdletConfigurationEntry("New-FarList", typeof(Commands.NewFarListCommand), Help),
new CmdletConfigurationEntry("New-FarMenu", typeof(Commands.NewFarMenuCommand), Help),
new CmdletConfigurationEntry("New-FarViewer", typeof(Commands.NewFarViewerCommand), Help),
new CmdletConfigurationEntry("Open-FarEditor", typeof(Commands.OpenFarEditorCommand), Help),
new CmdletConfigurationEntry("Open-FarPanel", typeof(Commands.OpenFarPanelCommand), Help),
new CmdletConfigurationEntry("Open-FarViewer", typeof(Commands.OpenFarViewerCommand), Help),
new CmdletConfigurationEntry("Out-FarList", typeof(Commands.OutFarListCommand), Help),
new CmdletConfigurationEntry("Out-FarPanel", typeof(Commands.OutFarPanelCommand), Help),
new CmdletConfigurationEntry("Search-FarFile", typeof(Commands.SearchFarFileCommand), Help),
new CmdletConfigurationEntry("Show-FarMessage", typeof(Commands.ShowFarMessageCommand), Help),
new CmdletConfigurationEntry("Start-FarJob", typeof(Commands.StartFarJobCommand), Help),
			});
		}
	}
}
