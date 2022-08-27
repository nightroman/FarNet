<#
.Synopsis
	Main profile (example).

.Description
	The profile should be in %FARPROFILE%\FarNet\PowerShellFar

	See API: Action [PowerShellFar.Actor]
	See help: Profile.ps1, Global objects
#>

### Aliases
Set-Alias edit Open-FarEditor -Description 'Opens the editor'
Set-Alias ff Find-FarFile -Description 'Find the panel file'
Set-Alias goto Go-To.ps1 -Description 'Go to file or directory'
Set-Alias op Out-FarPanel -Description 'Send objects to panel'
Set-Alias pp Get-FarPath -Description 'Get panel paths'
Set-Alias re Search-Regex -Description 'Search in files'

### Preferences
$Psf.Settings.PopupAutoSelect = $false
$Psf.Settings.PopupMaxHeight = 0
$Psf.Settings.PopupNoShadow = $false

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
