/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PowerShellFar.Commands
{
	///
	public class BaseCmdlet : PSCmdlet
	{
		/// <summary>
		/// Adds cmdlets to a configuration.
		/// </summary>
		internal static void AddCmdlets(RunspaceConfiguration configuration)
		{
			//! add cmdlets; Append() locks, so add all at once
			configuration.Cmdlets.Append(new CmdletConfigurationEntry[] {
new CmdletConfigurationEntry("Find-FarFile", typeof(Commands.FindFarFileCommand), string.Empty),
new CmdletConfigurationEntry("Get-FarFile", typeof(Commands.GetFarFileCommand), string.Empty),
new CmdletConfigurationEntry("Get-FarItem", typeof(Commands.GetFarItemCommand), string.Empty),
new CmdletConfigurationEntry("Get-FarPath", typeof(Commands.GetFarPathCommand), string.Empty),
new CmdletConfigurationEntry("New-FarEditor", typeof(Commands.NewFarEditorCommand), string.Empty),
new CmdletConfigurationEntry("New-FarFile", typeof(Commands.NewFarFileCommand), string.Empty),
new CmdletConfigurationEntry("New-FarItem", typeof(Commands.NewFarItemCommand), string.Empty),
new CmdletConfigurationEntry("New-FarList", typeof(Commands.NewFarListCommand), string.Empty),
new CmdletConfigurationEntry("New-FarMenu", typeof(Commands.NewFarMenuCommand), string.Empty),
new CmdletConfigurationEntry("New-FarObjectPanel", typeof(Commands.NewFarObjectPanelCommand), string.Empty),
new CmdletConfigurationEntry("New-FarUserPanel", typeof(Commands.NewFarUserPanelCommand), string.Empty),
new CmdletConfigurationEntry("New-FarViewer", typeof(Commands.NewFarViewerCommand), string.Empty),
new CmdletConfigurationEntry("Out-FarList", typeof(Commands.OutFarListCommand), string.Empty),
new CmdletConfigurationEntry("Out-FarPanel", typeof(Commands.OutFarPanelCommand), string.Empty),
new CmdletConfigurationEntry("Show-FarMsg", typeof(Commands.ShowFarMsgCommand), string.Empty),
new CmdletConfigurationEntry("Start-FarEditor", typeof(Commands.StartFarEditorCommand), string.Empty),
new CmdletConfigurationEntry("Start-FarJob", typeof(Commands.StartFarJobCommand), string.Empty),
new CmdletConfigurationEntry("Start-FarPanel", typeof(Commands.StartFarPanelCommand), string.Empty),
new CmdletConfigurationEntry("Start-FarViewer", typeof(Commands.StartFarViewerCommand), string.Empty),
new CmdletConfigurationEntry("Trace-Far", typeof(Commands.TraceFarCommand), string.Empty),
new CmdletConfigurationEntry("Update-FarDescription", typeof(Commands.UpdateFarDescriptionCommand), string.Empty)});
		}
		
		internal bool Stop()
		{
			return Stopping;
		}
	}
}
