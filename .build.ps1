<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Platform = (property Platform x64),
	$Configuration = (property Configuration Release),
	$TargetFramework = (property TargetFramework net7.0)
)

$FarHome = "C:\Bin\Far\$Platform"

$Builds = @(
	'FarNet\FarNet.build.ps1'
	'PowerShellFar\PowerShellFar.build.ps1'
)

# Synopsis: Uninstall and clean.
# Use to build after Visual Studio.
task reset {
	Invoke-Build uninstall, clean
}

# Synopsis: Remove temp files.
task clean {
	foreach($_ in $Builds) { Invoke-Build clean $_ }
	Invoke-Build clean .\FSharpFar\.build.ps1
}

# Synopsis: Generate or update meta files.
task meta -Inputs .build.ps1, Get-Version.ps1 -Outputs @(
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

	Invoke-Build help, markdown .\PowerShellFar\PowerShellFar.build.ps1
}

# Synopsis: Build and install API docs.
task docs {
	Invoke-Build build, install, clean .\Docs\.build.ps1
}

# Synopsis: Copy files to FarHome.
task install {
	assert (!(Get-Process [F]ar)) 'Please exit Far.'
	foreach($_ in $Builds) { Invoke-Build install $_ }
}

# Synopsis: Remove files from FarHome.
task uninstall {
	foreach($_ in $Builds) { Invoke-Build uninstall $_ }
}

# Synopsis: Make the NuGet packages at $Home.
task nuget {
	# Test build of the sample modules, make sure they are alive
	Invoke-Build testBuild .\Modules\Modules.build.ps1

	# Call
	foreach($_ in $Builds) { Invoke-Build nuget, clean $_ }

	# Move result archives
	Move-Item FarNet\FarNet.*.nupkg, PowerShellFar\FarNet.PowerShellFar.*.nupkg $Home -Force
}

# Synopsis: Build all modules.
task modules {
	assert (!(Get-Process [f]ar)) 'Exit Far.'

	# used main
	Invoke-Build build, clean .\CopyColor\.build.ps1
	Invoke-Build build, clean .\Drawer\.build.ps1
	Invoke-Build build, clean .\EditorKit\.build.ps1
	Invoke-Build build, clean .\Explore\.build.ps1
	Invoke-Build build, clean .\FolderChart\.build.ps1
	Invoke-Build build, clean .\FSharpFar\.build.ps1
	Invoke-Build build, clean .\GitKit\.build.ps1
	Invoke-Build build, clean .\JavaScriptFar\.build.ps1
	Invoke-Build build, clean .\RightControl\.build.ps1
	Invoke-Build build, clean .\RightWords\.build.ps1
	Invoke-Build build, clean .\Vessel\.build.ps1

	# used demo
	Invoke-Build build, clean .\Modules\FarNet.Demo\.build.ps1

	# pure demo
	Invoke-Build testBuild .\Modules\Modules.build.ps1
},
buildFarDescription

# Synopsis: Ensure Help, to test by Test-Help-.ps1
task buildFarDescription {
	Invoke-Build build, help, clean ..\..\DEV\FarDescription\.build.ps1
}
