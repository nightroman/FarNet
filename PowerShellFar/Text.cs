/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

namespace PowerShellFar
{
	/// <summary>
	/// Resource strings.
	/// </summary>
	static class Res
	{
		public const string
			InvokeSelectedCode = "Invoke selected code",
			BackgroundJobs = "Background jobs",
			// main menu
			MenuInvokeInputCode = "&1. Invoke input code... ", // use right margin spaces
			MenuInvokeSelectedCode = "&2. " + InvokeSelectedCode,
			MenuBackgroundJobs = "&3. " + BackgroundJobs + "...",
			MenuCommandHistory = "&4. Command history...",
			MenuEditorConsole = "&5. Editor console",
			MenuPowerPanel = "&6. Power panel...",
			MenuTabExpansion = "&7. TabExpansion",
			MenuSnapin = "&8. Modules+...",
			MenuDebugger = "&9. Debugger...",
			MenuError = "&0. Errors...",
			MenuHelp = "&-. Help",
			// errors
			AskSaveModified = "Would you like to save modified data?",
			EditorConsoleCannotComplete = "Editor console can't complete the command\nbecause its window is not current at this moment.",
			LogError = "Cannot write to the log; ensure the path is valid and the file is not busy.",
			PropertyIsNotSettable = "Note: this property is not settable, changes will be lost.",
			NeedsEditor = "Editor is not opened or its window is not current.",
			NotSupportedByProvider = "Operation is not supported by the provider.",
			NoUserMenu = "You did not define your user menu $Psf.UserMenu.\nPlease, see help and example script Profile-.ps1",
			PropertyIsNotSettableNow = "The property is not settable at this moment.",
			CanNotClose = "Cannot close the session at this time.",
			MaximumPanelColumnCount = "Valid maximum column count should be from 3 to 13.", // _100202_113617
			// others
			Cancel = "Cancel",
			Delete = "Delete",
			PromptCode = "Enter PowerShell code",
			Remove = "Remove",
			CtrlC = "Cancel key is pressed.",
			// history
			PowerShellFarPrompt = "PowerShellFarPrompt",
			// main name
			Me = "PowerShellFar";
	}

	/// <summary>
	/// Invariand words and strings.
	/// </summary>
	static class Word
	{
		public const string
			ConsoleExtension = ".psfconsole",
			Definition = "Definition",
			Description = "Description",
			ExecutionContext = "ExecutionContext",
			Expression = "Expression",
			Id = "Id",
			Key = "Key",
			Name = "Name",
			Label = "Label",
			PSModulePath = "PSModulePath",
			Status = "Status",
			Type = "Type",
			Value = "Value",
			Width = "Width";
	}
}
