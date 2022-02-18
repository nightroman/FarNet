<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$FarHome = (property FarHome C:\Bin\Far\x64),
	$Configuration = (property Configuration Release)
)

$ModuleName = 'FSharpFar'
$ProjectName = "$ModuleName.fsproj"

task init meta, {
	exec {dotnet tool restore}
	exec {dotnet paket install}
}

task kill clean, {
	remove @(
		'packages'
		'paket-files'
		'src\.vs'
		'src\Directory.Build.props'
	)
}

task build {
	assert (Test-Path packages) 'Please, ib init'
	exec {dotnet build "src\$ModuleName.sln" "/p:FarHome=$FarHome" "/p:Configuration=$Configuration"}
}

task clean {
	remove @(
		'z'
		'README.htm'
		"FarNet.$ModuleName.*.nupkg"
		"src\*\bin"
		"src\*\obj"
	)
}

task markdown {
	assert (Test-Path $env:MarkdownCss)
	exec { pandoc.exe @(
		'README.md'
		'--output=README.htm'
		'--from=gfm'
		'--self-contained', "--css=$env:MarkdownCss"
		'--standalone', "--metadata=pagetitle=$ModuleName"
	)}
}

task version {
	($script:Version = switch -regex -file History.txt {'^= (\d+\.\d+\.\d+) =$' {$matches[1]; break}})
}

task meta -Inputs .build.ps1, History.txt -Outputs src/Directory.Build.props -Jobs version, {
	Set-Content src/Directory.Build.props @"
<Project>
	<PropertyGroup>
		<Company>https://github.com/nightroman/FarNet</Company>
		<Copyright>Copyright (c) Roman Kuzmin</Copyright>
		<Description>F# scripting and interactive services in Far Manager</Description>
		<Product>FarNet.FSharpFar</Product>
		<Version>$Version</Version>
	</PropertyGroup>
</Project>
"@
}

task package markdown, {
	remove z
	$toModule = mkdir "z\tools\FarHome\FarNet\Modules\$ModuleName"
	$fromModule = "$FarHome\FarNet\Modules\$ModuleName"

	# assert module files, to copy all by *
	$moduleFiles = (Get-ChildItem $fromModule -Name | Out-String).Trim()
	equals $moduleFiles @'
FSharp.Compiler.Service.dll
FSharp.DependencyManager.Nuget.dll
FSharpFar.dll
Microsoft.Build.Framework.dll
Microsoft.Build.Tasks.Core.dll
Microsoft.Build.Utilities.Core.dll
System.Buffers.dll
System.Collections.Immutable.dll
System.Memory.dll
System.Numerics.Vectors.dll
System.Reflection.Metadata.dll
System.Runtime.CompilerServices.Unsafe.dll
'@

	# package: logo
	Copy-Item -Destination z ..\Zoo\FarNetLogo.png

	# FarHome: FSharp.Core.* required here by FCS, available for F# modules
	Copy-Item -Destination "z\tools\FarHome" @(
		"$FarHome\FSharp.Core.dll"
		"$FarHome\FSharp.Core.xml"
		"$FarHome\fsx.exe"
		"$FarHome\fsx.exe.config"
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
		'..\LICENSE'
		"$fromModule\*"
	)
}

task nuget package, version, {
	# test versions
	$dllPath = "$FarHome\FarNet\Modules\$ModuleName\$ModuleName.dll"
	($dllVersion = (Get-Item $dllPath).VersionInfo.FileVersion.ToString())
	assert $dllVersion.StartsWith("$Version.") 'Versions mismatch.'

	$description = @'
F# scripting and interactive services in Far Manager

---

How to install and update FarNet and modules:

https://github.com/nightroman/FarNet#readme
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
		<description>$description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/master/FSharpFar/History.txt</releaseNotes>
		<tags>FarManager FarNet Module FSharp</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack z\Package.nuspec }
}

task test_testing {
	Start-Far "fs: //exec file=$env:FarNetCode\FSharpFar\samples\Testing\App1.fsx" -ReadOnly -Title Testing -Environment @{QuitFarAfterTests=1}
}

task test_tests {
	Start-Far "fs: //exec file=$env:FarNetCode\FSharpFar\tests\App1.fsx" -ReadOnly -Title Tests -Environment @{QuitFarAfterTests=1}
}

task test_tasks {
	Start-Far "ps: Test.far.ps1 * -Quit #" $env:FarNetCode\Test\FSharpFar.test -ReadOnly -Title Tasks
}

task test_fsx {
	Invoke-Build Test src\fsx\.build.ps1
}

task test test_tasks, test_tests, test_testing, test_fsx

task . build, clean
