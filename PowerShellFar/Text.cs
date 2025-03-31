namespace PowerShellFar;

/// <summary>
/// Resource strings.
/// </summary>
static class Res
{
	public const int ConfirmSampleCount = 10;

	public const string
		// main menu
		MenuInvokeCommands /**/ = "&1. Invoke commands",
		MenuInvokeSelected /**/ = "&2. Invoke selected",
		MenuCommandHistory /**/ = "&4. Command history",
		MenuInteractive /*   */ = "&5. Interactive",
		MenuPowerPanel /*    */ = "&6. Power panel",
		MenuTabExpansion /*  */ = "&7. Complete",
		MenuDebugger /*      */ = "&9. Debugger",
		MenuError /*         */ = "&0. Errors",
		MenuHelp /*          */ = "&?. Help",
		// ask
		AskSaveModified = "Would you like to save modified data?",
		// errors
		CanNotClose = "Cannot close the session at this time.",
		InvalidColumnKind = "Invalid column kind: ",
		LogError = "Cannot write to the log; ensure the path is valid and the file is not busy.",
		MaximumPanelColumnCount = "Valid maximum column count should be from 3 to 13.", // _100202_113617
		NeedsEditor = "The current window must be an editor.",
		NotSupportedByProvider = "Operation is not supported by the provider.",
		NoUserMenu = "You did not define your user menu $Psf.UserMenu.\nPlease, see help and example script Profile-.ps1",
		ParameterString = "Parameter must be a string.",
		PropertyIsNotSettableNow = "The property is not settable at this moment.",
		UnknownFileSource = "The file source is unknown or not supported.",
		// others
		BackgroundJobs = "Background jobs",
		Cancel = "Cancel",
		CtrlC = "Cancel key is pressed.",
		Delete = "Delete",
		Stop = "Stop",
		Empty = "Empty",
		InvokeCommands = "Invoke commands",
		Remove = "Remove",
		UIApply = "Apply command",
		NullText = "<null>",

		// History of main input code, general top level code
		History = "PowerShellFar",
		// History of "Apply command", pieces of code based on $_
		HistoryApply = "PowerShellFarApply",
		// History of the host prompt box, any strings
		HistoryPrompt = "PowerShellFarPrompt",
		// History of BP action
		HistoryAction = "PowerShellFarAction",
		// History of BP command, command names
		HistoryCommand = "PowerShellFarCommand",
		// History of BP script, any script paths
		HistoryScript = "PowerShellFarScript",
		// History of BP variable, variable names
		HistoryVariable = "PowerShellFarVariable",

		// Known type names
		MatchInfoTypeName = "Microsoft.PowerShell.Commands.MatchInfo",

		// Main name
		Me = "PowerShellFar";
}

/// <summary>
/// Invariant words and strings.
/// </summary>
static class Word
{
	public const string
		Alignment = "Alignment",
		Definition = "Definition",
		Description = "Description",
		ExecutionContext = "ExecutionContext",
		Expression = "Expression",
		FormatString = "FormatString",
		Id = "Id",
		Id2 = "_id",
		InteractiveSuffix = ".interactive.ps1",
		Key = "Key",
		Kind = "Kind",
		Name = "Name",
		Label = "Label",
		PSModulePath = "PSModulePath",
		Status = "Status",
		Value = "Value",
		Width = "Width";
}
