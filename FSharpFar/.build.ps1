
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

[CmdletBinding()]
param(
	$FarHome = (property FarHome C:\Bin\Far\Win32),
	$Configuration = (property Configuration Release)
)

$ModuleName = 'FSharpFar'
$ProjectRoot = 'src'
$ProjectName = "$ModuleName.fsproj"

task . Build, Clean

task Init Meta, {
	exec {paket.exe install --only-referenced}
}

task Kill Clean, {
	Remove-Item -Force -Recurse -ErrorAction 0 @(
		'packages'
		'paket.lock'
		'src\.vs'
		'src\FSharpFar.sln'
		'src\AssemblyInfo.fs'
	)
}

task Build {
	use * MSBuild.exe
	exec {MSBuild.exe $ProjectRoot\$ProjectName /p:FarHome=$FarHome /p:Configuration=$Configuration}
}

task Clean {
	Remove-Item -Force -Recurse -ErrorAction 0 @(
		'z'
		'README.htm'
		"FarNet.$ModuleName.*.nupkg"
		"$ProjectRoot\bin"
		"$ProjectRoot\obj"
	)
}

task Help {
	exec { pandoc.exe --standalone --from=markdown_strict+backtick_code_blocks --output=README.htm README.md }
}

task Version {
	($script:Version = switch -regex -file History.txt {'^= (\d+\.\d+\.\d+) =$' {$matches[1]; break}})
}

task Meta Version, {
	Set-Content src/AssemblyInfo.fs @"
namespace System.Reflection
[<assembly: AssemblyCompany("https://github.com/nightroman/FarNet")>]
[<assembly: AssemblyCopyright("Copyright (c) 2016 Roman Kuzmin")>]
[<assembly: AssemblyDescription("F# interactive and editor services for Far Manager.")>]
[<assembly: AssemblyProduct("FarNet.FSharpFar")>]
[<assembly: AssemblyTitle("FarNet module FSharpFar for Far Manager")>]
[<assembly: AssemblyVersion("$Version")>]
()
"@
} -Inputs .build.ps1, History.txt -Outputs src/AssemblyInfo.fs

task Package Help, {
	$toModule = "z\tools\FarHome\FarNet\Modules\$ModuleName"
	$fromModule = "$FarHome\FarNet\Modules\$ModuleName"

	Remove-Item [z] -Force -Recurse
	$null = mkdir $toModule

	Copy-Item -Destination $toModule @(
		'README.htm'
		'History.txt'
		'LICENSE.txt'
		"$fromModule\$ModuleName.dll"
		"$fromModule\FSharp.Compiler.Service.dll"
	)
}

task NuGet Package, Version, {
	$text = @'
FSharpFar provides F# interactive, scripting, and editor services for Far Manager.

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
		<id>FarNet.FSharpFar</id>
		<version>$Version</version>
		<authors>Roman Kuzmin</authors>
		<owners>Roman Kuzmin</owners>
		<projectUrl>https://github.com/nightroman/FarNet/tree/master/FSharpFar</projectUrl>
		<iconUrl>https://raw.githubusercontent.com/wiki/nightroman/FarNet/images/FarNetLogo.png</iconUrl>
		<licenseUrl>https://raw.githubusercontent.com/nightroman/FarNet/master/FSharpFar/LICENSE.txt</licenseUrl>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<summary>$text</summary>
		<description>$text</description>
		<releaseNotes>https://raw.githubusercontent.com/nightroman/FarNet/master/FSharpFar/History.txt</releaseNotes>
		<tags>FarManager FarNet Module FSharp</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack z\Package.nuspec -NoPackageAnalysis }
}
