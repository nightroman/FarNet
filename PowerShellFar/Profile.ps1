<#
.Synopsis
	Main profile sample.

.Description
	Location: %FARPROFILE%\FarNet\PowerShellFar\Profile.ps1
	See
		FarNetAPI.chm /Actor
		About-PowerShellFar.html /Profile.ps1
#>

# Macros
doskey ib=ps:Invoke-Build

# Aliases
Set-Alias ff Find-FarFile
Set-Alias fm Show-FarMessage
Set-Alias gt Go-To.ps1
Set-Alias op Out-FarPanel
Set-Alias re Search-Regex.ps1

# Functions
function prompt {'PS> '}

# Preferences
$Psf.Settings.PopupAutoSelect = $false
$Psf.Settings.PopupMaxHeight = 0
$Psf.Settings.PopupNoShadow = $false

# Provider settings (ItemPanel)
$Psf.Providers = @{
	Registry = @{
		Columns = @(
			@{ Kind = 'Z'; Name = 'SKC'; Width = 8; Expression = 'SubKeyCount'; FormatString = '{0,8:n0}' }
			@{ Kind = 'O'; Name = 'VC'; Width = 8; Expression = 'ValueCount'; FormatString = '{0,8:n0}' }
			@{ Kind = 'N'; Name = 'Name' }
		)
	}
	Alias = @{
		Columns = 'Name', 'Definition', 'Description', 'Options'
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
