
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PowerShellFar.Commands;

/// <summary>
/// PowerShellFar base cmdlet.
/// </summary>
class BaseCmdlet : PSCmdlet
{
	const string Help = "PowerShellFar.dll-Help.xml";

	/// <summary>
	/// Adds cmdlets to the initial state.
	/// </summary>
	internal static void AddCmdlets(InitialSessionState state)
	{
		state.Commands.Add(new SessionStateCmdletEntry[] {
			new SessionStateCmdletEntry(AssertFarCommand.MyName, typeof(AssertFarCommand), Help),
			new SessionStateCmdletEntry("Find-FarFile", typeof(FindFarFileCommand), Help),
			new SessionStateCmdletEntry("Get-FarFile", typeof(GetFarFileCommand), Help),
			new SessionStateCmdletEntry("Get-FarItem", typeof(GetFarItemCommand), Help),
			new SessionStateCmdletEntry("Get-FarPath", typeof(GetFarPathCommand), Help),
			new SessionStateCmdletEntry("New-FarEditor", typeof(NewFarEditorCommand), Help),
			new SessionStateCmdletEntry("New-FarFile", typeof(NewFarFileCommand), Help),
			new SessionStateCmdletEntry("New-FarItem", typeof(NewFarItemCommand), Help),
			new SessionStateCmdletEntry("New-FarList", typeof(NewFarListCommand), Help),
			new SessionStateCmdletEntry("New-FarMenu", typeof(NewFarMenuCommand), Help),
			new SessionStateCmdletEntry("New-FarViewer", typeof(NewFarViewerCommand), Help),
			new SessionStateCmdletEntry("Open-FarEditor", typeof(OpenFarEditorCommand), Help),
			new SessionStateCmdletEntry("Open-FarPanel", typeof(OpenFarPanelCommand), Help),
			new SessionStateCmdletEntry("Open-FarViewer", typeof(OpenFarViewerCommand), Help),
			new SessionStateCmdletEntry("Out-FarList", typeof(OutFarListCommand), Help),
			new SessionStateCmdletEntry("Out-FarPanel", typeof(OutFarPanelCommand), Help),
			new SessionStateCmdletEntry("Search-FarFile", typeof(SearchFarFileCommand), Help),
			new SessionStateCmdletEntry("Show-FarMessage", typeof(ShowFarMessageCommand), Help),
			new SessionStateCmdletEntry("Start-FarJob", typeof(StartFarJobCommand), Help),
			new SessionStateCmdletEntry("Start-FarTask", typeof(StartFarTaskCommand), Help),
		});
	}
}
