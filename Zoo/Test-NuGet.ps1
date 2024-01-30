<#
.Synopsis
	Tests packages and the update procedures.

.Description
	Run this when FN and PSF packages are in $HOME.
#>

Set-StrictMode -Version 3
$ErrorActionPreference = 1
$tempFarHome = "C:\TEMP\TempFarHome"

Import-Module $env:FarNetCode\PowerShellFar\Modules\FarPackage

function Build-TempFolder {
	Remove-Item $tempFarHome -Force -Recurse -ErrorAction 0
	$null = mkdir $tempFarHome
}

function Test-Diff {
	try {
		Assert-SameFile -Fail -Text ($Sample | Out-String) ($Result | Out-String) $env:MERGE
	}
	catch {
		Write-Error $_
	}
	Write-Host -ForegroundColor Green OK
}

function Test-DiffUpdate($SampleFileName, $ExtraItems) {
	$Sample = $(
		Get-Content "$PSScriptRoot\$SampleFileName" | Select-UpdateLine
		$ExtraItems
	) | Sort-Object -Unique
	Test-Diff
}

filter Select-UpdateLine {
	if ($_.StartsWith("FarHome.$platform\")) {
		$_.Replace("FarHome.$platform\", '')
	}
	elseif ($_.StartsWith("FarHome.x")) {
	}
	elseif ($_.StartsWith("FarHome\")) {
		$_.Replace("FarHome\", '')
	}
	elseif ($_ -eq 'FarHome') {
	}
	else {
		$_
	}
}

### Test version data and archives; archives are ready to avoid download on testing
Write-Host -ForegroundColor Cyan "Checking version and archives..."
. $env:FarNetCode\Get-Version.ps1

# get/check paths
$pack1 = (Resolve-Path "$HOME\FarNet.$FarNetVersion.nupkg").Path
$pack2 = (Resolve-Path "$HOME\FarNet.PowerShellFar.$PowerShellFarVersion.nupkg").Path

### FarNet package
Write-Host -ForegroundColor Cyan "Testing FarNet package..."
Build-TempFolder
& 7z.exe x $pack1 '-ir!tools' "-o$tempFarHome" > $null
$Result = Get-ChildItem -LiteralPath $tempFarHome\tools -Recurse -Force -Name | Sort-Object
$Sample = Get-Content $PSScriptRoot\Test-NuGet-FarNet.txt
Test-Diff

### PowerShellFar package
Write-Host -ForegroundColor Cyan "Testing PowerShellFar package..."
Build-TempFolder
& 7z.exe x $pack2 '-ir!tools' "-o$tempFarHome" > $null
$Result = Get-ChildItem -LiteralPath $tempFarHome\tools -Recurse -Force -Name | Sort-Object
$Sample = Get-Content $PSScriptRoot\Test-NuGet-PowerShellFar.txt
Test-Diff

### Update FarNet
Write-Host -ForegroundColor Cyan "Testing FarNet update..."
Build-TempFolder
foreach($platform in 'x64', 'x86') {
	Write-Host -ForegroundColor Cyan "Testing FarNet update $platform..."
	Restore-FarPackage $pack1 -FarHome $tempFarHome -Platform $platform
	$Result = Get-ChildItem -LiteralPath $tempFarHome -Recurse -Force -Name | Sort-Object
	Test-DiffUpdate Test-NuGet-FarNet.txt Update.FarNet.info
}

### Update PowerShellFar
Write-Host -ForegroundColor Cyan "Testing PowerShellFar update..."
Build-TempFolder
Restore-FarPackage $pack2 -FarHome $tempFarHome
$Result = Get-ChildItem -LiteralPath $tempFarHome -Recurse -Force -Name | Sort-Object
Test-DiffUpdate Test-NuGet-PowerShellFar.txt Update.FarNet.PowerShellFar.info

# end
Remove-Item $tempFarHome -Force -Recurse -ErrorAction 0
