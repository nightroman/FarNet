<#
.Synopsis
	Interactive steps to release FarNet and FarNet.PowerShellFar.

	# simple
	Invoke-Build *

	# persistent
	Build-Checkpoint
#>

param(
	[ValidateScript({"GH::..\Code.build.ps1", "DC::..\FarNet\Docs\Docs.build.ps1"})]
	$Extends,
	# persistent data
	[Parameter(DontShow=1)]$Push,
	[Parameter(DontShow=1)]$Tags
)

Set-Alias ask Confirm-Build
$RepoRoot = $env:FarNetCode
requires -Path $RepoRoot

task setVersion -If {
	ask @'
Edit Get-Version.ps1 to set versions.
What you change is what you are about to push.
'@
} {
	Start-Process Far.exe /e, $RepoRoot\Get-Version.ps1 -Wait
}

task chooseToPush {
	while(($script:Push = Read-Host @'
Choose to push:
[1] All
[2] FarNet
[3] PowerShellFar

'@) -notmatch '^(1|2|3)$') {}

	. $RepoRoot\Get-Version.ps1
	$script:Tags = switch($Push) {
		1 {"FarNet.$FarNetVersion", "PowerShellFar.$PowerShellFarVersion"}
		2 {"FarNet.$FarNetVersion"}
		3 {"PowerShellFar.$PowerShellFarVersion"}
		default {throw}
	}
}

task build buildFarNet, buildHelp, buildDocs

task buildFarNet -If {
	ask 'Build FarNet projects'
} {
	while (Get-Process [F]ar) {Read-Host 'Exit Far and enter to build all'}
	Build-FarNet.ps1 -Reset
}

#! run when push=2 as well, or help file is missing -> assert
task buildHelp -If {
	$env:FarNetToBuildPowerShellFarHelp -and (ask 'Build PSF help')
} {
	Start-Far 'ps:Invoke-Build help' $RepoRoot\PowerShellFar -Exit 0
}

task buildDocs -If {($Push -ne 3) -and (ask 'Build FarNet CHM help')} DC::make

task nugetAndTest -If {
	ask @'
Make last changes in docs and notes.
Create and test NuGet packages?
'@
} GH::nuget, {
	.\Test-NuGet.ps1
}

task testAll -If {
	ask @'
Test all, mind x86, x64.
Start default testing?
'@
} {
	Test-Far.ps1 -All
	Clear-Host
}

task pushPackages -If {
	ask 'Push new packages to NuGet manually. Then continue.'
}

task commitSource -If {
	ask "Start git gui to commit/amend changes for [$Tags]?"
} {
	Set-Location $RepoRoot
	git gui
}

task pushSource -If {
	ask "Push commits and tags [$Tags]?"
} {
	Set-Location $RepoRoot

	# push changes
	exec { git push }

	# local tags
	foreach($_ in $Tags) {
		exec { git tag -a $_ -m $_ }
	}

	# remote tags
	exec { git push origin $Tags }

	# gc because then we zip
	exec { git gc --prune=now }
}

# before zip
task clean GH::clean

task zipFarDev -If {
	ask 'Zip FarDev? // checkpoint Code and maybe related extras'
} {
	. ..\Get-Version.ps1
	$zip = "FarDev.$FarNetVersion-$PowerShellFarVersion.7z"

	Set-Location ..\..\..
	requires -Path FarDev

	if (Test-Path $zip) { Remove-Item $zip -Confirm }
	exec { & 7z.exe a $zip FarDev '-xr!.vs' '-xr!bin' '-xr!obj' '-xr!packages' '-xr!*.clixml' }
}

task .
