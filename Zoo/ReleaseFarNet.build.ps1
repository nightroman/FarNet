<#
.Synopsis
	Release steps invoked by Invoke-Build *
#>

#_190903_023748 use PositionalBinding or TabEx test fails
[CmdletBinding(PositionalBinding=0)]
param(
	# persistent data
	$Push,
	$Tags
)

$IBRoot = Split-Path ((Get-Alias Invoke-Build).Definition)
. $IBRoot\Tasks\Ask\Ask.tasks.ps1

ask setVersion {
	Start-Process Far.exe /e, $env:FarNetCode\Get-Version.ps1 -Wait
} -Prompt @'
Edit Get-Version.ps1 to set versions.
What you change is what you are about to push.
'@

task chooseToPush {
	while(($script:Push = Read-Host @'
Choose to push:
[1] All
[2] FarNet
[3] PowerShellFar

'@) -notmatch '^(1|2|3)$') {}

	. $env:FarNetCode\Get-Version.ps1
	$script:Tags = switch($Push) {
		1 {"FarNet.$FarNetVersion", "PowerShellFar.$PowerShellFarVersion"}
		2 {"FarNet.$FarNetVersion"}
		3 {"PowerShellFar.$PowerShellFarVersion"}
		default {throw}
	}
}

task build {
	while (Get-Process [F]ar) {Read-Host 'Exit Far and enter to build all'}
	Build-FarNet.ps1
}

task buildDocs -If ($Push -ne 3) {
	Invoke-Build build, install, clean $env:FarNetCode\Docs\FarNetAPI.build.ps1
}

task buildModules {
	Invoke-Build modules, clean $env:FarNetCode\.build.ps1
}

ask nugetAndTest {
	Invoke-Build NuGet, TestNuGet
} -Prompt @'
Make last changes in About files, not in code.
Create and test NuGet packages?
'@

ask testAll {
	Test-Far.ps1 -All
	Clear-Host
} -Prompt @'
Test all, mind x86, x64.
Start default testing?
'@

ask pushPackages -Prompt 'Push new packages to NuGet manually. Then continue.'

ask commitSource -Prompt {"Start git gui to commit/amend changes for [$Tags]?"} {
	Set-Location $env:FarNetCode
	git gui
}

ask pushSource -Prompt {"Push commits and tags [$Tags]?"} {
	Set-Location $env:FarNetCode
	exec { git push }
	foreach($_ in $Tags) {
		exec { git tag -a $_ -m $_ }
	}
	exec { git push origin $Tags }
	exec { git gc --aggressive --prune=now }
}

ask zipFarDev -Prompt 'Zip FarDev (checkpoint all)?' {
	Invoke-Build zipFarDev
}
