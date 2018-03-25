
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Platform = (property Platform x64)
)
$FarHome = "C:\Bin\Far\$Platform"
$ModuleHome = "$FarHome\FarNet\Modules\FolderChart"

task Build Meta, {
	exec { dotnet restore }
	exec { dotnet msbuild FolderChart.csproj /p:Configuration=Release /p:FarHome=$FarHome }
}

task Clean {
	remove z, bin, obj, README.htm, *.nupkg
}

task Version {
	($script:Version = switch -regex -file History.txt {'^= (\d+\.\d+\.\d+) =$' {$matches[1]; break}})
}

task Markdown {
	function Convert-Markdown($Name) {pandoc.exe --standalone --from=gfm "--output=$Name.htm" "--metadata=pagetitle=$Name" "$Name.md"}
	exec { Convert-Markdown README }
}

task Package Version, Markdown, {
	$toModule = 'z\tools\FarHome\FarNet\Modules\FolderChart'

	remove z
	$null = mkdir $toModule

	Copy-Item -Destination $toModule `
	README.htm,
	History.txt,
	LICENSE.txt,
	$ModuleHome\FolderChart.dll

	$dll = Get-Item -LiteralPath $ModuleHome\FolderChart.dll
	assert ($dll.VersionInfo.FileVersion -match '^(\d+\.\d+\.\d+)\.0$')
	equals $script:Version ($matches[1])
}

task Meta -Inputs .build.ps1, History.txt -Outputs AssemblyInfo.cs -Jobs Version, {
	Set-Content AssemblyInfo.cs @"
using System;
using System.Reflection;
using System.Runtime.InteropServices;
[assembly: AssemblyProduct("FarNet.FolderChart")]
[assembly: AssemblyVersion("$script:Version")]
[assembly: AssemblyTitle("FarNet module for Far Manager")]
[assembly: AssemblyDescription("Shows folder item sizes in a chart.")]
[assembly: AssemblyCompany("https://github.com/nightroman/FarNet")]
[assembly: AssemblyCopyright("Copyright (c) Roman Kuzmin")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
"@
}

task NuGet Package, Version, {
	$text = @'
FolderChart is the FarNet module for Far Manager.

For the current directory it calculates file and directory sizes and
shows the results as a chart in a modal window with some interaction.

Requires .NET Framework 4.0

---

To install FarNet packages, follow these steps:

https://raw.githubusercontent.com/nightroman/FarNet/master/Install-FarNet.en.txt

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
		<iconUrl>https://raw.githubusercontent.com/wiki/nightroman/FarNet/images/FarNetLogo.png</iconUrl>
		<licenseUrl>https://raw.githubusercontent.com/nightroman/FarNet/master/FolderChart/LICENSE.txt</licenseUrl>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<summary>FarNet module for Far Manager</summary>
		<description>$text</description>
		<releaseNotes>https://raw.githubusercontent.com/nightroman/FarNet/master/FolderChart/History.txt</releaseNotes>
		<tags>FarManager FarNet Module</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack z\Package.nuspec }
}

task . Build, Clean
