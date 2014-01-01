
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
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
		const string Help = "PowerShellFar.dll-Help.xml";
		/// <summary>
		/// Adds cmdlets to the initial state.
		/// </summary>
		internal static void AddCmdlets(InitialSessionState state)
		{
			state.Commands.Add(new SessionStateCmdletEntry[] {
				new SessionStateCmdletEntry(AssertFarCommand.MyName, typeof(Commands.AssertFarCommand), Help),
				new SessionStateCmdletEntry("Find-FarFile", typeof(Commands.FindFarFileCommand), Help),
				new SessionStateCmdletEntry("Get-FarFile", typeof(Commands.GetFarFileCommand), Help),
				new SessionStateCmdletEntry("Get-FarItem", typeof(Commands.GetFarItemCommand), Help),
				new SessionStateCmdletEntry("Get-FarPath", typeof(Commands.GetFarPathCommand), Help),
				new SessionStateCmdletEntry("Invoke-FarStepper", typeof(Commands.InvokeFarStepperCommand), Help),
				new SessionStateCmdletEntry("New-FarEditor", typeof(Commands.NewFarEditorCommand), Help),
				new SessionStateCmdletEntry("New-FarFile", typeof(Commands.NewFarFileCommand), Help),
				new SessionStateCmdletEntry("New-FarItem", typeof(Commands.NewFarItemCommand), Help),
				new SessionStateCmdletEntry("New-FarList", typeof(Commands.NewFarListCommand), Help),
				new SessionStateCmdletEntry("New-FarMenu", typeof(Commands.NewFarMenuCommand), Help),
				new SessionStateCmdletEntry("New-FarViewer", typeof(Commands.NewFarViewerCommand), Help),
				new SessionStateCmdletEntry("Open-FarEditor", typeof(Commands.OpenFarEditorCommand), Help),
				new SessionStateCmdletEntry("Open-FarPanel", typeof(Commands.OpenFarPanelCommand), Help),
				new SessionStateCmdletEntry("Open-FarViewer", typeof(Commands.OpenFarViewerCommand), Help),
				new SessionStateCmdletEntry("Out-FarList", typeof(Commands.OutFarListCommand), Help),
				new SessionStateCmdletEntry("Out-FarPanel", typeof(Commands.OutFarPanelCommand), Help),
				new SessionStateCmdletEntry("Search-FarFile", typeof(Commands.SearchFarFileCommand), Help),
				new SessionStateCmdletEntry("Show-FarMessage", typeof(Commands.ShowFarMessageCommand), Help),
				new SessionStateCmdletEntry("Start-FarJob", typeof(Commands.StartFarJobCommand), Help)
			});
		}
	}
}
