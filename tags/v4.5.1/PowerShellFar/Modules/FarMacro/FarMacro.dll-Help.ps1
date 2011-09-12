
$BaseMacroParameters = @{ ###
	Area = 'See Macro.Area'
	Name = 'See Macro.Name'
	Sequence = 'See Macro.Sequence'
	Description = 'See Macro.Description'
	CommandLine = 'See Macro.CommandLine'
	SelectedText = 'See Macro.SelectedText'
	SelectedItems = 'See Macro.SelectedItems'
	PanelIsPlugin = 'See Macro.PanelIsPlugin'
	ItemIsDirectory = 'See Macro.ItemIsDirectory'
	SelectedItems2 = 'See Macro.SelectedItems2'
	PanelIsPlugin2 = 'See Macro.PanelIsPlugin2'
	ItemIsDirectory2 = 'See Macro.ItemIsDirectory2'
	EnableOutput = 'See Macro.EnableOutput'
	DisablePlugins = 'See Macro.DisablePlugins'
	RunAfterFarStart = 'See Macro.RunAfterFarStart'
}

$FarMacroType = @{
	type = '[FarNet.FarMacro]'
	description = 'Macro instances.'
}

@{
	command = 'New-FarMacro' ###
	synopsis = 'Creates a new macro. Use Set-FarMacro or $Far.Macro.Install() to save it.'
	parameters = $BaseMacroParameters
	inputs = @()
	outputs = @()
}

@{
	command = 'Set-FarMacro' ###
	synopsis = 'Installs one (using properties) or more (using properties or macro object(s)) macros.'
	parameters = $BaseMacroParameters + @{
		InputObject = 'Input macro instances.'
	}
	inputs = $FarMacroType
	outputs = @()
}

@{
	command = 'Edit-FarMacro' ###
	synopsis = 'Opens the editor with the macro sequence and install the macro on saving if its syntax is correct.'
	description = @'
Opens the editor with the macro sequence. Syntax highlighting should work if
the Colorer plugin is used. On saving, if syntax is valid, the macro is
installed and ready to use. On syntax errors an error message is shown and the
cursor is set to the error position. On exit, if syntax is not valid, you are
prompted to continue or discard the changes. NOTE: in this way you only edit
the sequence, other macro options remains default for a new macro and not
changed for an existing.

There are three modes depending on parameter sets:
'@
	sets = @{
		Macro = 'Based on a macro object.'
		Name = 'Based on an existing or a new macro specified by names.'
		File = 'Based on an existing or a new macro file. It simply edits the file and checks syntax on saving.'
	}
	parameters = @{
		Name = 'The key name. It is corrected to the standard form.'
		Area = 'The macro area. If it is not set then the current Dialog, Editor, Shell, or Viewer area is assumed.'
		InputObject = 'Input macro instances.'
		Macro = 'The macro instance which sequence is edited.'
		FilePath = 'The path of a file with a macro sequence to edit.'
		Panel = 'A panel to be updated on saving.'
	}
	inputs = $FarMacroType
	outputs = @()
}

@{
	provider = 'FarMacro' ###
	drives = 'FarMacro:'
	synopsis = 'Far Manager macros provider.'
	description = @'
The FarMacro provider lets you view the macros in PowerShell as though they
were on a file system drive.
'@
	capabilities = 'ShouldProcess'
	tasks = @(
		@{
			title = 'Go to the FarMacro drive and get its items'
			examples = @{
				code = {
					Set-Location FarMacro:
					Get-ChildItem
				}
			}
		}
		@{
			title = 'Get macros in an area'
			description = 'Type the drive name and the area name.'
			examples = @{
				code = {
					Get-ChildItem FarMacro:\Shell
				}
			}
		}
	)
}
