<#
.Synopsis
	Help script, https://github.com/nightroman/Helps
#>

Set-StrictMode -Version 3

### Assert-Far
@{
	command = 'Assert-Far'
	synopsis = @'
	Checks for the conditions and stops invocation if any is false.
'@
	description = @'
	Supported sessions: main (all parameters), async (Value, Eq, Message, Title).

	If the assertion fails, a dialog is shown with several options.
	A running macro, if any, is stopped before showing the dialog.

	Use the parameter Title for simple messages without assertion details
	with only two options Stop and Throw without Ignore, Debug, Edit.

	ACTIONS

		[Stop], [Esc]
			Stop running PowerShell commands.

		[Throw]
			Throw 'Assertion failed.' error.

		[Ignore]
			Continue running commands.

		[Debug]
			Break into an attached debugger, if any.
			Otherwise ask to attach a debugger and repeat.

		[Edit]
			Stop commands and open the editor at the assert.
			(when the source script is available)
'@
	parameters = @{
		Value = @'
		One or more condition values to be checked. If any value is false-y
		(null, 0, empty string, etc.) then an assertion dialog is shown.

		If it is a collection then all items are checked as conditions.

		With Eq, it is the value compared with Eq.
'@
		Eq = @'
		Specifies the value compared with Value by Object.Equals().

		`Assert-Far X -eq Y` is not the same as `Assert-Far (X -eq Y)`.
		The first uses Object.Equals(). The second uses operator `-eq`,
		which is case insensitive, converts types, has different meaning
		when X is collection.
'@
		Message = @'
		Specifies a user friendly message shown on failures or a script block
		invoked on failures in order to get a message.
'@
		Title = @'
		Tells to show the simple dialog with [Stop] and [Throw] buttons and
		specifies its title.
'@
		NoError = 'Asserts $Global:Error is empty.'
		FileDescription = 'Specifies the expected current panel file description.'
		FileName = 'Specifies the expected current panel file name.'
		FileOwner = 'Specifies the expected current file owner.'
		Dialog = 'Checks the current window is dialog.'
		Editor = 'Checks the current window is editor.'
		EditorFileName = 'Checks the current editor file name wildcard.'
		EditorTitle = 'Checks the current editor title wildcard.'
		Panels = 'Checks the current window is panels.'
		Viewer = 'Checks the current window is viewer.'
		Plugin = 'Checks the panel is plugin.'
		Native = 'Checks the panel is not plugin.'
		Passive = 'Tells using the passive panel.'
		DialogTypeId = 'Checks the current window is dialog with the specified type ID.'
		ExplorerTypeId = 'Checks the panel explorer with the specified type ID.'
	}
	examples = @(
		@{ code = {
			# Hardcoded breakpoint
			Assert-Far
		}}
		@{ code = {
			# Single checks
			Assert-Far -Panels
			Assert-Far ($Far.Window.Kind -eq 'Panels')
			Assert-Far $Far.Window.Kind -eq ([FarNet.WindowKind]::Panels)
		}}
		@{ code = {
			# Combined checks
			Assert-Far -Panels -Plugin
			Assert-Far @(
				$Far.Window.Kind -eq 'Panels'
				$Far.Panel.IsPlugin
			)
		}}
		@{ code = {
			# User friendly stop
			Assert-Far -Panels -Message "Run this from panels." -Title Search-Regex.ps1
		}}
	)
}

### Find-FarFile
@{
	command = 'Find-FarFile'
	synopsis = 'Finds the specified panel file and sets it current.'
	description = 'If a panel file is not found the cmdlet writes an error.'
	sets = @{
		Name = 'Find the file by its exact name.'
		Where = 'Find the file using the Boolean script block.'
	}
	parameters = @{
		Name = 'File name to find.'
		Up = 'Tells to search up, not down.'
		Where = 'Boolean script block operating on $_ ~ FarFile.'
	}
}

