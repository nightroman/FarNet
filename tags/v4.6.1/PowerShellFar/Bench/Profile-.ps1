
<#
.Synopsis
	Main startup code (example).
	Author: Roman Kuzmin

.Description
	Configuration: startup code to load this profile:
	. Profile-.ps1

	See API: Action [PowerShellFar.Actor]
	See help: Profile-.ps1, Global objects, Plugin settings
#>

### Error action: 'Stop' is recommended to stop on errors immediately
$ErrorActionPreference = 'Stop'

### Aliases
Set-Alias fff Find-FarFile -Description 'Finds the panel file'
Set-Alias gff Get-FarFile -Description 'Gets the panel file(s)'
Set-Alias go Go-To- -Description 'Go to specified file or directory'
Set-Alias ip Import-Panel- -Description 'Import objects from files to a panel'
Set-Alias nff New-FarFile -Description 'Creates a new panel file'
Set-Alias op Out-FarPanel -Description 'Send objects to an object panel'
Set-Alias pd Panel-DbData- -Description 'Panel SQL SELECT or table data'
Set-Alias pp Panel-Property- -Description 'Panel provider item properties'
Set-Alias scff Select-FarFile- -Description 'Select files in the panel'
Set-Alias srff Search-FarFile -Description 'Search files in the panel'

### Actions
$Psf.Action('&m. Macro(s)...', { Panel-Macro- })
$Psf.Action('&a. Favorites...', { Menu-Favorites- })
$Psf.Action('&c. Complete word', { Complete-Word- }, 'Dialog, Editor, Panels')
$Psf.Action('&t. Edit description', { Edit-FarDescription- }, 'Editor, Viewer, Panels')
$Psf.Action('&n. Go to recent folder', { Show-History- -Folder })
$Psf.Action('&h. Go to panel head item', { Go-Head- }, 'Panels')
$Psf.Action('&g. Go to selection start', { Go-Selection- }, 'Dialog, Editor, Panels')
$Psf.Action('&d. Go to selection end', { Go-Selection- -End }, 'Dialog, Editor, Panels')
$Psf.Action('&l. To lower case', { Set-Selection- -ToLower }, 'Dialog, Editor, Panels')
$Psf.Action('&u. To upper case', { Set-Selection- -ToUpper }, 'Dialog, Editor, Panels')
$Psf.Action('&x. Search regex', { Search-Regex- }, 'Panels')
$Psf.Action('&[. Open text link', { Open-TextLink- }, 'Dialog, Editor, Panels')
$Psf.Action('&=. More...', { Menu-More- })
$Psf.Action('Editor', $null, 'Editor')
$Psf.Action('&i. Indent selection', { Indent-Selection- }, 'Editor')
$Psf.Action('&o. Outdent selection', { Indent-Selection- -Outdent }, 'Editor')
$Psf.Action('&r. Reindent selection', { Reindent-Selection- }, 'Editor')
$Psf.Action('&f. Reformat selection', { Reformat-Selection- }, 'Editor')
$Psf.Action('&]. Copy text link', { $Far.CopyToClipboard((Get-TextLink-)) }, 'Editor')

### Provider settings (ItemPanel)
$Psf.Providers = @{

	Registry = @{
		Columns = @(
			@{ Kind = 'Z'; Name = 'SKC'; Width = 8; Expression = 'SubKeyCount'; FormatString = '{0,8:n0}' }
			@{ Kind = 'O'; Name = 'VC'; Width = 8; Expression = 'ValueCount'; FormatString = '{0,8:n0}' }
			@{ Kind = 'N'; Name = 'Name'; Expression = '' } # note: data source is ignored
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
			@{ Expression = 'Options'; Width = 10 }
		)
	}
}

### Preferences
$Psf.Settings.PopupAutoSelect = $false
$Psf.Settings.PopupMaxHeight = 0
$Psf.Settings.PopupNoShadow = $false
$Psf.Settings.ListMenuFilterKey = [FarNet.KeyMode]::Ctrl + [FarNet.KeyCode]::Down

### Module helpers
# Here we import some modules so that they are always loaded and ready to use.
# Alternatively we can use proxy functions that load their modules themselves.

#Import-Module FarDescription
#Import-Module FarInventory
#Import-Module FarMacro

<#
.Synopsis
	Open-ServicePanel proxy.
#>
function Open-ServicePanel
{
	Remove-Item Function:\Open-ServicePanel
	Import-Module FarInventory
	Open-ServicePanel
}

<#
.Synopsis
	Open-StartupCommandPanel proxy.
#>
function Open-StartupCommandPanel
{
	Remove-Item Function:\Open-StartupCommandPanel
	Import-Module FarInventory
	Open-StartupCommandPanel
}

<#
.Synopsis
	Open-UninstallPanel proxy.
#>
function Open-UninstallPanel
{
	Remove-Item Function:\Open-UninstallPanel
	Import-Module FarInventory
	Open-UninstallPanel
}
