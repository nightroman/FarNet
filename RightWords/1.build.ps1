<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

Set-StrictMode -Version 3
$_name = 'RightWords'
$_root = "$FarHome\FarNet\Modules\$_name"
$_description = 'Spell-checker. FarNet module for Far Manager.'

task build meta, {
	exec { dotnet build -c $Configuration /p:FarHome=$FarHome --tl:off }
}

task publish help, resgen

task help -Inputs README.md -Outputs $_root\RightWords.hlf {
	exec { pandoc.exe README.md "--output=$env:TEMP\z.html" --from=gfm --syntax-highlighting=none }
	exec { HtmlToFarHelp.exe "from=$env:TEMP\z.html" "to=$_root\RightWords.hlf" }
}

# https://github.com/nightroman/PowerShelf/blob/main/Invoke-Environment.ps1
task resgen -Partial -Inputs RightWords.restext, RightWords.ru.restext -Outputs $_root\RightWords.resources, $_root\RightWords.ru.resources {
	begin {
		$VsDevCmd = @(Get-Item "$env:ProgramFiles\Microsoft Visual Studio\2022\*\Common7\Tools\VsDevCmd.bat")
		Invoke-Environment.ps1 -File ($VsDevCmd[0])
	}
	process {
		exec { resgen.exe $_ $2 }
	}
}

task clean {
	remove z, bin, obj, README.html, *.nupkg
}

task version {
	($Script:_version = Get-BuildVersion History.txt '^= (\d+\.\d+\.\d+) =$')
}

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

task markdown {
	requires -Path $env:MarkdownCss
	exec { pandoc.exe @(
		'README.md'
		'--output=README.html'
		'--from=gfm'
		'--embed-resources'
		'--standalone'
		"--css=$env:MarkdownCss"
		'--standalone', '--metadata=pagetitle=RightWords'
	)}
}

task package markdown, version, {
	remove z
	$toModule = New-Item -ItemType Directory "z\tools\FarHome\FarNet\Modules\$_name"

	Copy-Item -Destination $toModule -Recurse `
	$_root\*,
	..\LICENSE,
	README.html,
	History.txt,
	RightWords.macro.lua

	Copy-Item -Destination z `
	..\Zoo\FarNetLogo.png,
	README.md

	Assert-SameFile.ps1 -Result (Get-ChildItem $toModule -Recurse -File -Name) -Text -View $env:MERGE @'
History.txt
LICENSE
README.html
RightWords.dll
RightWords.hlf
RightWords.macro.lua
RightWords.pdb
RightWords.resources
RightWords.ru.resources
WeCantSpell.Hunspell.dll
'@
}

task nuget package, version, {
	equals $_version (Get-Item "$_root\$_name.dll").VersionInfo.ProductVersion

	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.RightWords</id>
		<version>$_version</version>
		<authors>Roman Kuzmin</authors>
		<owners>Roman Kuzmin</owners>
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<icon>FarNetLogo.png</icon>
		<readme>README.md</readme>
		<license type="expression">BSD-3-Clause</license>
		<description>$_description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/main/RightWords/History.txt</releaseNotes>
		<tags>FarManager FarNet Module Hunspell</tags>
	</metadata>
</package>
"@

	exec { NuGet pack z\Package.nuspec }
}

task . build, clean