### New-FarFile
@{
	command = 'New-FarFile'
	synopsis = 'Creates a panel file (custom or from a file system info).'
	parameters = @{
		Description = 'Sets FarFile.Description'
		Owner = 'Sets FarFile.Owner'
		Columns = 'Sets FarFile.Columns'
		Data = 'Sets FarFile.Data'
		Name = 'Sets FarFile.Name'
		Length = 'Sets FarFile.Length'
		CreationTime = 'Sets FarFile.CreationTime'
		LastAccessTime = 'Sets FarFile.LastAccessTime'
		LastWriteTime = 'Sets FarFile.LastWriteTime'
		Attributes = 'Sets FarFile.Attributes'
		File = 'File system info (file or directory).'
		FullName = 'Tells to use the full name for a file system item.'
	}
	inputs = @(
		@{
			type = 'System.String'
			description = 'Strings used as names of new file objects.'
		}
		@{
			type = 'System.IO.FileSystemInfo'
			description = 'File and directory objects which properties are copied to new file objects.'
		}
	)
	outputs = @{
		type = 'FarNet.FarFile'
		description = 'New file objects.'
	}
}

### New-FarItem
@{
	command = 'New-FarItem'
	synopsis = 'Creates an item for menus, list menus and list dialog controls.'
	parameters = @{
		Text = 'Sets FarItem.Text'
		Click = 'Sets FarItem.Click'
		Data = 'Sets FarItem.Data'
		Checked = 'Sets FarItem.Checked'
		Disabled = 'Sets FarItem.Disabled'
		Grayed = 'Sets FarItem.Grayed'
		Hidden = 'Sets FarItem.Hidden'
		IsSeparator = 'Sets FarItem.IsSeparator'
	}
	outputs = @{
		type = 'FarNet.FarItem'
		description = 'A new item for menus and lists.'
	}
}

### Search-FarFile
@{
	command = 'Search-FarFile'
	synopsis = 'Searches module panel files and opens the result panel with found items.'
	description = @'
This command searches for FarNet module panel files.
It is similar to FarNet.Explore `explore:`.

If the panel is not FarNet then the file system is used.
(FarNet.Tools.FileSystemExplorer)

This command is particularly useful with a script filter.
The script is called for each file with two arguments:
$args[0] as file explorer and $args[1] as the file.
The script returns Boolean or equivalent.
See examples.
'@
	parameters = @{
		Mask = 'Far Manager file mask including exclude and regular expression forms.'
		Script = 'Filter script with $args[0] as file explorer and $args[1] as the file.'
		Exclude = 'Mask or script which excludes directories from getting their files.'
		Path = 'Specifies the root directory for the file system search.'
		Directory = 'Tells to include only directories.'
		File = 'Tells to include only files.'
		Bfs = 'Tells to use breadth-first-search. Ignored in XPath searches.'
		Depth = 'Search depth. Zero for just root, negative for unlimited (default).'
		XPath = 'XPath expression text.'
		XFile = 'XPath expression file.'
		Async = 'Tells to search in the background and open the result panel immediately.'
	}
	examples = @{
		code = {
			# find using Mask
			Search-FarFile README*

			# find large files
			Search-FarFile -File -Exclude 'bin,obj' {$args[1].Length -ge 5mb}

			# find empty directories
			Search-FarFile -Directory {param($e, $f) !(Get-ChildItem -LiteralPath "$($e.Location)\$($f.name)")}
		}
	}
}

### Show-FarMessage
@{
	command = 'Show-FarMessage'
	synopsis = 'Shows a message box with one or more choice buttons.'
	description = @'
If there are two or more buttons it returns either the selected button index or -1 on escape,
otherwise nothing is returned, it is used simply to display a message.
'@
	parameters = @{
		Text = 'Message text. Text with long lines or many lines is allowed, but some lines may be not shown.'
		Caption = 'Message caption.'
		Buttons = 'Standard message buttons.'
		Choices = 'User defined choice buttons. On too many choices a message box is converted into a dialog.'
		HelpTopic = 'Help topic.'
		Draw = 'Tells to draw the message box with no buttons and continue. The caller has to redraw or restore the screen.'
		LeftAligned = 'OBSOLETE'
		AlignCenter = 'Tells to center the message lines.'
		KeepBackground = 'Do not redraw the message background.'
		IsError = 'If error type returned by GetLastError is known, the error description will be shown before the message body text.'
		IsWarning = 'Warning message colors are used (white text on red background by default).'
	}
	outputs = @{
		type = '[int] or none'
		description = @'
The selected button index or -1 on escape, or none if the message box has no
choice buttons and just shows a message.
'@
	}
}

