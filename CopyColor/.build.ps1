<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Platform = (property Platform x64)
)
$FarHome = "C:\Bin\Far\$Platform"
$ModuleHome = "$FarHome\FarNet\Modules\CopyColor"

task meta -Inputs .build.ps1, History.txt -Outputs Directory.Build.props version, {
	Set-Content Directory.Build.props @"
<Project>
  <PropertyGroup>
    <Company>https://github.com/nightroman/FarNet</Company>
    <Copyright>Copyright (c) Roman Kuzmin</Copyright>
    <Product>FarNet.CopyColor</Product>
    <Version>$Version</Version>
    <Description>Copy text with colors as HTML.</Description>
  </PropertyGroup>
</Project>
"@
}

task build {
	exec { dotnet build -c Release /p:FarHome=$FarHome }
}

task help {
	assert (Test-Path $env:MarkdownCss)
	exec {
		pandoc.exe @(
			'README.md'
			'--output=README.htm'
			'--from=gfm'
			'--self-contained'
			"--css=$env:MarkdownCss"
			'--metadata=pagetitle:FarNet'
		)
	}
}

task clean {
	remove z, bin, obj, README.htm, FarNet.CopyColor.*.nupkg
}

task version {
	($script:Version = switch -regex -file History.txt {'^= (\d+\.\d+\.\d+) =$' {$matches[1]; break}})
	assert $script:Version
}

task package help, version, {
	equals "$Version.0" (Get-Item $ModuleHome\CopyColor.dll).VersionInfo.FileVersion
	$toModule = 'z\tools\FarHome\FarNet\Modules\CopyColor'

	remove z
	$null = mkdir $toModule

	# logo
	Copy-Item -Destination z ..\Zoo\FarNetLogo.png

	# module
	Copy-Item -Destination $toModule @(
		'README.htm'
		'History.txt'
		'LICENSE'
		"$ModuleHome\CopyColor.dll"
	)
}

task nuget package, version, {
	$description = @'
CopyColor is the FarNet module for Far Manager.

It copies selected text with colors from the editor to the clipboard using HTML
clipboard format. This text can be pasted to editors supporting this format.

---

How to install and update FarNet and modules:

https://github.com/nightroman/FarNet#readme
'@

	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.CopyColor</id>
		<version>$Version</version>
		<owners>Roman Kuzmin</owners>
		<authors>Roman Kuzmin</authors>
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<icon>FarNetLogo.png</icon>
		<license type="expression">BSD-3-Clause</license>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<description>$description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/master/CopyColor/History.txt</releaseNotes>
		<tags>FarManager FarNet Module</tags>
	</metadata>
</package>
"@

	exec { NuGet.exe pack z\Package.nuspec }
}

task . build, clean
