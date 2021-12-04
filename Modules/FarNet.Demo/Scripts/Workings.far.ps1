<#
.Synopsis
	How to get, update, edit settings.

.Description
	This script works a newly created settings instance.

	PowerShellFar command:
	ps: .\Workings.far.ps1
#>

# load the library
Add-Type -Path "$env:FARHOME\FarNet\Modules\FarNet.Demo\FarNet.Demo.dll"

# get settings
$settings = [FarNet.Demo.Workings]::new()
$data = $settings.GetData()

# update settings
++$data.MoreCount
$settings.Save()

# edit settings
$settings.Edit()
