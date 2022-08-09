<#
.Synopsis
	Help script (https://github.com/nightroman/Helps)
#>

Set-StrictMode -Version Latest

### Assert-Far
@{
	command = 'Assert-Far'
	synopsis = @'
Checks for the conditions and stops invocation if any of them is evaluated to false.
'@
	description = @'
If the assertion fails, a dialog is shown with several options.
A running macro, if any, is stopped before showing the dialog.

Use the parameter Title for simple messages without assertion details
with only two options: Stop and Throw, without Edit and Debug.
'@
	parameters = @{
		Value = @'
One or more condition values to be checked. If any value is evaluated to false
by PowerShell (null, 0, empty string, etc.) then an assertion dialog is shown.

If it is a collection then all items are checked as conditions.

With Eq, it is the value compared with Eq.
'@
		Eq = @'
Specifies the value compared with Value by Object.Equals().

`Assert-Far X -eq Y` is not the same as `Assert-Far (X -eq Y)`. The first
uses Object.Equals(). The second uses the PowerShell operator -eq, which is
case insensitive, converts types, has different meaning when X is collection.
'@
		Message = @'
Specifies a user friendly message shown on failures or a script block invoked
on failures in order to get a message.
'@
		Title = @'
Specifies a message box title and tells to show a simplified message box with
less options and diagnostics. Such a dialog normally tells what to do instead
of where the problem is.
'@
		FileDescription = 'Specifies the expected current panel file description.'
		FileName = 'Specifies the expected current panel file name.'
		FileOwner = 'Specifies the expected current file owner.'
		Dialog = 'Checks the current window is dialog.'
		Editor = 'Checks the current window is editor.'
		EditorFileName = 'Checks the current editor file name wildcard.'
		EditorTitle = 'Checks the current editor title wildcard.'
		Panels = 'Checks the current window is panels.'
		Viewer = 'Checks the current window is viewer.'
		Plugin = 'Checks the active panel is plugin.'
		Plugin2 = 'Checks the passive panel is plugin.'
		Native = 'Checks the active panel is not plugin.'
		Native2 = 'Checks the passive panel is not plugin.'
		DialogTypeId = 'Checks the current window is dialog with the specified type ID.'
		ExplorerTypeId = 'Checks the active panel explorer with the specified type ID.'
		ExplorerTypeId2 = 'Checks the passive panel explorer with the specified type ID.'
	}

	examples = @(
		@{code={
	# Hardcoded breakpoint
	Assert-Far
		}}
		@{code={
	# Single checks
	Assert-Far -Panels
	Assert-Far ($Far.Window.Kind -eq 'Panels')
	Assert-Far $Far.Window.Kind -eq ([FarNet.WindowKind]::Panels)
		}}
		@{code={
	# Combined checks
	Assert-Far -Panels -Plugin
	Assert-Far @(
		$Far.Window.Kind -eq 'Panels'
		$Far.Panel.IsPlugin
	)
		}}
		@{code={
	# User friendly error message
	Assert-Far -Panels -Message "Run this script from panels." -Title Search-Regex.ps1
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
	synopsis = 'Searches files in the panel and opens the result panel with found items.'
	parameters = @{
		Mask = 'Classic Far Manager file mask including exclude and regular expression forms.'
		Script = 'Search script. Variables: $this is the explorer providing the file, $_ is the file.'
		XPath = 'XPath expression text.'
		XFile = 'XPath expression file.'
		Depth = 'Search depth. 0: ignored; negative: unlimited.'
		Directory = 'Tells to include directories into the search process and results.'
		Recurse = 'Tells to search through all directories and sub-directories.'
		Asynchronous = 'Tells to performs the search in the background and to open the result panel immediately.'
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
		LeftAligned = 'Tells to left align the message lines.'
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

### Start-FarJob
@{
	command = 'Start-FarJob'
	synopsis = 'Starts a new background job (not native PowerShell job).'
	description = @'
It starts a new background job with the specified arguments or parameters.

Far jobs run in separated workspaces in the same process. They take live input
objects and may return live output. (Compare with PowerShell jobs: they run in
separate processes and deal with serialized input and output.)
'@
	parameters = @{
		Command = 'A command name or a script block.'
		Parameters = 'Command parameters. IDictionary for named parameters, IList for arguments, or a single argument.'
		Name = 'Job friendly name to display.'
		Output = 'Tells to start and return the job with exposed Output. Dispose() has to called when the job is done.'
		Return = 'Returns not yet started job with exposed Output. StartJob() and Dispose() are called explicitly.'
		Hidden = 'Started job is not returned, not shown in the list, output is discarded and succeeded job is disposed.',
		'If the job fails or finishes with errors it is included in the list so that errors can be investigated.',
		'For a hidden job parameters Output, Return, and KeepSeconds are ignored.'
		KeepSeconds = 'Tells to keep succeeded job only for specified number of seconds.',
		'Set 0 to remove the succeeded job immediately.',
		'Jobs with errors are not removed automatically, you should remove them from the list.',
		'Stopwatch is started when the first job notification is shown in the console title.'
	}
	outputs = @{
		type = 'PowerShellFar.Job'
		description = 'A new not yet started job if the Return switch is used, otherwise nothing is returned.'
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

### Get-FarFile
Merge-Helps $BaseFile @{
	command = 'Get-FarFile'
	synopsis = 'Gets the current panel file, selected files, or all files.'
	outputs = @{
		type = 'FarNet.FarFile'
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
		InputObject = 'Object to be represented as a list item.'
		Text = @'
A property name or a script to get the FarItem.Text text of a list item.
Example: 'FullName' or {$_.FullName} tell to use a property FullName.
'@
	}
	inputs = @{
		type = '[object]'
		description = 'Any objects.'
	}
	outputs = @{
		type = '[object] or none'
		description = 'One of the input objects selected by a user or none if nothing is selected.'
	}
}

### Start-FarTask
@{
	command = 'Start-FarTask'
	synopsis = 'Starts the script task.'
	description = @'
	This cmdlet starts the specified script task. File script parameters are
	defined in the script and specified for Start-FarTask as its own. Known
	issue: switch parameters must be specified after Script.

	If the script is a script block or code then parameters are not supported.
	Use the parameter Data in order to import specified variables to the task
	automatic hashtable $Data.

	The script is invoked in a new runspace asynchronously without blocking the
	main thread. The code must not work with FarNet directly, it should use job
	blocks instead. Jobs are invoked in the main session.

	The task and jobs may exchange data using the automatic hashtable $Data.

	The cmdlet returns nothing by default and the script output is ignored. Use
	the switch AsTask in order to return the started task. Use it in a calling
	async scenario and get the script output as the task result, object[].

	JOBS AND MACROS

	job [-Arguments ...] [-Script] {...}

		This job works with FarNet and may output data as usual. Special case:
		if the output is a task then this task is awaited and its result is
		returned instead.

	ps: [-Arguments ...] [-Script] {...}

		This job is used for console output as if its commands are invoked from
		the command line. It returns nothing because the output is sent to the
		console.

	run [-Arguments ...] [-Script] {...}

		This job is used to run modal UI without blocking the task.
		It is useful for automation and tests. Output is ignored.

	keys <keys> [<keys> [...]]

		This command invokes the specified keys.
		Arguments are concatenated with spaces.

	macro <code>

		This command invokes the specified macro.
'@
	parameters = @{
		Script = 'Specifies the task as script file, block, or code.'
		AsTask = 'Tells to return the started task.'
		Confirm = @'
Tells to confirm steps before invoking using dialogs.
Use it for troubleshooting, demonstrations, and etc.
'@
		Data = @'
Specifies variables to import from the current session to the task $Data.
Notes: (1) specify variable names, not values; (2) variables must exist.
'@
	}

	outputs = @{
		type = 'System.Threading.Tasks.Task[object[]]'
		description = 'With AsTask, the started task.'
	}

	links = @(
		@{ text = 'Samples -- https://github.com/nightroman/FarNet/tree/master/PowerShellFar/Samples/FarTask' }
	)
}