### File Cmdlets
$BaseFile = @{
	parameters = @{
		All = 'Tells to get all the panel items.'
		Passive = 'Tells to get items from the passive panel.'
		Selected = 'Tells to get selected panel items or the current one if none is selected.'
	}
}

### Get-FarItem
Merge-Helps $BaseFile @{
	command = 'Get-FarItem'
	synopsis = 'Gets provider items or attached to files data objects from panels.'
	outputs = @{
		type = '[object]'
	}
}

### Get-FarPath
Merge-Helps $BaseFile @{
	command = 'Get-FarPath'
	synopsis = 'Gets the current panel path, selected paths, or all paths.'
	parameters = @{
		Mirror = 'Tells to join the target file names with the opposite panel path.'
	}
	outputs = @{
		type = '[string]'
	}
}

### Text Cmdlets
$BaseText = @{
	parameters = @{
		CodePage = 'Code page identifier.'
		DeleteSource = 'Tells when and how to delete the file when closed.'
		DisableHistory = 'Tells to not add the file to the history.'
		Path = 'The path of a file to be opened.'
		Switching = 'Switching between editor and viewer.'
		Title = 'Window title. The default is the file path.'
	}
}

# editor
$BaseEditor = Merge-Helps $BaseText @{
	parameters = @{
		LineNumber = 'Line number to open the editor at. The first is 1.'
		CharNumber = 'Character number in the line to open the editor at. The first is 1.'
		Host = 'The host instance.'
		IsLocked = 'Sets the lock mode ([CtrlL]).'
	}
}

# misc
$parametersModal = @{
	Modal = "Tells to open modal. By default it is not but it depends on where it is opened."
}

### New-FarEditor
Merge-Helps $BaseEditor @{
	command = 'New-FarEditor'
	synopsis = 'Creates an editor for other settings before opening.'
}

### Open-FarEditor
Merge-Helps $BaseEditor @{
	command = 'Open-FarEditor'
	synopsis = 'Creates and opens an editor.'
	parameters = $parametersModal
}

### New-FarViewer
Merge-Helps $BaseText @{
	command = 'New-FarViewer'
	synopsis = 'Creates a viewer for other settings before opening.'
}

### Open-FarViewer
Merge-Helps $BaseText @{
	command = 'Open-FarViewer'
	synopsis = 'Creates and opens a viewer.'
	parameters = $parametersModal
}

### Base panel
$BasePanel = @{
	parameters = @{
		Title = 'Specifies the panel title.'
		TypeId = 'Specifies the panel type ID which is used to identify the panel by other tools.'
		SortMode = 'Specifies the panel start sort mode.'
		ViewMode = 'Specifies the panel start view mode.'
		Data = @'
Specifies any object which is used later by custom panel event handlers.
'@
		TimerUpdate = @'
Tells to update the panel on timer events and specifies the interval in
milliseconds. Useful for objects changing their properties over time.
'@
		DataId = @'
Specifies the custom data ID used to distinguish files by their data.
The following types can be used:
	String
		Specifies an ID property name.
	ScriptBlock
		Specifies an ID calculated from $_.
'@
	}
}

### Open-FarPanel
Merge-Helps $BasePanel @{
	command = 'Open-FarPanel'
	synopsis = 'Opens the panel.'
	description = 'The panel is opened only when the core gets control.'
	parameters = @{
		InputObject = 'A panel or explorer to be opened or an object which members to be shown.'
		AsChild = 'Tells to open the panel as a child of the current parent.'
	}
	inputs = @{
		type = 'FarNet.Panel'
		description = 'The panel being opened.'
	}
}

