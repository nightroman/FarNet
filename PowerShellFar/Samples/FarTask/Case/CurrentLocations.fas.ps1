<#
.Synopsis
	Shows different current paths in tasks and jobs.

.Description
	How to run:
	(1) Ensure two panels have different locations
	(2) ps: Start-FarTask CurrentLocations.fas.ps1
#>

# print initial current paths
$Data.CurrentLocation = Get-Location
ps: {
	Write-Host 'Test 1' -ForegroundColor Cyan
	"Far process directory : $([Environment]::CurrentDirectory)"
	"Far panel directory   : $($Far.CurrentDirectory)"
	"Task current location : $($Data.CurrentLocation)"
	"Job current location  : $(Get-Location)"
}

# go to another panel
keys Tab

# change task current location
Set-Location $env:FARHOME\FarNet

# print changed current paths
$Data.CurrentLocation = Get-Location
ps: {
	Write-Host 'Test 2' -ForegroundColor Cyan
	"Far process directory : $([Environment]::CurrentDirectory)"
	"Far panel directory   : $($Far.CurrentDirectory)"
	"Task current location : $($Data.CurrentLocation)"
	"Job current location  : $(Get-Location)"
}

# go back to first panel
keys Tab
