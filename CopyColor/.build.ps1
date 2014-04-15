
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Platform = (property Platform Win32)
)

$FarHome = "C:\Bin\Far\$Platform"
$ModuleHome = "$FarHome\FarNet\Modules\CopyColor"

task . Build, Clean

# Build and install
task Build {
	use 4.0 MSBuild
	exec { MSBuild CopyColor.csproj /p:Configuration=Release /p:FarHome=$FarHome }
}

# New About-CopyColor.htm
task Help {
	exec { MarkdownToHtml "From = About-CopyColor.text; To = About-CopyColor.htm" }
}

task Clean {
	Remove-Item -Force -Recurse -ErrorAction 0 -Path `
	z, bin, obj, About-CopyColor.htm, FarNet.CopyColor.*.nupkg
}

task Version {
	$dll = Get-Item -LiteralPath $ModuleHome\CopyColor.dll
	assert ($dll.VersionInfo.FileVersion -match '^(\d+\.\d+\.\d+)\.0$')
	($script:Version = $matches[1])
}

task Package Help, {
	$toModule = 'z\tools\FarHome\FarNet\Modules\CopyColor'

	Remove-Item -Force -Recurse -ErrorAction 0 -Path [z]
	$null = mkdir $toModule

	Copy-Item -Destination $toModule `
	About-CopyColor.htm,
	History.txt,
	LICENSE.txt,
	$ModuleHome\CopyColor.dll
}

task NuGet Package, Version, {
	$text = @'
CopyColor is the FarNet module for Far Manager.

It copies selected text with colors from the editor to the clipboard
using HTML clipboard format. This text can be pasted into Microsoft
Word, Outlook, and some other editors.

---

To install FarNet packages, follow these steps:

https://farnet.googlecode.com/svn/trunk/Install-FarNet.en.txt

---
'@
	# nuspec
	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.CopyColor</id>
		<version>$Version</version>
		<owners>Roman Kuzmin</owners>
		<authors>Roman Kuzmin</authors>
		<projectUrl>https://code.google.com/p/farnet</projectUrl>
		<iconUrl>https://farnet.googlecode.com/svn/trunk/FarNetLogo.png</iconUrl>
		<licenseUrl>https://farnet.googlecode.com/svn/trunk/CopyColor/LICENSE.txt</licenseUrl>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<summary>$text</summary>
		<description>$text</description>
		<releaseNotes>https://farnet.googlecode.com/svn/trunk/CopyColor/History.txt</releaseNotes>
		<tags>FarManager FarNet Module</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack z\Package.nuspec -NoPackageAnalysis }
}