### Out-FarPanel
Merge-Helps $BasePanel @{
	command = 'Out-FarPanel'
	synopsis = 'Sends output to a new object panel or appends to the active.'
	description = @'
This command is used in order to create a panel from input objects on the fly.
By default a set of appropriate columns is chosen automatically. In some cases
automatic columns are not effective. Use the property Columns in order to tell
what is needed exactly.
'@
	parameters = @{
		Columns = @'
Custom columns. Each column is either a property name (string) or a column
description (hashtable). A column description table looks like

	@{Expression = ..; Name = ..; Kind = ..; Width = ..; Alignment = ..}

Keys are case insensitive and can be shortened, even to their first letters.

	Expression
		Property name (string) or a calculated property (script block operating
		on input object $_). Name/Label is normally also used for a script block.

		If scripts use variables from the current context, use `GetNewClosure()`.

	Name or Label
		Display name for a value from a script block or alternative name for a
		property. It is used as a panel column title.

	Kind
		Far column kind, e.g. N, O, Z, S, C0, ... C9

	Width
		Far column width: positive: absolute width, negative: percentage.
		Positive widths are ignored if a panel is too narrow to display all
		columns.

	Alignment
		If the width is positive Right alignment can be used. If a panel is too
		narrow to display all columns this option can be ignored.

Column kinds (see Far manual for details):

	N  Name
	O  Owner
	Z  Description
	S  Length
	DC CreationTime
	DM LastWriteTime
	DA LastAccessTime
	C0 Custom
	..
	C9 Custom

Column kind rules:

	A column kind can be specified just once.

	Specify column kinds when you really have to do so. Especially avoid
	C0..C9, let them to be processed automatically.

	C0...C9 must be listed incrementally without gaps. But other kinds between
	them is fine. E.g. C1, C0 or C0, C2 are wrong, C0, N, C1 is correct.

	If a kind is not specified then the next available from the remaining
	default sequence is taken automatically.

Default column kind sequence:

	"N", "Z", "O", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9".
'@
		InputObject = @'
Object(s) to be sent to an object panel. Normally this parameter is not used
directly, instead input objects come from the pipeline.
'@
		ExcludeMemberPattern = 'Regular expression pattern of members to be excluded in a child list panel.'
		HideMemberPattern = 'Regular expression pattern of members to be hidden in a child list panel.'
		Return = 'Tells to return the panel without opening.'
	}

	inputs = @{
		type = '[object]'
		description = 'Any objects to be shown as panel files.'
	}

	outputs = @{
		type = '[PowerShellFar.ObjectPanel]'
		description = 'With Return, the created panel, not yet opened.'
	}

	examples = @(
		@{code={
	# Invoke the commands and compare their panels.

	# Group processes and panel them as it is.
	# Note that the column Group is not very useful.

	Get-Process | Group-Object Name | Out-FarPanel

	# Specify only useful columns Count and Name.
	# The column Count is too wide and not aligned.

	Get-Process | Group-Object Name | Out-FarPanel Count, Name

	# Customize the column Count.
	# The panel looks better now.

	Get-Process | Group-Object Name | Out-FarPanel @{e='Count'; k='S'}, Name
		}}
	)
}

### Menu Cmdlets
$BaseMenu = @{
	parameters = @{
		Title = 'Sets IAnyMenu.Title'
		Items = 'Items to add to IAnyMenu.Items'
		AutoAssignHotkeys = 'Sets IAnyMenu.AutoAssignHotkeys'
		Bottom = 'Sets IAnyMenu.Bottom'
		HelpTopic = 'Sets IAnyMenu.HelpTopic'
		NoShadow = 'Sets IAnyMenu.NoShadow'
		Selected = 'Sets IAnyMenu.Selected'
		SelectLast = 'Sets IAnyMenu.SelectLast'
		ShowAmpersands = 'Sets IAnyMenu.ShowAmpersands'
		WrapCursor = 'Sets IAnyMenu.WrapCursor'
		X = 'Sets IAnyMenu.X coordinate.'
		Y = 'Sets IAnyMenu.Y coordinate.'
	}
}

