
<#
.SYNOPSIS
	Main startup code (example).
	Author: Roman Kuzmin

.DESCRIPTION
	Configuration: startup code to load this profile:
	. Profile-.ps1

	See API: Action [PowerShellFar.Actor]
	See help: Profile-.ps1, Global objects, Plugin settings
#>

### Error action: 'Stop' is recommended to stop on errors immediately
$ErrorActionPreference = 'Stop'

### Aliases
Set-Alias bp Debug-Msg- -Description 'Hardcoded breakpoint'
Set-Alias fff Find-FarFile -Description 'Finds the panel file'
Set-Alias gff Get-FarFile -Description 'Gets the panel file(s)'
Set-Alias gfh Get-FarHelp -Description 'Gets command details'
Set-Alias go Go-To- -Description 'Go to specified file or directory'
Set-Alias ip Import-Panel- -Description 'Import objects from files to a panel'
Set-Alias op Out-FarPanel -Description 'Send objects to an object panel'
Set-Alias pd Panel-DbData- -Description 'Panel SQL SELECT or table data'
Set-Alias pp Panel-Property- -Description 'Panel provider item properties'
Set-Alias sff Select-FarFile- -Description 'Select panel files'

### Actions
$Psf.Action('&m. Macro(s)...', { Panel-Macro- })
$Psf.Action('&a. Favorites...', { Menu-Favorites- })
$Psf.Action('&c. Complete word', { Complete-Word- }, 'Dialog, Editor, Panels')
$Psf.Action('&t. Edit description', { Edit-FarDescription- }, 'Editor, Viewer, Panels')
$Psf.Action('&e. Edit recent file', { Show-History- -View })
$Psf.Action('&n. Goto recent folder', { Show-History- -Folder })
$Psf.Action('&h. Go to panel head item', { Go-Head- }, 'Panels')
$Psf.Action('&g. Go to selection start', { Go-Selection- }, 'Dialog, Editor, Panels')
$Psf.Action('&d. Go to selection end', { Go-Selection- -End }, 'Dialog, Editor, Panels')
$Psf.Action('&l. To lower case', { Set-Selection- -ToLower }, 'Dialog, Editor, Panels')
$Psf.Action('&u. To upper case', { Set-Selection- -ToUpper }, 'Dialog, Editor, Panels')
$Psf.Action('&x. Search regex', { Search-Regex- }, 'Panels')
$Psf.Action('&=. More...', { Menu-More- })
$Psf.Action('Editor', $null, 'Editor')
$Psf.Action('&i. Indent selection', { Indent-Selection- }, 'Editor')
$Psf.Action('&o. Outdent selection', { Indent-Selection- -Outdent }, 'Editor')
$Psf.Action('&r. Reindent selection', { Reindent-Selection- }, 'Editor')
$Psf.Action('&f. Reformat selection', { Reformat-Selection- }, 'Editor')
$Psf.Action('&h. Go to extended home', { Go-Home- }, 'Editor')
$Psf.Action('&s. Select to extended home', { Go-Home- -Select }, 'Editor')

### Provider settings (ItemPanel)
$Psf.Providers = @{
	FileSystem = @{
		Columns = @(
			'Name'
			@{ Type = 'S'; Expression = 'Length' }
		)
	}

	Registry = @{
		Columns = @(
			@{ Type = 'Z'; Name = 'SKC'; Width = 8; Expression = 'SubKeyCount'; FormatString = '{0,8:n0}' }
			@{ Type = 'O'; Name = 'VC'; Width = 8; Expression = 'ValueCount'; FormatString = '{0,8:n0}' }
			@{ Type = 'N'; Name = 'Name'; Expression = 'PSChildName'  }
		)
	}

	Alias = @{
		Columns = 'Name', 'Definition', 'Description', 'Options'
	}

	Environment = @{
		Columns = 'Name', 'Value'
	}

	Function = @{
		Columns = @(
			'Name'
			@{ Name = 'Type'; Width = 8; Expression = 'CommandType' }
			@{ Name = 'File'; Expression = { $_.ScriptBlock.File } }
			'Definition'
		)
	}

	Variable = @{
		Columns = @(
			'Name'
			'Value'
			'Description'
			@{ Expression = 'Options'; Width = '10' }
		)
	}

	Certificate = @{
		ExcludeColumns = 'PS*'
	}

	FeedStore = @{
		ExcludeColumns = 'PS*', 'Feeds', 'LocalId', 'Path', 'Parent', 'Subfolders', 'IsRoot', 'Total*'
	}
}

### Preferences
$Psf.Settings.IntelliAutoSelect = $false
$Psf.Settings.IntelliMaxHeight = 0
$Psf.Settings.IntelliNoShadow = $false
$Psf.Settings.ListMenuFilterKey = [FarNet.KeyMode]::Ctrl + [FarNet.KeyCode]::Down

### Import modules
#Import-Module FarDescription
#Import-Module FarMacro
