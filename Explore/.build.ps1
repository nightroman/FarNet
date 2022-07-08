<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

$ModuleName = 'Explore'
$ModuleHome = "$FarHome\FarNet\Modules\$ModuleName"

task build {
	exec { dotnet build -c $Configuration /p:FarHome=$FarHome }
}

task publish {
	Copy-Item -Destination $ModuleHome @(
		"bin\$Configuration\net6.0\$ModuleName.dll"
		"bin\$Configuration\net6.0\$ModuleName.pdb"
	)
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
	remove z, bin, obj, README.htm, FarNet.$ModuleName.*.nupkg
}

task version {
	($script:Version = switch -regex -file History.txt {'^= (\d+\.\d+\.\d+) =$' {$matches[1]; break}})
	assert $script:Version
}

task package help, version, {
	equals "$Version.0" (Get-Item $ModuleHome\$ModuleName.dll).VersionInfo.FileVersion
	$toModule = "z\tools\FarHome\FarNet\Modules\$ModuleName"

	remove z
	$null = mkdir $toModule

	# logo
	Copy-Item -Destination z ..\Zoo\FarNetLogo.png

	# module
	Copy-Item -Destination $toModule @(
		'README.htm'
		'History.txt'
		'LICENSE'
		"$ModuleHome\$ModuleName.dll"
	)
}

task nuget package, version, {
	$description = @'
Explore is the FarNet module for Far Manager.

It searches in FarNet module panels and opens the result panel.
It is invoked from the command line with the prefix "Explore:".

---

How to install and update FarNet and modules:

https://github.com/nightroman/FarNet#readme
'@

	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.$ModuleName</id>
		<version>$Version</version>
		<owners>Roman Kuzmin</owners>
		<authors>Roman Kuzmin</authors>
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<icon>FarNetLogo.png</icon>
		<license type="expression">BSD-3-Clause</license>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<description>$description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/master/$ModuleName/History.txt</releaseNotes>
		<tags>FarManager FarNet Module</tags>
	</metadata>
</package>
"@

	exec { NuGet.exe pack z\Package.nuspec }
}

task . build, clean
