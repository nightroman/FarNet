
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Platform = (property Platform Win32),
	$Configuration = (property Configuration Release),
	$TargetFrameworkVersion = (property TargetFrameworkVersion v4.0)
)

$FarHome = "C:\Bin\Far\$Platform"

use 12.0 MSBuild

$Builds = @(
	'FarNet\FarNet.build.ps1'
	'PowerShellFar\PowerShellFar.build.ps1'
)

# Synopsis: Remove temp files.
task Clean {
	foreach($_ in $Builds) { Invoke-Build Clean $_ }

	Remove-Item -Force -Recurse -ErrorAction 0 `
	ipch, obj, FarNetAccord.sdf
}

# Synopsis: Generate or update meta files.
task Meta -Inputs Get-Version.ps1 -Outputs (
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
[assembly: AssemblyCopyright("Copyright (c) 2006-2015 Roman Kuzmin")];
"@

	Set-Content PowerShellFar\AssemblyMeta.cs @"
using System.Reflection;
[assembly: AssemblyProduct("PowerShellFar")]
[assembly: AssemblyVersion("$PowerShellFarVersion")]
[assembly: AssemblyCompany("https://github.com/nightroman/FarNet")]
"@
}

# Synopsis: Build projects (Configuration, Platform) and PSF help.
task Build Meta, {
	$PlatformToolset = if ($TargetFrameworkVersion -lt 'v4') {'v90'} else {'v120'}
	exec {
		MSBuild FarNetAccord.sln /t:FarNetMan /p:Platform=$Platform /p:Configuration=$Configuration /p:TargetFrameworkVersion=$TargetFrameworkVersion /p:PlatformToolset=$PlatformToolset
	}
	Invoke-Build Help .\PowerShellFar\PowerShellFar.build.ps1
}

# Synopsis: Copy files to FarHome.
task Install {
	assert (!(Get-Process [F]ar)) 'Please exit Far.'
	foreach($_ in $Builds) { Invoke-Build Install $_ }
}

# Synopsis: Remove files from FarHome.
task Uninstall {
	foreach($_ in $Builds) { Invoke-Build Uninstall $_ }
}

# Synopsis: Make the NuGet packages at $Home.
task NuGet {
	# Test build of the sample modules, make sure they are alive
	Invoke-Build TestBuild Modules\Modules.build.ps1

	# Call
	foreach($_ in $Builds) { Invoke-Build NuGet, Clean $_ }

	# Move result archives
	Move-Item FarNet\FarNet.*.nupkg, PowerShellFar\FarNet.PowerShellFar.*.nupkg $Home -Force
}
