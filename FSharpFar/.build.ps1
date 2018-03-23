
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$FarHome = (property FarHome C:\Bin\Far\x64),
	$Configuration = (property Configuration Release)
)

$ModuleName = 'FSharpFar'
$ProjectRoot = 'src'
$ProjectName = "$ModuleName.fsproj"

task Init Meta, {
	exec {paket.exe install}
	Remove-Item paket-files
}

task Kill Clean, {
	Get-Item -ErrorAction 0 @(
		'packages'
		'src\.vs'
		'src\FSharpFar.sln'
		'src\AssemblyInfo.fs'
	) | Remove-Item -Force -Recurse
}

task Build {
	Set-Alias MSBuild (Resolve-MSBuild)
	exec {MSBuild $ProjectRoot\$ProjectName /p:FarHome=$FarHome /p:Configuration=$Configuration /v:n}
}

task Clean {
	Get-Item -ErrorAction 0 @(
		'z'
		'README.htm'
		'src\FSharpFar.fs.ini'
		"FarNet.$ModuleName.*.nupkg"
		"$ProjectRoot\bin"
		"$ProjectRoot\obj"
	) | Remove-Item -Force -Recurse
}

task Markdown {
	function Convert-Markdown($Name) {pandoc.exe --standalone --from=gfm "--output=$Name.htm" "--metadata=pagetitle=$Name" "$Name.md"}
	exec { Convert-Markdown README }
}

task Version {
	($script:Version = switch -regex -file History.txt {'^= (\d+\.\d+\.\d+) =$' {$matches[1]; break}})
}

task Meta -Inputs .build.ps1, History.txt -Outputs src/AssemblyInfo.fs -Jobs Version, {
	Set-Content src/AssemblyInfo.fs @"
namespace System.Reflection
[<assembly: AssemblyCompany("https://github.com/nightroman/FarNet")>]
[<assembly: AssemblyCopyright("Copyright (c) 2016-2018 Roman Kuzmin")>]
[<assembly: AssemblyDescription("F# interactive, scripting, compiler, and editor services for Far Manager.")>]
[<assembly: AssemblyProduct("FarNet.FSharpFar")>]
[<assembly: AssemblyTitle("FarNet module FSharpFar for Far Manager")>]
[<assembly: AssemblyVersion("$Version")>]
()
"@
}

task Package Markdown, {
	$toHome = "z\tools\FarHome"
	$toModule = "$toHome\FarNet\Modules\$ModuleName"
	$fromModule = "$FarHome\FarNet\Modules\$ModuleName"

	remove z
	$null = mkdir $toModule

	Copy-Item -Destination $toHome @(
		"$FarHome\FSharp.Core.dll"
		"$FarHome\FSharp.Core.optdata"
		"$FarHome\FSharp.Core.sigdata"
	)

	Copy-Item -Destination $toModule @(
		'README.htm'
		'History.txt'
		'LICENSE.txt'
		"$fromModule\$ModuleName.dll"
		"$fromModule\FSharp.Compiler.Service.dll"
		"$fromModule\System.Reflection.Metadata.dll"
	)
}

task NuGet Package, Version, {
	# test versions
	$dllPath = "$FarHome\FarNet\Modules\$ModuleName\$ModuleName.dll"
	($dllVersion = (Get-Item $dllPath).VersionInfo.FileVersion.ToString())
	assert $dllVersion.StartsWith($Version) 'Versions mismatch.'

	$text = @'
F# interactive, scripting, compiler, and editor services for Far Manager.

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

task path {
	Add-Path tools, packages\FSharp.Compiler.Service.ProjectCracker\utilities\net45
}

task . Build, Clean
