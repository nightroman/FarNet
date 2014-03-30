
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Platform = (property Platform Win32)
)

$FarHome = "C:\Bin\Far\$Platform"
$ModuleHome = "$FarHome\FarNet\Modules\Vessel"

#! Close Far Manager.
task . Build, Help, Clean

# Build and install the assembly.
task Build {
	use 4.0 MSBuild
	exec { MSBuild Vessel.csproj /p:Configuration=Release /p:FarHome=$FarHome }
}

# In addition to Build: new About-Vessel.htm, $ModuleHome\Vessel.hlf
task Help {
	exec { MarkdownToHtml "From=About-Vessel.text" "To=About-Vessel.htm" }
	exec { HtmlToFarHelp "From=About-Vessel.htm" "To=$ModuleHome\Vessel.hlf" }
}

task Clean {
	Remove-Item -Force -Recurse -ErrorAction 0 `
	z, bin, obj, About-Vessel.htm, FarNet.Vessel.*.nupkg
}

task Version {
	$dll = Get-Item -LiteralPath $ModuleHome\Vessel.dll
	assert ($dll.VersionInfo.FileVersion -match '^(\d+\.\d+\.\d+)\.0$')
	($script:Version = $matches[1])
}

task Package Help, {
	$toModule = 'z\tools\FarHome\FarNet\Modules\Vessel'

	Remove-Item -Force -Recurse -ErrorAction 0 -Path [z]
	$null = mkdir $toModule

	Copy-Item -Destination $toModule `
	About-Vessel.htm,
	History.txt,
	LICENSE.txt,
	Vessel.macro.lua,
	$ModuleHome\Vessel.dll,
	$ModuleHome\Vessel.hlf
}

task NuGet Package, Version, {
	$text = @'
Vessel (View/Edit/Save/SELect) is the FarNet module for Far Manager.

It records and maintains history of file view, edit, and save operations
and provides related tools.

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
		<id>FarNet.Vessel</id>
		<version>$Version</version>
		<owners>Roman Kuzmin</owners>
		<authors>Roman Kuzmin</authors>
		<projectUrl>https://code.google.com/p/farnet</projectUrl>
		<iconUrl>https://farnet.googlecode.com/svn/trunk/FarNetLogo.png</iconUrl>
		<licenseUrl>https://farnet.googlecode.com/svn/trunk/Vessel/LICENSE.txt</licenseUrl>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<summary>$text</summary>
		<description>$text</description>
		<releaseNotes>https://farnet.googlecode.com/svn/trunk/Vessel/History.txt</releaseNotes>
		<tags>FarManager FarNet Module</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack z\Package.nuspec -NoPackageAnalysis }
}
