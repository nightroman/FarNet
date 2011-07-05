
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// PowerShellFar base cmdlet.
	/// </summary>
	public class BaseCmdlet : PSCmdlet
	{
		/// <summary>
		/// Adds cmdlets to a configuration.
		/// </summary>
		internal static void AddCmdlets(RunspaceConfiguration configuration)
		{
			//! add cmdlets; Append() locks, so add all at once
			configuration.Cmdlets.Append(new CmdletConfigurationEntry[] {
new CmdletConfigurationEntry(AssertFarCommand.MyName, typeof(Commands.AssertFarCommand), string.Empty),
new CmdletConfigurationEntry("Find-FarFile", typeof(Commands.FindFarFileCommand), string.Empty),
new CmdletConfigurationEntry("Get-FarFile", typeof(Commands.GetFarFileCommand), string.Empty),
new CmdletConfigurationEntry("Get-FarItem", typeof(Commands.GetFarItemCommand), string.Empty),
new CmdletConfigurationEntry("Get-FarPath", typeof(Commands.GetFarPathCommand), string.Empty),
new CmdletConfigurationEntry("New-FarEditor", typeof(Commands.NewFarEditorCommand), string.Empty),
new CmdletConfigurationEntry("New-FarFile", typeof(Commands.NewFarFileCommand), string.Empty),
new CmdletConfigurationEntry("New-FarItem", typeof(Commands.NewFarItemCommand), string.Empty),
new CmdletConfigurationEntry("New-FarList", typeof(Commands.NewFarListCommand), string.Empty),
new CmdletConfigurationEntry("New-FarMenu", typeof(Commands.NewFarMenuCommand), string.Empty),
new CmdletConfigurationEntry("New-FarViewer", typeof(Commands.NewFarViewerCommand), string.Empty),
new CmdletConfigurationEntry("Open-FarEditor", typeof(Commands.OpenFarEditorCommand), string.Empty),
new CmdletConfigurationEntry("Open-FarPanel", typeof(Commands.OpenFarPanelCommand), string.Empty),
new CmdletConfigurationEntry("Open-FarViewer", typeof(Commands.OpenFarViewerCommand), string.Empty),
new CmdletConfigurationEntry("Out-FarList", typeof(Commands.OutFarListCommand), string.Empty),
new CmdletConfigurationEntry("Out-FarPanel", typeof(Commands.OutFarPanelCommand), string.Empty),
new CmdletConfigurationEntry("Search-FarFile", typeof(Commands.SearchFarFileCommand), string.Empty),
new CmdletConfigurationEntry("Show-FarMessage", typeof(Commands.ShowFarMessageCommand), string.Empty),
new CmdletConfigurationEntry("Start-FarJob", typeof(Commands.StartFarJobCommand), string.Empty),
			});
		}
	}
}