### New-FarMenu
Merge-Helps $BaseMenu @{
	command = 'New-FarMenu'
	synopsis = 'Creates a menu with some properties.'
	parameters = @{
		ReverseAutoAssign = 'Sets IMenu.ReverseAutoAssign'
		ChangeConsoleTitle = 'Sets IMenu.ChangeConsoleTitle'
		NoBox = 'Sets IMenu.NoBox'
		NoMargin = 'Sets IMenu.NoMargin'
		SingleBox = 'Sets IMenu.SingleBox'
		Show = 'Tells to show immediately. Nothing is returned, actions are done by item event handlers.'
	}
	outputs = @{
		type = 'FarNet.IMenu or none'
		description = 'A new menu object or none if Show is used.'
	}
}

### List Cmdlets
$FarList = Merge-Helps $BaseMenu @{
	parameters = @{
		AutoSelect = 'Sets IListMenu.AutoSelect'
		Incremental = 'Sets IListMenu.Incremental'
		IncrementalOptions = 'Sets IListMenu.IncrementalOptions'
		ScreenMargin = 'Sets IListMenu.ScreenMargin'
		UsualMargins = 'Sets IListMenu.UsualMargins'
		Popup = 'Popup-list style. Uses $Psf.Settings.Popup* options.'
	}
}

### New-FarList
Merge-Helps $FarList @{
	command = 'New-FarList'
	synopsis = 'Creates a list with some properties.'
	outputs = @{
		type = 'FarNet.IListMenu'
		description = 'A new list menu object.'
	}
}

### Out-FarList
Merge-Helps $FarList @{
	command = 'Out-FarList'
	synopsis = 'Shows a list of input objects and returns selected.'
	parameters = @{
		InputObject = @'
		Objects to show as list items.

		Objects FarItem are used as is and shown as their property
		Text. Their property Click may be used for custom actions.

		Other objects are shown as SetItem with Text as ToString() or
		specified by the parameter Text. The Data is set to the input
		object, to be returned.
'@
		Text = @'
		A property name or script to get the list item text. Example:
		'FullName' or {$_.FullName} to use the property FullName.
'@
	}
	inputs = @{
		type = '[object]'
		description = 'Any objects.'
	}
	outputs = @(
		@{
			type = 'null'
			description = 'Nothing is selected.'
		}
		@{
			type = '[object]'
			description = 'The selected input object.'
		}
	)
}

### --Register
$BaseRegister = @{
	parameters = @{
		Id = 'The action GUID.'
		Name = 'The name for UI.'
	}
}

### Register-FarCommand
Merge-Helps $BaseRegister @{
	command = 'Register-FarCommand'
	synopsis = 'Registers the command handler invoked from the command line by its prefix.'
	description = @'
	This command wraps IModuleManager.RegisterCommand, see FarNet API.
'@
	parameters = @{
		Prefix = @'
		Specifies the command prefix.
'@
		Handler = @'
		Processes the command which text is provided as `$_.Command`.
'@
	}
	links = @(
		@{ text = 'https://github.com/nightroman/FarNet/blob/main/Samples/Tests/Test-RegisterCommand.far.ps1' }
	)
}

### Register-FarDrawer
Merge-Helps $BaseRegister @{
	command = 'Register-FarDrawer'
	synopsis = 'Registers the editor drawer handler.'
	description = @'
	This command wraps IModuleManager.RegisterDrawer, see FarNet API.
'@
	parameters = @{
		Mask = @'
		Specifies the Far Manager file mask.
'@
		Priority = @'
		Specifies color priority.
'@
		Handler = @'
		Processes text rendering events with:
		$this - current editor [FarNet.IEditor]
		$_ [FarNet.ModuleDrawerEventArgs]:
		$_.Colors - result color collection
		$_.Lines - lines to get colors for
		$_.StartChar - the first character
		$_.EndChar - after the last character
'@
	}
	links = @(
		@{ text = 'https://github.com/nightroman/FarNet/blob/main/Samples/Tests/Test-RegisterDrawer.far.ps1' }
	)
}

