<#
.Synopsis
	How to get, update, edit settings.

.Description
	This script works with the static cached instance.

	PowerShellFar command:
	ps: .\Settings.far.ps1
#>

# load the library
Add-Type -Path "$env:FARHOME\FarNet\Modules\FarNet.Demo\FarNet.Demo.dll"

# get settings
$settings = [FarNet.Demo.Settings]::Default
$data = $settings.GetData()

# update settings
++$data.Age
$settings.Save()

# edit settings
$settings.Edit()
