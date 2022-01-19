<#
.Synopsis
	Tests packages and the update procedures.
#>

$ErrorActionPreference = 1
Set-StrictMode -Version Latest
$tempFarHome = "C:\TEMP\TempFarHome"

Import-Module $env:FarNetCode\PowerShellFar\Modules\FarPackage

function MakeTemp {
	Remove-Item $tempFarHome -Force -Recurse -ErrorAction 0
	$null = mkdir $tempFarHome
}

function TestDiff {
	$diff = Compare-Object $actual $sample
	if ($diff) {
		$diff
		$actual | %{"'$_'"} | Set-Content $tempFarHome\actual.ps1
		Write-Error -ErrorAction 1 "Actual list is different from expected. See actual in $tempFarHome\actual.ps1"
	}
	else {
		Write-Host -ForegroundColor Green OK
	}
}

### Test version data and archives; archives are ready to avoid download on testing
Write-Host -ForegroundColor Cyan "Checking version and archives..."
. $env:FarNetCode\Get-Version.ps1

# get/check paths
$pack1 = (Resolve-Path "$HOME\FarNet.$FarNetVersion*.nupkg").Path
$pack2 = (Resolve-Path "$HOME\FarNet.PowerShellFar.$PowerShellFarVersion*.nupkg").Path

### FarNet package
MakeTemp
& 7z.exe x $pack1 '-ir!tools' "-o$tempFarHome" > $null
Push-Location $tempFarHome\tools
$actual = Get-ChildItem -Recurse -Force -Name | Sort-Object
Pop-Location

Write-Host -ForegroundColor Cyan "Testing..."
$sample = @(
'FarHome'
'FarHome.x64'
'FarHome.x86'
'FarHome\FarNet'
'FarHome\Plugins'
'FarHome\Far.exe.config'
'FarHome\FarNet\About-FarNet.htm'
'FarHome\FarNet\FarNet.dll'
'FarHome\FarNet\FarNet.Tools.dll'
'FarHome\FarNet\FarNet.Tools.xml'
'FarHome\FarNet\FarNet.Works.Config.dll'
'FarHome\FarNet\FarNet.Works.Dialog.dll'
'FarHome\FarNet\FarNet.Works.Editor.dll'
'FarHome\FarNet\FarNet.Works.Loader.dll'
'FarHome\FarNet\FarNet.Works.Panels.dll'
'FarHome\FarNet\FarNet.xml'
'FarHome\FarNet\FarNetAPI.chm'
'FarHome\FarNet\History.txt'
'FarHome\FarNet\LICENSE'
'FarHome\Plugins\FarNet'
'FarHome\Plugins\FarNet\FarNetMan.hlf'
'FarHome.x64\Plugins'
'FarHome.x64\Plugins\FarNet'
'FarHome.x64\Plugins\FarNet\FarNetMan.dll'
'FarHome.x86\Plugins'
'FarHome.x86\Plugins\FarNet'
'FarHome.x86\Plugins\FarNet\FarNetMan.dll'
)

TestDiff

### PowerShellFar package
MakeTemp
& 7z.exe x $pack2 '-ir!tools' "-o$tempFarHome" > $null
Push-Location $tempFarHome\tools
$actual = Get-ChildItem -Recurse -Force -Name | Sort-Object
Pop-Location

Write-Host -ForegroundColor Cyan "Testing..."
$sample = @(
'FarHome'
'FarHome\FarNet'
'FarHome\FarNet\Modules'
'FarHome\FarNet\Modules\PowerShellFar'
'FarHome\FarNet\Modules\PowerShellFar\About-PowerShellFar.htm'
'FarHome\FarNet\Modules\PowerShellFar\Bench'
'FarHome\FarNet\Modules\PowerShellFar\Bench\ArgumentCompleters.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Clear-Session.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Complete-Word-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Connect-SQLite-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Descript.ion'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Edit-FarDescription.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Generate-Dialog-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Get-TextLink.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Go-Head-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Go-Selection-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Go-To-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Import-Panel-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Indent-Selection-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Invoke-Editor-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Invoke-Shortcut-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Job-RemoveItem-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Menu-Favorites-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Menu-More-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Open-TextLink.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Open-TODO-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Panel-BitsTransfer-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Panel-DbData-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Panel-DbTable-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Panel-Job-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Panel-Process-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Panel-Property-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Profile.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Profile-Editor.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Reformat-Selection-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Reindent-Selection-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Remove-EmptyString-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Remove-EndSpace-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Rename-FarFile.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Search-Regex-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Select-Bookmark-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Select-FarFile-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Set-Selection-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Show-EditorColor-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Show-History-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Show-Hlf-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Show-Image.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Show-KeyName-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Show-Markdown-.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Start-Far.ps1'
'FarHome\FarNet\Modules\PowerShellFar\Bench\Update-FarManager.ps1'
'FarHome\FarNet\Modules\PowerShellFar\History.txt'
'FarHome\FarNet\Modules\PowerShellFar\LICENSE'
'FarHome\FarNet\Modules\PowerShellFar\Modules'
'FarHome\FarNet\Modules\PowerShellFar\Modules\FarInventory'
'FarHome\FarNet\Modules\PowerShellFar\Modules\FarInventory\about_FarInventory.help.txt'
'FarHome\FarNet\Modules\PowerShellFar\Modules\FarInventory\FarInventory.psm1'
'FarHome\FarNet\Modules\PowerShellFar\PowerShellFar.dll'
'FarHome\FarNet\Modules\PowerShellFar\PowerShellFar.dll-Help.xml'
'FarHome\FarNet\Modules\PowerShellFar\PowerShellFar.hlf'
'FarHome\FarNet\Modules\PowerShellFar\PowerShellFar.macro.lua'
'FarHome\FarNet\Modules\PowerShellFar\PowerShellFar.ps1'
'FarHome\FarNet\Modules\PowerShellFar\PowerShellFar.xml'
'FarHome\FarNet\Modules\PowerShellFar\TabExpansion.txt'
'FarHome\FarNet\Modules\PowerShellFar\TabExpansion2.ps1'
)

