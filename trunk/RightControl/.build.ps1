
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Platform = (property Platform Win32)
)

$FarHome = "C:\Bin\Far\$Platform"
$ModuleHome = "$FarHome\FarNet\Modules\RightControl"

task . Build, Clean

# Build and install
task Build {
	use 4.0 MSBuild
	exec { MSBuild RightControl.csproj /p:Configuration=Release /p:FarHome=$FarHome }
}

# New About-RightControl.htm
task Help {
	exec { MarkdownToHtml "From = About-RightControl.text; To = About-RightControl.htm" }
}

task Clean {
	Remove-Item -Force -Recurse -ErrorAction 0 -Path `
	z, bin, obj, About-RightControl.htm, FarNet.RightControl.*.nupkg
}

task Version {
	$dll = Get-Item -LiteralPath $ModuleHome\RightControl.dll
	assert ($dll.VersionInfo.FileVersion -match '^(\d+\.\d+\.\d+)\.0$')
	($script:Version = $matches[1])
}

task Package Help, {
	$toModule = 'z\tools\FarHome\FarNet\Modules\RightControl'

	Remove-Item -Force -Recurse -ErrorAction 0 -Path [z]
	$null = mkdir $toModule

	Copy-Item -Destination $toModule `
	About-RightControl.htm,
	History.txt,
	LICENSE.txt,
	RightControl.macro.lua,
	$ModuleHome\RightControl.dll
}

task NuGet Package, Version, {
	$text = @'
RightControl is the FarNet module for Far Manager.

It alters some actions in editors, edit controls, and the command line.
New actions are similar to what many popular editors do on stepping,
selecting, deleting by words, and etc.

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
		<id>FarNet.RightControl</id>
		<version>$Version</version>
		<owners>Roman Kuzmin</owners>
		<authors>Roman Kuzmin</authors>
		<projectUrl>https://code.google.com/p/farnet</projectUrl>
		<iconUrl>https://farnet.googlecode.com/svn/trunk/FarNetLogo.png</iconUrl>
		<licenseUrl>https://farnet.googlecode.com/svn/trunk/RightControl/LICENSE.txt</licenseUrl>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<summary>$text</summary>
		<description>$text</description>
		<releaseNotes>https://farnet.googlecode.com/svn/trunk/RightControl/History.txt</releaseNotes>
		<tags>FarManager FarNet Module</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack z\Package.nuspec -NoPackageAnalysis }
}
