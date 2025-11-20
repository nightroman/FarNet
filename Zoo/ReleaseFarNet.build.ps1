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
	$Extends
	,
	[Parameter(DontShow=1)]$Push
	,
	[Parameter(DontShow=1)]$Tags
	,
	[Parameter(DontShow=1)]$FarNetVersion
	,
	[Parameter(DontShow=1)]$PowerShellFarVersion
)

Set-Alias ask Confirm-Build
$RepoRoot = $env:FarNetCode
requires -Path $RepoRoot

task Set-Version -If {
	ask @'
Edit Get-Version.ps1 to set versions.
What you change is what you are about to push.
'@
} {
	Start-Process Far.exe "/e $RepoRoot\Get-Version.ps1" -Wait
	. $RepoRoot\Get-Version.ps1
	$Script:FarNetVersion = $FarNetVersion
	$Script:PowerShellFarVersion = $PowerShellFarVersion
}

task Select-Project {
	while(($Script:Push = Read-Host @'
Select project to push:
[1] All
[2] FarNet
[3] PowerShellFar

'@) -notmatch '^(1|2|3)$') {}

	$Script:Tags = switch($Push) {
		1 {"FarNet.$FarNetVersion", "PowerShellFar.$PowerShellFarVersion"}
		2 {"FarNet.$FarNetVersion"}
		3 {"PowerShellFar.$PowerShellFarVersion"}
		default {throw}
	}
}

task Build PS::sync, Build-FarNet, Build-PSF-Help, Build-Docs, pwsf::build

task Build-FarNet -If {
	ask 'Build FarNet projects'
} {
	while (Get-Process [F]ar) {Read-Host 'Exit Far and enter to build all'}
	Build-FarNet.ps1 -Reset
}

#! run when push=2 as well, or help file is missing -> assert
task Build-PSF-Help -If {
	$env:FarNetToBuildPowerShellFarHelp -and (ask 'Build PSF help')
} {
	Start-Far 'ps:Invoke-Build help' $RepoRoot\PowerShellFar -Exit 0
}

task Build-Docs -If {($Push -ne 3) -and (ask 'Build FarNet CHM help')} DC::make

task Make-NuGet -If {
	ask @'
Make last changes in docs and notes.
Create and test NuGet packages?
'@
} GH::nuget, {
	.\Test-NuGet.ps1
}

task Test-FarNet -If {
	ask @'
Start testing?
'@
} {
	$begin = [datetime]::Now

	Set-Alias far $(if ($env:FARHOME) {"$env:FARHOME\Far.exe"} else {'Far.exe'})
	far -ro "ps:$env:FarNetCode\Test\Test-FarNet.ps1"

	$end = [datetime](Get-Content temp:Test-FarNet.end.txt -ErrorAction 0)
	if ($end -lt $begin) {
		throw "Tests failed."
	}
}

### Extras
$extras = Get-ChildItem ..\..\Test -Filter *.test.ps1
foreach($test in $extras) {
	task $test.Name -Data $test {
		Invoke-Build * $Task.Data.FullName
	}
}

task Test-Extras $extras.ForEach('Name') -If {
	ask 'Start extra tests?'
}

task Push-Packages -If {
	ask "Push new packages to NuGet: [$Tags]"
} {
	$Script:NuGetApiKey = $(property NuGetApiKey)
	$files = @(
		if ($Push -in 1, 2) {"$HOME\FarNet.$FarNetVersion.nupkg"}
		if ($Push -in 1, 3) {"$HOME\FarNet.PowerShellFar.$PowerShellFarVersion.nupkg"}
	)
	foreach($file in $files) {
		print 3 $file
		exec { nuget push $file -Source nuget.org -ApiKey $NuGetApiKey }
	}
	remove "$HOME\FarNet.*.nupkg" -verbose
}

task Commit-Source -If {
	ask "Start git gui to commit/amend changes for [$Tags]?"
} {
	Set-Location $RepoRoot
	git gui
}

task Push-Source -If {
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
task Clean GH::clean

task Zip-FarDev -If {
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
