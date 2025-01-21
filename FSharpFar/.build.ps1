<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

Set-StrictMode -Version 3
$ModuleName = 'FSharpFar'
$ModuleRoot = "$FarHome\FarNet\Modules\$ModuleName"
$Description = 'F# scripting and interactive services in Far Manager.'

task build meta, {
	exec { dotnet build "src\$ModuleName.sln" -c $Configuration "/p:FarHome=$FarHome" }
}

task publish {
	exec { dotnet publish "src\$ModuleName\$ModuleName.fsproj" -c $Configuration -o $ModuleRoot --no-build }

	$xml = [xml](Get-Content "src\$ModuleName\$ModuleName.fsproj")
	$node = $xml.SelectSingleNode('Project/ItemGroup/PackageReference[@Include="FSharp.Core"]')
	Copy-Item "$HOME\.nuget\packages\FSharp.Core\$($node.Version)\lib\netstandard2.1\FSharp.Core.xml" $ModuleRoot

	# used to be deleted, now missing: runtimes\unix
	Set-Location $ModuleRoot
	remove cs, de, es, fr, it, ja, ko, pl, pt-BR, ru, tr, zh-Hans, zh-Hant
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

task version {
	($script:Version = switch -regex -file History.txt {'^= (\d+\.\d+\.\d+) =$' {$matches[1]; break}})
}

task meta -Inputs .build.ps1, History.txt -Outputs src/Directory.Build.props -Jobs version, {
	Set-Content src/Directory.Build.props @"
<Project>
	<PropertyGroup>
		<Description>$Description</Description>
		<Company>https://github.com/nightroman/FarNet</Company>
		<Copyright>Copyright (c) Roman Kuzmin</Copyright>
		<Product>FarNet.FSharpFar</Product>
		<Version>$Version</Version>
		<IncludeSourceRevisionInInformationalVersion>False</IncludeSourceRevisionInInformationalVersion>
	</PropertyGroup>
</Project>
"@
}

task markdown {
	assert (Test-Path $env:MarkdownCss)
	exec { pandoc.exe @(
		'README.md'
		'--output=README.htm'
		'--from=gfm'
		'--embed-resources'
		'--standalone'
		"--css=$env:MarkdownCss"
		"--metadata=pagetitle=$ModuleName"
	)}
}

task package markdown, {
	remove z
	$toModule = mkdir "z\tools\FarHome\FarNet\Modules\$ModuleName"

	# module
	exec { robocopy $ModuleRoot $toModule /s /xf *.pdb } 1

	# meta
	Copy-Item -Destination z @(
		'README.md'
		'..\Zoo\FarNetLogo.png'
	)

	# repo
	Copy-Item -Destination $toModule @(
		'README.htm'
		'History.txt'
		'..\LICENSE'
	)

	Assert-SameFile.ps1 -Text -View $env:MERGE -Result (Get-ChildItem $toModule -Recurse -File -Name) -Sample @'
FarNet.FSharp.dll
FarNet.FSharp.xml
FSharp.Compiler.Service.dll
FSharp.Core.dll
FSharp.Core.xml
FSharp.DependencyManager.Nuget.dll
FSharpFar.dll
fsx.dll
fsx.exe
fsx.runtimeconfig.json
History.txt
LICENSE
README.htm
'@
}

task nuget package, version, {
	equals (Get-Item "$ModuleRoot\$ModuleName.dll").VersionInfo.ProductVersion "$Version.0"

	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.FSharpFar</id>
		<version>$Version</version>
		<authors>Roman Kuzmin</authors>
		<owners>Roman Kuzmin</owners>
		<projectUrl>https://github.com/nightroman/FarNet/tree/main/FSharpFar</projectUrl>
		<icon>FarNetLogo.png</icon>
		<readme>README.md</readme>
		<license type="expression">BSD-3-Clause</license>
		<description>$Description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/main/FSharpFar/History.txt</releaseNotes>
		<tags>FarManager FarNet Module FSharp</tags>
	</metadata>
</package>
"@

	exec { NuGet pack z\Package.nuspec }
}

task test_psf_ib {
	Start-Far 'ps: Invoke-Build ** tests\PSF.test' -Test 500
}

task test_psf_fas {
	Start-Far "ps: Test.far.ps1 * -Quit" $env:FarNetCode\FSharpFar\tests\PSF.test -ReadOnly
}

task test_testing {
	Start-Far "fs:exec file=$env:FarNetCode\FSharpFar\samples\Testing\App1.fsx" -ReadOnly -Environment @{QuitFarAfterTests=1}
}

task test_tests {
	Start-Far "fs:exec file=$env:FarNetCode\FSharpFar\tests\App1.fsx" -ReadOnly -Environment @{QuitFarAfterTests=1}
}

task test_fsx {
	Invoke-Build test src\fsx
}

task test test_psf_ib, test_psf_fas, test_testing, test_tests, test_fsx

task . build, clean