### Register-FarTool
Merge-Helps $BaseRegister @{
	command = 'Register-FarTool'
	synopsis = 'Registers the tool handler invoked from one of Far menus.'
	description = @'
	This command wraps IModuleManager.RegisterTool, see FarNet API.
'@
	parameters = @{
		Options = @'
		The tool options with at least one target area specified.
'@
		Handler = @'
		Processes the command which text is provided as `$_.Command`.
'@
	}
	links = @(
		@{ text = 'https://github.com/nightroman/FarNet/blob/main/Samples/Tests/Test-RegisterTool.far.ps1' }
	)
}

### --Task
### Start-FarTask
@{
	command = 'Start-FarTask'
	synopsis = 'Starts the script task.'
	description = @'
	This cmdlet starts the script task. The script runs in a new session
	asynchronously without blocking the main thread. It uses special job
	blocks for accessing FarNet API in the main session.

	INPUT

	The task script and jobs may use the shared hashtable $Data. It is set
	explicitly by the parameter Data and implicitly by script parameters.

	If Script uses parameters, they may be specified on Start-FarTask calls.
	Known issue: avoid switch parameters or specify after parameter Script.
	The specified parameters are also added to the shared hashtable $Data.

	OUTPUT

	The cmdlet returns nothing by default and the task script output is
	ignored. Use the switch AsTask in order to return the started task.
	The task result is the task script output presented as [object[]].

	LOCATION

	The task current location is the caller file system current location.
	The task may change it, this does not affect anything else.

	Task jobs current locations are the main session current location.
	Jobs should not change it without restoring the original.

	JOBS AND MACROS

	`ps:` and `job` may be used in async and main sessions.
	`run`, `keys`, `macro` are used in async sessions only.

	ps: {...}

		This job prints its commands output to the console.

		Use $Var.<name> for getting or setting the task variables.

	job {...}

		This job may output data as usual. If an object is a task then this
		task is awaited and its result is returned, if not null.

		Use $Var.<name> for getting or setting the task variables.

	run {...}

		This job starts some modal UI as the last command and immediately
		returns to the task with modal UI still running. Output is ignored.

		Use $Var.<name> for getting or setting the task variables.

	keys <key> [<key> ...]

		This command invokes the specified keys.

	macro <code>

		This command invokes the specified macro.

	DEBUGGING AND STEPPING

	AddDebugger and Step enable debugging and require Add-Debugger.ps1 in the path.
	Get Add-Debugger.ps1 -- https://www.powershellgallery.com/packages/Add-Debugger
'@
	parameters = @{
		Script = @'
		Specifies the task as script block or file name or script code.

		File names are full or relative paths or just names in the path.
		File names should end with ".ps1".

		Strings not ending with ".ps1" are treated as code and compiled to
		script blocks.
'@
		AsTask = @'
		Tells to return the started task.
		The task result is the task script output presented as [object[]].
'@
		Data = @'
		The list of variable names or hashtables added to the shared hashtable $Data.

		String items are names of existing variables added to $Data.

		Other items are hashtables merged into $Data.
'@
		AddDebugger = @'
		Tells to use Add-Debugger.ps1 and specifies its parameters hashtable.
		Use null or empty hashtable for defaults.
		Example:

			Start-FarTask ... -AddDebugger @{
				Path = "$env:TEMP\debug-1.log"
				Context = 10
			}

		Ensure some breakpoints in the task script or use Wait-Debugger or
		use Step to break at jobs and macros. Otherwise, the debugger will
		not stop.
'@
		Step = @'
		Tells to use Add-Debugger.ps1 and sets breakpoints at jobs and macros:
		`job`, `run`, `ps:`, `keys`, `macro`.
'@
	}
	outputs = @{
		type = 'System.Threading.Tasks.Task[object[]]'
		description = 'With AsTask, the started task.'
	}
	links = @(
		@{ text = 'Samples -- https://github.com/nightroman/FarNet/tree/main/Samples/FarTask' }
		@{ text = 'Invoke-FarTaskCmd' }
		@{ text = 'Invoke-FarTaskJob' }
		@{ text = 'Invoke-FarTaskRun' }
		@{ text = 'Invoke-FarTaskKeys' }
		@{ text = 'Invoke-FarTaskMacro' }
	)
}

