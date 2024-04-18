
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PowerShellFar.Commands;

/// <summary>
/// PowerShellFar cmdlet.
/// </summary>
class BaseCmdlet : PSCmdlet
{
	const string Help = "PowerShellFar.dll-Help.xml";

	/// <summary>
	/// Adds cmdlets to the initial state.
	/// </summary>
	internal static void AddCmdlets(InitialSessionState state)
	{
		state.Commands.Add(new SessionStateCmdletEntry[]
		{
			new(AssertFarCommand.MyName, typeof(AssertFarCommand), Help),
			new("Find-FarFile", typeof(FindFarFileCommand), Help),
			new("Get-FarItem", typeof(GetFarItemCommand), Help),
			new("Get-FarPath", typeof(GetFarPathCommand), Help),
			new("New-FarEditor", typeof(NewFarEditorCommand), Help),
			new("New-FarFile", typeof(NewFarFileCommand), Help),
			new("New-FarItem", typeof(NewFarItemCommand), Help),
			new("New-FarList", typeof(NewFarListCommand), Help),
			new("New-FarMenu", typeof(NewFarMenuCommand), Help),
			new("New-FarViewer", typeof(NewFarViewerCommand), Help),
			new("Open-FarEditor", typeof(OpenFarEditorCommand), Help),
			new("Open-FarPanel", typeof(OpenFarPanelCommand), Help),
			new("Open-FarViewer", typeof(OpenFarViewerCommand), Help),
			new("Out-FarList", typeof(OutFarListCommand), Help),
			new("Out-FarPanel", typeof(OutFarPanelCommand), Help),
			new("Register-FarCommand", typeof(RegisterFarCommandCommand), Help),
			new("Register-FarDrawer", typeof(RegisterFarDrawerCommand), Help),
			new("Register-FarTool", typeof(RegisterFarToolCommand), Help),
			new("Search-FarFile", typeof(SearchFarFileCommand), Help),
			new("Show-FarMessage", typeof(ShowFarMessageCommand), Help),
			new("Start-FarJob", typeof(StartFarJobCommand), Help),
			new("Start-FarTask", typeof(StartFarTaskCommand), Help),
		});
	}
}
