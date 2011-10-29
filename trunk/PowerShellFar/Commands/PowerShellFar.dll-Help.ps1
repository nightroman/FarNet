
<#
.Synopsis
	Help script (https://github.com/nightroman/Helps)
#>

Set-StrictMode -Version 2

### Assert-Far
@{
	command = 'Assert-Far'
	synopsis = @'
Checks for the condition(s) and stops the pipeline with a message if any condition is not evaluated to true.
'@
	description = @'
If the assertion fails then a message is shown and the PipelineStoppedException exception is thrown after that.
A running macro, if any, is stopped before showing the message.
'@,
	@'
If the message Title is provided then just a simple message is shown on failures, all the assertion details are omitted.
This mode is suitable for production scripts.
'@
	parameters = @{
		Conditions = 'A value or an array of values to be checked.'
		Message = 'The message to display on failure or a script block to invoke and get the message.'
		Title = 'The title of a simple message designed for production scripts.'
		FileDescription = 'Asserts the current panel file description.'
		FileName = 'Asserts the current panel file name.'
		FileOwner = 'Asserts the current file owner.'
		Dialog = 'Checks the current window is dialog.'
		Editor = 'Checks the current window is editor.'
		Panels = 'Checks the current window is panels.'
		Viewer = 'Checks the current window is viewer.'
		Plugin = 'Checks the active panel is plugin.'
		Plugin2 = 'Checks the passive panel is plugin.'
		Native = 'Checks the active panel is native (not plugin).'
		Native2 = 'Checks the passive panel is native (not plugin).'
	}
	inputs = @()
	outputs = @()
	examples = @(
		@{
			code = @'
# Hardcoded breakpoint
Assert-Far
'@
		}
		@{
			code = @'
# Single checks
Assert-Far -Panels
Assert-Far -Plugin
Assert-Far ($Far.Window.Kind -eq 'Panels')
'@
		}
		@{
			code = @'
# Combined checks
Assert-Far -Panels -Plugin
Assert-Far -Panels ($Far.Panel.IsPlugin)
Assert-Far @(
	$Far.Window.Kind -eq 'Panels'
	$Far.Panel.IsPlugin
)
'@
		}
		@{
			code = @'
# User friendly error message. Mind use of -Message and -Title with switches:
Assert-Far -Panels -Message "Run this script from panels." -Title "Search-Regex"
Assert-Far ($Far.Window.Kind -eq 'Panels') "Run this script from panels." "Search-Regex"
'@
		}
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
	inputs = @()
	outputs = @()
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
	inputs = @()
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
	inputs = @()
	outputs = @()
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
	inputs = @()
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
	synopsis = 'Starts a new background job (not classic PowerShell job).'
	description = @'
It helps to create a background job with available parameters. Note:
PowerShellFar background jobs are simple jobs that use PowerShell engine
only and oriented for no output or formatted text output. In contrast,
standard PowerShell background jobs require WSMan and output objects.
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
	inputs = @()
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
	inputs = @()
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
		Path = 'The path of a file to be opened.'
		Title = 'Window title. The default is the file path.'
		DeleteSource = 'Tells when and how to delete the file when closed.'
		DisableHistory = 'Tells to not add the file to the history.'
		Switching = 'Switching between editor and viewer.'
	}
	inputs = @()
	outputs = @()
}

# editor
$BaseEditor = Merge-Helps $BaseText @{
	parameters = @{
		LineNumber = 'Line number to open the editor at. The first is 1.'
		CharNumber = 'Character number in the line to open the editor at. The first is 1.'
		Host = 'The host instance.'
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

### Panel Cmdlets
$BasePanel = @{
	parameters = @{
		TypeId = 'Panel type ID.'
		Title = 'Panel title.'
		SortMode = 'Panel sort mode.'
		ViewMode = 'Panel view mode.'
		IdleUpdate = 'Tells to update data periodically when idle.'
		DataId = 'Custom data ID to distinguish files by data.'
		Data = 'Attached user data.'
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
	outputs = @()
}

### Out-FarPanel
Merge-Helps $BasePanel @{
	command = 'Out-FarPanel'
	synopsis = 'Sends output to a new or appends to the active object panel.'
	parameters = @{
		Columns = 'Custom columns (names or special hash tables).',
		@'
Use property names to specify columns or hash tables to describe columns in details,
see [Meta] about hash tables and PanelPlan.Columns about column types.
'@
		InputObject = 'Object(s) to be sent to an object panel.'
		ExcludeMemberPattern = 'Regular expression pattern of members to be excluded in a child list panel.'
		HideMemberPattern = 'Regular expression pattern of members to be hidden in a child list panel.'
		Append = 'Tells to append objects to the active object panel. All others options are ignored.'
	}
	inputs = @{
		type = '[object]'
		description = 'Any objects to be shown as panel files.'
	}
	outputs = @()
}

### Menu Cmdlets
$BaseMenu = @{
	parameters = @{
		Title = 'Sets IAnyMenu.Title'
		Items = 'Items to add to IAnyMenu.Items'
		AutoAssignHotkeys = 'Sets IAnyMenu.AutoAssignHotkeys'
		Bottom = 'Sets IAnyMenu.Bottom'
		HelpTopic = 'Sets IAnyMenu.HelpTopic'
		Selected = 'Sets IAnyMenu.Selected'
		SelectLast = 'Sets IAnyMenu.SelectLast'
		ShowAmpersands = 'Sets IAnyMenu.ShowAmpersands'
		WrapCursor = 'Sets IAnyMenu.WrapCursor'
		X = 'Sets IAnyMenu.X coordinate.'
		Y = 'Sets IAnyMenu.Y coordinate.'
	}
	inputs = @()
}

### New-FarMenu
Merge-Helps $BaseMenu @{
	command = 'New-FarMenu'
	synopsis = 'Creates a menu with some properties.'
	parameters = @{
		ReverseAutoAssign = 'Sets IMenu.ReverseAutoAssign'
		ChangeConsoleTitle = 'Sets IMenu.ChangeConsoleTitle'
		Show = 'Tells to show immediately. In this case nothing is returned and all actions are done by item event handlers.'
	}
	outputs = @{
		type = 'FarNet.IMenu or none'
		description = 'A new menu object or none if the Show switch is used.'
	}
}

### List Cmdlets
$FarList = Merge-Helps $BaseMenu @{
	parameters = @{
		AutoSelect = 'Sets IListMenu.AutoSelect'
		Filter = 'Sets IListMenu.Filter'
		FilterHistory = 'Sets IListMenu.FilterHistory'
		FilterKey = 'Sets IListMenu.FilterKey'
		FilterOptions = 'Sets IListMenu.FilterOptions'
		FilterRestore = 'Sets IListMenu.FilterRestore'
		Incremental = 'Sets IListMenu.Incremental'
		IncrementalOptions = 'Sets IListMenu.IncrementalOptions'
		NoShadow = 'Sets IListMenu.NoShadow'
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
