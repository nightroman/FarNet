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
}

task Kill Clean, {
	remove @(
		'packages'
		'paket-files'
		'src\.vs'
		'src\Directory.Build.props'
	)
}

task Build {
	exec {dotnet build $ProjectRoot\$ModuleName.sln /p:FarHome=$FarHome /p:Configuration=$Configuration /v:n}
}

task Clean {
	remove @(
		'z'
		'README.htm'
		'src\FSharpFar.fs.ini'
		"FarNet.$ModuleName.*.nupkg"
		"$ProjectRoot\*\bin"
		"$ProjectRoot\*\obj"
	)
}

task Markdown {
	assert (Test-Path $env:MarkdownCss)
	exec { pandoc.exe @(
		'README.md'
		'--output=README.htm'
		'--from=gfm'
		'--self-contained', "--css=$env:MarkdownCss"
		'--standalone', "--metadata=pagetitle=$ModuleName"
	)}
}

task Version {
	($script:Version = switch -regex -file History.txt {'^= (\d+\.\d+\.\d+) =$' {$matches[1]; break}})
}

task Meta -Inputs .build.ps1, History.txt -Outputs src/Directory.Build.props -Jobs Version, {
	Set-Content src/Directory.Build.props @"
<Project>
	<PropertyGroup>
		<Company>https://github.com/nightroman/FarNet</Company>
		<Copyright>Copyright (c) Roman Kuzmin</Copyright>
		<Description>F# interactive, scripting, compiler, and editor services for Far Manager.</Description>
		<Product>FarNet.FSharpFar</Product>
		<Version>$Version</Version>
		<FileVersion>$Version</FileVersion>
		<AssemblyVersion>$Version</AssemblyVersion>
	</PropertyGroup>
</Project>
"@
}

task Package Markdown, {
	$toModule = "z\tools\FarHome\FarNet\Modules\$ModuleName"
	$fromModule = "$FarHome\FarNet\Modules\$ModuleName"

	remove z
	$null = mkdir $toModule

	# package: logo
	Copy-Item -Destination z ..\Zoo\FarNetLogo.png

	# FarHome: required here by FCS, available for F# modules
	Copy-Item -Destination "z\tools\FarHome" @(
		"$FarHome\FSharp.Core.dll"
		"$FarHome\FSharp.Core.optdata"
		"$FarHome\FSharp.Core.sigdata"
	)

	# FarNet: available for F# modules
	Copy-Item -Destination "z\tools\FarHome\FarNet" @(
		"$FarHome\FarNet\FarNet.FSharp.dll"
		"$FarHome\FarNet\FarNet.FSharp.xml"
	)

	# module
	Copy-Item -Destination $toModule @(
		'README.htm'
		'History.txt'
		'LICENSE.txt'
		"$fromModule\$ModuleName.dll"
		"$fromModule\FSharp.Compiler.Service.dll"
		"$fromModule\System.Buffers.dll"
		"$fromModule\System.Reflection.Metadata.dll"
		"$fromModule\System.ValueTuple.dll"
	)
}

task NuGet Package, Version, {
	# test versions
	$dllPath = "$FarHome\FarNet\Modules\$ModuleName\$ModuleName.dll"
	($dllVersion = (Get-Item $dllPath).VersionInfo.FileVersion.ToString())
	assert $dllVersion.StartsWith("$Version.") 'Versions mismatch.'

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
		<icon>FarNetLogo.png</icon>
		<license type="expression">BSD-3-Clause</license>
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
