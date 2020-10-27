<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Platform = (property Platform x64),
	$Configuration = (property Configuration Release),
	$TargetFrameworkVersion = (property TargetFrameworkVersion v4.5)
)

$FarHome = "C:\Bin\Far\$Platform"

$Builds = @(
	'FarNet\FarNet.build.ps1'
	'PowerShellFar\PowerShellFar.build.ps1'
)

# Synopsis: Remove temp files.
task clean {
	foreach($_ in $Builds) { Invoke-Build Clean $_ }
	Invoke-Build Clean FSharpFar\.build.ps1

	remove debug, ipch, obj, FarNetAccord.sdf, FarNetAccord.VC.db
}

# Synopsis: Generate or update meta files.
task meta -Inputs .build.ps1, Get-Version.ps1 -Outputs (
	'FarNet\AssemblyMeta.cs',
	'FarNet\FarNetMan\Active.h',
	'FarNet\FarNetMan\AssemblyMeta.h',
	'PowerShellFar\AssemblyMeta.cs'
) {
	. .\Get-Version.ps1

	Set-Content FarNet\AssemblyMeta.cs @"
using System.Reflection;
[assembly: AssemblyProduct("FarNet")]
[assembly: AssemblyVersion("$FarNetVersion")]
[assembly: AssemblyCompany("https://github.com/nightroman/FarNet")]
"@

	$v1 = [Version]$FarVersion
	$v2 = [Version]$FarNetVersion
	Set-Content FarNet\FarNetMan\Active.h @"
#pragma once

#define MinFarVersionMajor $($v1.Major)
#define MinFarVersionMinor $($v1.Minor)
#define MinFarVersionBuild $($v1.Build)

#define FarNetVersionMajor $($v2.Major)
#define FarNetVersionMinor $($v2.Minor)
#define FarNetVersionBuild $($v2.Build)
"@

	Set-Content FarNet\FarNetMan\AssemblyMeta.h @"
[assembly: AssemblyProduct("FarNet")];
[assembly: AssemblyVersion("$FarNetVersion")];
[assembly: AssemblyCompany("https://github.com/nightroman/FarNet")];
[assembly: AssemblyTitle("FarNet plugin manager")];
[assembly: AssemblyDescription("FarNet plugin manager")];
[assembly: AssemblyCopyright("Copyright (c) Roman Kuzmin")];
"@

	Set-Content PowerShellFar\AssemblyMeta.cs @"
using System.Reflection;
[assembly: AssemblyProduct("PowerShellFar")]
[assembly: AssemblyVersion("$PowerShellFarVersion")]
[assembly: AssemblyCompany("https://github.com/nightroman/FarNet")]
"@
}

# Synopsis: Build projects and PSF help.
task build meta, {
	#! build the whole solution, i.e. FarNet, FarNetMan, PowerShellFar
	exec { & (Resolve-MSBuild) @(
		'FarNetAccord.sln'
		'/verbosity:minimal'
		"/p:FarHome=$FarHome"
		"/p:Platform=$Platform"
		"/p:Configuration=$Configuration"
	)}

	Invoke-Build -File PowerShellFar\PowerShellFar.build.ps1 -Task Help, BuildPowerShellFarHelp
}

# Synopsis: Build and install API docs.
task docs {
	Invoke-Build Build, Install, Clean ./Docs/.build.ps1
}

# Synopsis: Copy files to FarHome.
task install {
	assert (!(Get-Process [F]ar)) 'Please exit Far.'
	foreach($_ in $Builds) { Invoke-Build Install $_ }
}

# Synopsis: Remove files from FarHome.
task uninstall {
	foreach($_ in $Builds) { Invoke-Build Uninstall $_ }
}

# Synopsis: Make the NuGet packages at $Home.
task nuget {
	# Test build of the sample modules, make sure they are alive
	Invoke-Build TestBuild Modules\Modules.build.ps1

	# Call
	foreach($_ in $Builds) { Invoke-Build NuGet, Clean $_ }

	# Move result archives
	Move-Item FarNet\FarNet.*.nupkg, PowerShellFar\FarNet.PowerShellFar.*.nupkg $Home -Force
}
