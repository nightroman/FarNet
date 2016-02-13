
<#
.Synopsis
	Main profile (example).
	Author: Roman Kuzmin

.Description
	The profile should be in %FARPROFILE%\FarNet\PowerShellFar

	See API: Action [PowerShellFar.Actor]
	See help: Profile.ps1, Global objects
#>

### Recommended error action
$ErrorActionPreference = 'Stop'

### Aliases
Set-Alias fff Find-FarFile -Description 'Finds the panel file'
Set-Alias gff Get-FarFile -Description 'Gets the panel file(s)'
Set-Alias go Go-To-.ps1 -Description 'Go to specified file or directory'
Set-Alias ip Import-Panel-.ps1 -Description 'Import objects from files to a panel'
Set-Alias nff New-FarFile -Description 'Creates a new panel file'
Set-Alias op Out-FarPanel -Description 'Send objects to an object panel'
Set-Alias pd Panel-DbData-.ps1 -Description 'Panel SQL SELECT or table data'
Set-Alias pp Panel-Property-.ps1 -Description 'Panel provider item properties'
Set-Alias scff Select-FarFile-.ps1 -Description 'Select files in the panel'
Set-Alias srff Search-FarFile -Description 'Search files in the panel'

### Preferences
$Psf.Settings.PopupAutoSelect = $false
$Psf.Settings.PopupMaxHeight = 0
$Psf.Settings.PopupNoShadow = $false

### The script invoked after editor console commands.
#$Psf.Settings.EditorConsoleEndOutputScript = 'Get-Date'

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
