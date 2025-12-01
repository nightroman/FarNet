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
	exec { dotnet build -c $Configuration "/p:FarHome=$FarHome" -tl:off }
}

task publish {
	$xml = [xml](Get-Content "src\$ModuleName\$ModuleName.fsproj")
	$node = $xml.SelectSingleNode('Project/ItemGroup/PackageReference[@Include="FSharp.Core"]')
	Copy-Item "$HOME\.nuget\packages\FSharp.Core\$($node.Version)\lib\netstandard2.1\FSharp.Core.xml" $ModuleRoot
}

task clean {
	remove @(
		'z'
		'README.html'
		"FarNet.$ModuleName.*.nupkg"
		"src\*\bin"
		"src\*\obj"
	)
}

task version {
	($Script:Version = Get-BuildVersion History.txt '^= (\d+\.\d+\.\d+) =$')
}

task meta -Inputs 1.build.ps1, History.txt -Outputs src/Directory.Build.props -Jobs version, {
	Set-Content src/Directory.Build.props @"
<Project>
	<PropertyGroup>
		<Description>$Description</Description>
		<Company>https://github.com/nightroman/FarNet</Company>
		<Copyright>Copyright (c) Roman Kuzmin</Copyright>
		<Product>FarNet.FSharpFar</Product>
		<Version>$Version</Version>
		<SatelliteResourceLanguages>en</SatelliteResourceLanguages>
		<IncludeSourceRevisionInInformationalVersion>False</IncludeSourceRevisionInInformationalVersion>
	</PropertyGroup>
</Project>
"@
}

task markdown {
	requires -Path $env:MarkdownCss
	exec { pandoc.exe @(
		'README.md'
		'--output=README.html'
		'--from=gfm'
		'--embed-resources'
		'--standalone'
		"--css=$env:MarkdownCss"
		"--metadata=pagetitle=$ModuleName"
	)}
}

task package markdown, {
	remove z
	$toModule = New-Item "z\tools\FarHome\FarNet\Modules\$ModuleName" -Type Directory

	exec { robocopy $ModuleRoot $toModule /s } 1

	Copy-Item -Destination z @(
		'README.md'
		'..\Zoo\FarNetLogo.png'
	)

	Copy-Item -Destination $toModule @(
		'README.html'
		'History.txt'
		'..\LICENSE'
	)

	Assert-SameFile.ps1 -Result (Get-ChildItem $toModule -Recurse -File -Name) -Text -View $env:MERGE -Sample @'
FarNet.FSharp.dll
FarNet.FSharp.pdb
FarNet.FSharp.xml
FSharp.Compiler.Service.dll
FSharp.Core.dll
FSharp.Core.xml
FSharp.DependencyManager.Nuget.dll
FSharpFar.dll
FSharpFar.pdb
fsx.dll
fsx.exe
fsx.pdb
fsx.runtimeconfig.json
History.txt
LICENSE
README.html
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
	exec { pwsf -nop -x 999 -c 'Invoke-Build ** tests\PSF.test' }
}

task test_psf_fas {
	exec { pwsf .\tests\PSF.test -nop -x 999 -c Test-FarNet.ps1 }
}

task test_testing {
	Start-Far "fs:exec file=$env:FarNetCode\FSharpFar\samples\Testing\App1.fsx" -Exit 999
}

task test_tests {
	Start-Far "fs:exec file=$env:FarNetCode\FSharpFar\tests\App1.fsx" -Exit 999
}

task test_fsx {
	Invoke-Build test src\fsx
}

task test test_psf_ib, test_psf_fas, test_testing, test_tests, test_fsx

task . build, clean