### Invoke-FarTaskCmd
@{
	command = 'Invoke-FarTaskCmd'
	synopsis = '(ps:) Invokes task script as from command line.'
	description = @'
	Invokes the script as if it is typed in the command line and prints its
	output to the console.

	Supported sessions: async, main.
'@
	parameters = @{
		Script = 'Script block.'
	}
	outputs = @(
		@{
			type = 'None'
			description = 'Output is printed to the console.'
		}
	)
    examples = @(
        @{
            code = {
            	# Prints the active panel path.
            	ps: {
            		$Far.Panel.CurrentDirectory
            	}
            }
        }
    )
}

### Invoke-FarTaskJob
@{
	command = 'Invoke-FarTaskJob'
	synopsis = '(job) Invokes task script with output.'
	description = @'
	Invokes the script and returns its output objects. If an object is a task
	then this task is awaited and its result is returned, if not null.

	Supported sessions: async, main.
'@
	parameters = @{
		Script = 'Script block.'
	}
	outputs = @(
		@{
			type = 'PSObject'
			description = 'Script results.'
		}
	)
    examples = @(
        @{
            code = {
            	# Returns the active panel path.
            	job {
            		$Far.Panel.CurrentDirectory
            	}
            }
        }
    )
}

### Invoke-FarTaskRun
@{
	command = 'Invoke-FarTaskRun'
	synopsis = '(run) Invokes task script.'
	description = @'
	Runs the script which starts some modal UI and immediately returns with the
	modal UI still running. Output is ignored.

	Then the caller may operate on the running modal UI. This is useful for
	testing expected results in UI or doing something else more practical.

	Supported sessions: async, main (*).

	(*) Experiment. In main session `run` posts the script with scope variables
	for later. The script runs as soon as the current pipeline finishes and the
	window is not modal. E.g. this allows opening result panels.
'@
	parameters = @{
		Script = 'Script block.'
	}
	outputs = @(
		@{
			type = 'None'
			description = 'Output is ignored.'
		}
	)
	examples = @(
        @{
            code = {
            	# Shows some error message box.
            	run {
            		$Far.Message('Cannot find file.', 'Error', 'Warning, Ok')
            	}
            }
        }
	)
}

### Invoke-FarTaskKeys
@{
	command = 'Invoke-FarTaskKeys'
	synopsis = '(keys) Posts task keys.'
	description = @'
	Posts the specified keys as if they are typed and awaits them, then the
	caller continues. Keys are Far Manager key names. Keys are specified as
	separate arguments.

	Supported sessions: async only.
'@
	parameters = @{
		Keys = 'One or more keys as separate arguments.'
	}
	outputs = @(
		@{
			type = 'None'
		}
	)
	examples = @(
		@{
			code = {
				# Enters "Hello"
				keys H e l l o Enter
			}
		}
	)
}

### Invoke-FarTaskMacro
@{
	command = 'Invoke-FarTaskMacro'
	synopsis = '(macro) Posts task macro.'
	description = @'
	Posts the specified Lua macro and awaits it, then the caller continues.

	Supported sessions: async only.
'@
	parameters = @{
		Macro = 'Lua macro.'
	}
	outputs = @(
		@{
			type = 'None'
		}
	)
	examples = @(
		@{
			code = {
				# Shows Lua message box.
				macro 'far.Message("Hello")'
			}
		}
	)
}
