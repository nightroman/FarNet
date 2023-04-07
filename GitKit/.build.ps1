<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	[ValidateSet('Debug', 'Release')]
	[string]$Configuration = 'Release'
	,
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

Set-StrictMode -Version 3
$ModuleName = 'GitKit'
$ModuleRoot = "$FarHome\FarNet\Modules\$ModuleName"
$Description = 'Far Manager git helpers based on LibGit2Sharp.'

task build meta, {
	exec { dotnet build -c $Configuration "-p:FarHome=$FarHome" }
}

task publish {
	$bit = if ($FarHome -match 'x64') {'win-x64'} elseif ($FarHome -match 'Win32') {'win-x86'} else {throw}
	$ver = (Select-Xml '//PackageReference[@Include="LibGit2Sharp"]' "$ModuleName.csproj").Node.Version

	Copy-Item -Destination $ModuleRoot @(
		"$ModuleRoot\runtimes\$bit\native\*.dll"
		"$HOME\.nuget\packages\LibGit2Sharp\$ver\lib\netstandard2.0\LibGit2Sharp.xml"
	)

	remove $ModuleRoot\runtimes
}

task help {
	exec { pandoc.exe README.md --output=README.htm --from=gfm --no-highlight }
	exec { HtmlToFarHelp from=README.htm to=$ModuleRoot\GitKit.hlf }
	remove README.htm
}

task clean {
	remove z, obj, README.htm, *.nupkg
}

task version {
	($script:Version = switch -regex -file History.txt {'^= (\d+\.\d+\.\d+) =$' {$matches[1]; break}})
}

task markdown {
	assert (Test-Path $env:MarkdownCss)
	exec { pandoc.exe @(
		'README.md'
		'--output=README.htm'
		'--from=gfm'
		'--embed-resources'
		'--standalone'
		"--css=$env:MarkdownCss"
		"--metadata=pagetitle=$ModuleName"
	)}
}

task win32 {
	Invoke-Build build -FarHome C:\Bin\Far\Win32
}

task package win32, help, markdown, {
	remove z
	$toModule = mkdir "z\tools\FarHome\FarNet\Modules\$ModuleName"
	$toModule64 = mkdir "z\tools\FarHome.x64\FarNet\Modules\$ModuleName"
	$toModule86 = mkdir "z\tools\FarHome.x86\FarNet\Modules\$ModuleName"

	# module
	exec { robocopy $ModuleRoot $toModule /s /xf *.pdb git2*.dll} (0..2)

	# meta
	Copy-Item -Destination z @(
		'README.md'
		'..\Zoo\FarNetLogo.png'
	)

	# repo
	Copy-Item -Destination $toModule @(
		'README.htm'
		'History.txt'
		'..\LICENSE'
	)

	# bits
	Copy-Item $ModuleRoot\git2*.dll $toModule64
	Copy-Item C:\Bin\Far\Win32\FarNet\Modules\GitKit\git2*.dll $toModule86

	equals 9 @(Get-ChildItem $toModule -Recurse -File).Count
	equals 1 @(Get-ChildItem $toModule64 -Recurse -File).Count
	equals 1 @(Get-ChildItem $toModule86 -Recurse -File).Count
}

task meta -Inputs .build.ps1, History.txt -Outputs Directory.Build.props -Jobs version, {
	Set-Content Directory.Build.props @"
<Project>
	<PropertyGroup>
		<Description>$Description</Description>
		<Company>https://github.com/nightroman/FarNet</Company>
		<Copyright>Copyright (c) Roman Kuzmin</Copyright>
		<Product>FarNet.$ModuleName</Product>
		<Version>$Version</Version>
	</PropertyGroup>
</Project>
"@
}

task nuget package, version, {
	# test versions
	$dllPath = "$FarHome\FarNet\Modules\$ModuleName\$ModuleName.dll"
	($dllVersion = (Get-Item $dllPath).VersionInfo.FileVersion.ToString())
	assert $dllVersion.StartsWith("$Version.") 'Versions mismatch.'

	# nuspec
	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.$ModuleName</id>
		<version>$Version</version>
		<authors>Roman Kuzmin</authors>
		<owners>Roman Kuzmin</owners>
		<projectUrl>https://github.com/nightroman/FarNet/tree/main/$ModuleName</projectUrl>
		<icon>FarNetLogo.png</icon>
		<readme>README.md</readme>
		<license type="expression">BSD-3-Clause</license>
		<description>$Description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/main/$ModuleName/History.txt</releaseNotes>
		<tags>FarManager FarNet Module Git</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack z\Package.nuspec }
}

task . build, help, clean
