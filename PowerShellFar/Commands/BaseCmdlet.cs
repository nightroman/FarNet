using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PowerShellFar.Commands;

class BaseCmdlet : PSCmdlet
{
	const string Help = "PowerShellFar.dll-Help.xml";

	internal static void AddCmdlets(InitialSessionState state)
	{
		state.Commands.Add(
		[
			new SessionStateCmdletEntry(AssertFarCommand.MyName, typeof(AssertFarCommand), Help),
			new SessionStateCmdletEntry("Find-FarFile", typeof(FindFarFileCommand), Help),
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
			new SessionStateCmdletEntry("Register-FarCommand", typeof(RegisterFarCommandCommand), Help),
			new SessionStateCmdletEntry("Register-FarDrawer", typeof(RegisterFarDrawerCommand), Help),
			new SessionStateCmdletEntry("Register-FarTool", typeof(RegisterFarToolCommand), Help),
			new SessionStateCmdletEntry("Search-FarFile", typeof(SearchFarFileCommand), Help),
			new SessionStateCmdletEntry("Show-FarMessage", typeof(ShowFarMessageCommand), Help),
			// Far task
			new SessionStateCmdletEntry("Start-FarTask", typeof(StartFarTaskCommand), Help),
			new SessionStateCmdletEntry(InvokeTaskCmd.MyName, typeof(InvokeTaskCmd), Help),
			new SessionStateCmdletEntry(InvokeTaskFun.MyName, typeof(InvokeTaskFun), Help),
			new SessionStateCmdletEntry(InvokeTaskJob.MyName, typeof(InvokeTaskJob), Help),
			new SessionStateCmdletEntry(InvokeTaskRun.MyName, typeof(InvokeTaskRun), Help),
			new SessionStateCmdletEntry(InvokeTaskKeys.MyName, typeof(InvokeTaskKeys), Help),
			new SessionStateCmdletEntry(InvokeTaskMacro.MyName, typeof(InvokeTaskMacro), Help),
			new SessionStateAliasEntry("ps:", InvokeTaskCmd.MyName),
			new SessionStateAliasEntry("fun", InvokeTaskFun.MyName),
			new SessionStateAliasEntry("job", InvokeTaskJob.MyName),
			new SessionStateAliasEntry("run", InvokeTaskRun.MyName),
			new SessionStateAliasEntry("keys", InvokeTaskKeys.MyName),
			new SessionStateAliasEntry("macro", InvokeTaskMacro.MyName),
		]);
	}
}