TestDiff

#####################################################################

$sample = @(
'FarNet'
'Plugins'
'Far.exe.config'
'Update.FarNet.info'
'FarNet\About-FarNet.htm'
'FarNet\FarNet.dll'
'FarNet\FarNet.Tools.dll'
'FarNet\FarNet.Tools.xml'
'FarNet\FarNet.Works.Config.dll'
'FarNet\FarNet.Works.Dialog.dll'
'FarNet\FarNet.Works.Editor.dll'
'FarNet\FarNet.Works.Loader.dll'
'FarNet\FarNet.Works.Panels.dll'
'FarNet\FarNet.xml'
'FarNet\FarNetAPI.chm'
'FarNet\History.txt'
'FarNet\LICENSE'
'Plugins\FarNet'
'Plugins\FarNet\FarNetMan.dll'
'Plugins\FarNet\FarNetMan.hlf'
)

### Update FarNet
Write-Host -ForegroundColor Cyan "Updating FarNet..."
MakeTemp
1..2 | %{
	Restore-FarPackage $pack1 -FarHome $tempFarHome -Platform x86

	Write-Host -ForegroundColor Cyan "Checking update..."
	Push-Location $tempFarHome
	$actual = Get-ChildItem -Recurse -Force -Name | Sort-Object
	Pop-Location

	TestDiff
}

#####################################################################

$sample = @(
'FarNet'
'FarNet\Modules'
'FarNet\Modules\PowerShellFar'
'FarNet\Modules\PowerShellFar\About-PowerShellFar.htm'
'FarNet\Modules\PowerShellFar\Bench'
'FarNet\Modules\PowerShellFar\Bench\ArgumentCompleters.ps1'
'FarNet\Modules\PowerShellFar\Bench\Clear-Session.ps1'
'FarNet\Modules\PowerShellFar\Bench\Complete-Word-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Connect-SQLite-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Descript.ion'
'FarNet\Modules\PowerShellFar\Bench\Edit-FarDescription.ps1'
'FarNet\Modules\PowerShellFar\Bench\Generate-Dialog-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Get-TextLink.ps1'
'FarNet\Modules\PowerShellFar\Bench\Go-Head-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Go-Selection-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Go-To-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Import-Panel-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Indent-Selection-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Invoke-Editor-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Invoke-Shortcut-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Job-RemoveItem-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Menu-Favorites-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Menu-More-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Open-TextLink.ps1'
'FarNet\Modules\PowerShellFar\Bench\Open-TODO-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Panel-BitsTransfer-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Panel-DbData-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Panel-DbTable-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Panel-Job-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Panel-Process-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Panel-Property-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Profile.ps1'
'FarNet\Modules\PowerShellFar\Bench\Profile-Editor.ps1'
'FarNet\Modules\PowerShellFar\Bench\Reformat-Selection-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Reindent-Selection-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Remove-EmptyString-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Remove-EndSpace-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Rename-FarFile.ps1'
'FarNet\Modules\PowerShellFar\Bench\Search-Regex-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Select-Bookmark-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Select-FarFile-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Set-Selection-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Show-EditorColor-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Show-History-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Show-Hlf-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Show-Image.ps1'
'FarNet\Modules\PowerShellFar\Bench\Show-KeyName-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Show-Markdown-.ps1'
'FarNet\Modules\PowerShellFar\Bench\Start-Far.ps1'
'FarNet\Modules\PowerShellFar\Bench\Update-FarManager.ps1'
'FarNet\Modules\PowerShellFar\History.txt'
'FarNet\Modules\PowerShellFar\LICENSE'
'FarNet\Modules\PowerShellFar\Modules'
'FarNet\Modules\PowerShellFar\Modules\FarInventory'
'FarNet\Modules\PowerShellFar\Modules\FarInventory\about_FarInventory.help.txt'
'FarNet\Modules\PowerShellFar\Modules\FarInventory\FarInventory.psm1'
'FarNet\Modules\PowerShellFar\PowerShellFar.dll'
'FarNet\Modules\PowerShellFar\PowerShellFar.dll-Help.xml'
'FarNet\Modules\PowerShellFar\PowerShellFar.hlf'
'FarNet\Modules\PowerShellFar\PowerShellFar.macro.lua'
'FarNet\Modules\PowerShellFar\PowerShellFar.ps1'
'FarNet\Modules\PowerShellFar\PowerShellFar.xml'
'FarNet\Modules\PowerShellFar\TabExpansion.txt'
'FarNet\Modules\PowerShellFar\TabExpansion2.ps1'
'Update.FarNet.PowerShellFar.info'
)

### Update PowerShellFar
Write-Host -ForegroundColor Cyan "Updating PowerShellFar..."
MakeTemp
1..2 | %{
	Restore-FarPackage $pack2 -FarHome $tempFarHome

	Write-Host -ForegroundColor Cyan "Checking the update..."
	Push-Location $tempFarHome
	$actual = Get-ChildItem -Recurse -Force -Name | Sort-Object
	Pop-Location

	TestDiff
}

# end
Remove-Item $tempFarHome -Force -Recurse -ErrorAction 0
