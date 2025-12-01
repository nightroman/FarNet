<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

Set-StrictMode -Version 3
$_name = 'JavaScriptFar'
$_root = "$FarHome\FarNet\Modules\$_name"
$_description = 'JavaScript scripting in Far Manager.'

task meta -Inputs $BuildFile, History.txt -Outputs Directory.Build.props -Jobs version, {
	Set-Content Directory.Build.props @"
<Project>
	<PropertyGroup>
		<Company>https://github.com/nightroman/FarNet</Company>
		<Copyright>Copyright (c) Roman Kuzmin</Copyright>
		<Description>$_description</Description>
		<Product>FarNet.$_name</Product>
		<Version>$_version</Version>
		<IncludeSourceRevisionInInformationalVersion>False</IncludeSourceRevisionInInformationalVersion>
	</PropertyGroup>
</Project>
"@
}

task build meta, {
	exec { dotnet build -c $Configuration -p:FarHome=$FarHome --tl:off }
}

task publish {
	Set-Location $_root
	Copy-Item runtimes\win-x64\native\* .
	remove runtimes
}

task clean {
	remove z, bin, obj, README.html, *.nupkg
}

task version {
	($Script:_version = Get-BuildVersion History.txt '^= (\d+\.\d+\.\d+) =$')
}

task markdown {
	requires -Path $env:MarkdownCss
	exec { pandoc.exe @(
		'README.md'
		'--output=README.html'
		'--from=gfm'
		'--embed-resources'
		'--standalone'
		"--css=$env:MarkdownCss"
		"--metadata=pagetitle=$_name"
	)}
}

task package markdown, version, {
	equals $_version (Get-Item "$_root\$_name.dll").VersionInfo.ProductVersion

	remove z
	$toModule = New-Item "z\tools\FarHome\FarNet\Modules\$_name" -ItemType Directory

	# module
	Copy-Item "$_root\*" $toModule -Recurse

	# nuget
	Copy-Item -Destination z @(
		'README.md'
		'..\Zoo\FarNetLogo.png'
	)

	# about
	Copy-Item -Destination $toModule @(
		'README.html'
		'History.txt'
		'..\LICENSE'
	)

	Assert-SameFile -Text -View $env:MERGE -Result (Get-ChildItem $toModule -Force -Recurse -File -Name) -Sample @'
ClearScript.Core.dll
ClearScript.V8.dll
ClearScript.V8.ICUData.dll
ClearScript.Windows.Core.dll
ClearScript.Windows.dll
ClearScriptV8.win-x64.dll
History.txt
JavaScriptFar.dll
JavaScriptFar.pdb
LICENSE
Newtonsoft.Json.dll
README.html
'@
}

task nuget package, version, {
	$dllVersion = (Get-Item "$FarHome\FarNet\Modules\$_name\$_name.dll").VersionInfo.ProductVersion
	equals $_version $dllVersion

	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.$_name</id>
		<version>$_version</version>
		<authors>Roman Kuzmin</authors>
		<owners>Roman Kuzmin</owners>
		<projectUrl>https://github.com/nightroman/FarNet/tree/main/$_name</projectUrl>
		<icon>FarNetLogo.png</icon>
		<readme>README.md</readme>
		<license type="expression">BSD-3-Clause</license>
		<description>$_description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/main/$_name/History.txt</releaseNotes>
		<tags>FarManager FarNet Module JavaScript ClearScript V8</tags>
	</metadata>
</package>
"@

	exec { NuGet pack z\Package.nuspec }
}

task test {
	exec { pwsf .\Tests -nop -x 999 -c Test-FarNet.ps1 }
}

task . build, clean
