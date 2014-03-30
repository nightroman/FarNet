
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Platform = (property Platform Win32)
)

$FarHome = "C:\Bin\Far\$Platform"
$ModuleHome = "$FarHome\FarNet\Modules\FolderChart"

task . Build, Clean

# Build and install
task Build {
	use 4.0 MSBuild
	exec { MSBuild FolderChart.csproj /p:Configuration=Release /p:FarHome=$FarHome }
}

# New About-FolderChart.htm
task Help {
	exec { MarkdownToHtml "From = About-FolderChart.text; To = About-FolderChart.htm" }
}

task Clean {
	Remove-Item -Force -Recurse -ErrorAction 0 -Path `
	z, bin, obj, About-FolderChart.htm, FarNet.FolderChart.*.nupkg
}

task Version {
	$dll = Get-Item -LiteralPath $ModuleHome\FolderChart.dll
	assert ($dll.VersionInfo.FileVersion -match '^(\d+\.\d+\.\d+)\.0$')
	($script:Version = $matches[1])
}

task Package Help, {
	$toModule = 'z\tools\FarHome\FarNet\Modules\FolderChart'

	Remove-Item -Force -Recurse -ErrorAction 0 -Path [z]
	$null = mkdir $toModule

	Copy-Item -Destination $toModule `
	About-FolderChart.htm,
	History.txt,
	LICENSE.txt,
	$ModuleHome\FolderChart.dll
}

task NuGet Package, Version, {
	$text = @'
FolderChart is the FarNet module for Far Manager.

For the current directory it calculates file and directory sizes and
shows the results as a chart in a modal window with some interaction.

Requires .NET Framework 4.0

---

To install and update FarNet packages, follow these steps:

https://farnet.googlecode.com/svn/trunk/Install-FarNet.en.txt

---
'@
	# nuspec
	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.FolderChart</id>
		<version>$Version</version>
		<owners>Roman Kuzmin</owners>
		<authors>Roman Kuzmin</authors>
		<projectUrl>https://code.google.com/p/farnet</projectUrl>
		<iconUrl>https://farnet.googlecode.com/svn/trunk/FarNetLogo.png</iconUrl>
		<licenseUrl>https://farnet.googlecode.com/svn/trunk/FolderChart/LICENSE.txt</licenseUrl>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<summary>$text</summary>
		<description>$text</description>
		<releaseNotes>https://farnet.googlecode.com/svn/trunk/FolderChart/History.txt</releaseNotes>
		<tags>FarManager FarNet Module</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack z\Package.nuspec -NoPackageAnalysis }
}
