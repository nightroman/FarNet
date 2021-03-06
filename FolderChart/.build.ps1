<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Platform = (property Platform x64)
)
$FarHome = "C:\Bin\Far\$Platform"
$ModuleName = 'FolderChart'
$ModuleHome = "$FarHome\FarNet\Modules\$ModuleName"

task build meta, {
	exec { dotnet restore }
	exec { dotnet msbuild $ModuleName.csproj /p:FarHome=$FarHome /p:Configuration=Release }
}

task clean {
	remove z, bin, obj, README.htm, *.nupkg
}

task version {
	($script:Version = switch -regex -file History.txt { '^= (\d+\.\d+\.\d+) =$' { $matches[1]; break } })
	assert $script:Version
}

task markdown {
	assert (Test-Path $env:MarkdownCss)
	exec { pandoc.exe @(
		'README.md'
		'--output=README.htm'
		'--from=gfm'
		'--self-contained', "--css=$env:MarkdownCss"
		'--standalone', "--metadata=pagetitle=$ModuleName"
	)}
}

task package version, markdown, {
	# test files
	assert (!(Test-Path "$ModuleHome\$ModuleName.pdb")) 'Is it Debug build? .pdb exists.'

	$dll = Get-Item "$ModuleHome\$ModuleName.dll"
	assert ($dll.VersionInfo.FileVersion -match '^(\d+\.\d+\.\d+)\.0$')
	equals ($matches[1]) $script:Version

	# package files
	remove z
	$dir = mkdir "z\tools\FarHome\FarNet\Modules\$ModuleName"

	Copy-Item -Destination z ..\Zoo\FarNetLogo.png

	Copy-Item -Destination $dir @(
		"README.htm"
		"History.txt"
		"LICENSE.txt"
		"$ModuleHome\$ModuleName.dll"
	)
}

task nuget package, {
	$text = @'
FolderChart is the FarNet module for Far Manager.

For the current panel directory this tool calculates file and directory sizes
and shows the results as a chart in a separate window with some interaction.

---

How to install and update FarNet and modules:

https://github.com/nightroman/FarNet#readme

---
'@
	# nuspec
	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.FolderChart</id>
		<version>$script:Version</version>
		<owners>Roman Kuzmin</owners>
		<authors>Roman Kuzmin</authors>
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<icon>FarNetLogo.png</icon>
		<license type="expression">BSD-3-Clause</license>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<description>$text</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/master/FolderChart/History.txt</releaseNotes>
		<tags>FarManager FarNet Module</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack z\Package.nuspec }
}

task meta -Inputs .build.ps1, History.txt -Outputs AssemblyInfo.cs -Jobs version, {
	Set-Content AssemblyInfo.cs @"
using System.Reflection;
[assembly: AssemblyProduct("FarNet.FolderChart")]
[assembly: AssemblyVersion("$script:Version")]
[assembly: AssemblyTitle("FarNet module for Far Manager")]
[assembly: AssemblyDescription("Shows folder item sizes in a chart.")]
[assembly: AssemblyCompany("https://github.com/nightroman/FarNet")]
[assembly: AssemblyCopyright("Copyright (c) Roman Kuzmin")]
"@
}

task . build, clean
