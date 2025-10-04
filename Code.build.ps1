<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	[ValidateScript({"FN::.\FarNet\FarNet.build.ps1", "PS::.\PowerShellFar\PowerShellFar.build.ps1"})]
	$Extends,
	$Platform = (property Platform x64),
	$FarHome = (property FarHome "C:\Bin\Far\$Platform"),
	$Configuration = (property Configuration Release)
)

Enter-Build {
	$ProgressPreference = 0
}

# Synopsis: Uninstall and clean.
# Use to build after Visual Studio.
task reset {
	Invoke-Build uninstall, clean
}

# Synopsis: Remove temp files.
task clean FN::clean, PS::clean, {
	Invoke-Build clean .\FSharpFar
}

# Synopsis: Generate or update meta files.
task meta -Inputs $BuildFile, Get-Version.ps1 -Outputs @(
	'FarNet\Directory.Build.props'
	'FarNet\FarNetMan\Active.h'
	'FarNet\FarNetMan\AssemblyMeta.h'
	'PowerShellFar\Directory.Build.props'
) {
	. .\Get-Version.ps1

	Set-Content FarNet\Directory.Build.props @"
<Project>
	<PropertyGroup>
		<Company>https://github.com/nightroman/FarNet</Company>
		<Copyright>Copyright (c) Roman Kuzmin</Copyright>
		<Product>FarNet</Product>
		<Version>$FarNetVersion</Version>
		<IncludeSourceRevisionInInformationalVersion>False</IncludeSourceRevisionInInformationalVersion>
	</PropertyGroup>
</Project>
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

	Set-Content PowerShellFar\Directory.Build.props @"
<Project>
	<PropertyGroup>
		<Company>https://github.com/nightroman/FarNet</Company>
		<Copyright>Copyright (c) Roman Kuzmin</Copyright>
		<Product>FarNet.PowerShellFar</Product>
		<Version>$PowerShellFarVersion</Version>
		<IncludeSourceRevisionInInformationalVersion>False</IncludeSourceRevisionInInformationalVersion>
	</PropertyGroup>
</Project>
"@
}

# Synopsis: Build projects and PSF help.
task build meta, {
	#! build the whole solution, i.e. FarNet, FarNetMan, PowerShellFar
	exec { & (Resolve-MSBuild) @(
		'FarNet.sln'
		'/t:restore,build'
		'/verbosity:minimal'
		"/p:FarHome=$FarHome"
		"/p:Platform=$Platform"
		"/p:Configuration=$Configuration"
	)}
}, PS::help, PS::markdown

# Synopsis: Build FarNet API docs.
task docs {
	Invoke-Build build, install, clean .\FarNet\Docs
}

# Synopsis: Remove files from FarHome.
task uninstall FN::uninstall, PS::uninstall

# Synopsis: Make the NuGet packages at $Home.
task nuget FN::nuget, PS::nuget, {
	# Test build of the sample modules, make sure they are alive
	Invoke-Build testBuild .\Modules\Modules.build.ps1

	# Move result archives
	Move-Item FarNet\FarNet.*.nupkg, PowerShellFar\FarNet.PowerShellFar.*.nupkg $Home -Force
}

# Synopsis: Build all modules.
task modules {
	assert (!(Get-Process [f]ar)) 'Exit Far.'

	# used main
	Invoke-Build build, clean .\CopyColor
	Invoke-Build build, clean .\Drawer
	Invoke-Build build, clean .\EditorKit
	Invoke-Build build, clean .\Explore
	Invoke-Build build, clean .\FolderChart
	Invoke-Build build, clean .\FSharpFar
	Invoke-Build build, clean .\GitKit
	Invoke-Build build, clean .\JavaScriptFar
	Invoke-Build build, clean .\RightControl
	Invoke-Build build, clean .\RightWords
	Invoke-Build build, clean .\Vessel

	# used demo
	Invoke-Build build, clean .\Modules\FarNet.Demo

	# pure demo
	Invoke-Build testBuild .\Modules\Modules.build.ps1
},
buildFarDescription

# Synopsis: Ensure Help, to test.
task buildFarDescription {
	Invoke-Build build, help, clean ..\..\DEV\FarDescription
}

task . clean
