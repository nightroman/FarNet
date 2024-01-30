<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$FarHome = (property FarHome C:\Bin\Far\x64),
	$Configuration = (property Configuration Release)
)

Set-StrictMode -Version 3
$ModuleName = 'JavaScriptFar'
$ModuleRoot = "$FarHome\FarNet\Modules\$ModuleName"
$Description = 'JavaScript scripting in Far Manager.'

task meta -Inputs $BuildFile, History.txt -Outputs Directory.Build.props -Jobs version, {
	Set-Content Directory.Build.props @"
<Project>
	<PropertyGroup>
		<Description>$Description</Description>
		<Company>https://github.com/nightroman/FarNet</Company>
		<Copyright>Copyright (c) Roman Kuzmin</Copyright>
		<Product>FarNet.$ModuleName</Product>
		<Version>$Version</Version>
		<IncludeSourceRevisionInInformationalVersion>False</IncludeSourceRevisionInInformationalVersion>
	</PropertyGroup>
</Project>
"@
}

task build meta, {
	exec { dotnet build -c $Configuration -p:FarHome=$FarHome }
}

task clean {
	remove z, bin, obj, README.htm, *.nupkg
}

task version {
	($script:Version = switch -regex -file History.txt {'^= (\d+\.\d+\.\d+) =$' {$matches[1]; break}})
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

	# nuget
	Copy-Item -Destination z @(
		'README.md'
		'..\Zoo\FarNetLogo.png'
	)

	# about
	Copy-Item -Destination $toModule @(
		'README.htm'
		'History.txt'
		'..\LICENSE'
	)

	Assert-SameFile -Text -View $env:MERGE -Result (Get-ChildItem $toModule -Force -Recurse -File -Name | Out-String) -Sample @'
ClearScript.Core.dll
ClearScript.V8.dll
ClearScript.V8.ICUData.dll
ClearScript.Windows.Core.dll
ClearScript.Windows.dll
History.txt
JavaScriptFar.deps.json
JavaScriptFar.dll
JavaScriptFar.runtimeconfig.json
LICENSE
Newtonsoft.Json.dll
README.htm
runtimes\win-x64\native\ClearScriptV8.win-x64.dll
runtimes\win-x86\native\ClearScriptV8.win-x86.dll
'@
}

task nuget package, version, {
	$dllVersion = (Get-Item "$FarHome\FarNet\Modules\$ModuleName\$ModuleName.dll").VersionInfo.ProductVersion
	equals $Version $dllVersion

	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.$ModuleName</id>
		<version>$Version</version>
		<authors>Roman Kuzmin</authors>
		<owners>Roman Kuzmin</owners>
		<projectUrl>https://github.com/nightroman/FarNet/tree/main/$ModuleName</projectUrl>
		<icon>FarNetLogo.png</icon>
		<readme>README.md</readme>
		<license type="expression">BSD-3-Clause</license>
		<description>$Description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/main/$ModuleName/History.txt</releaseNotes>
		<tags>FarManager FarNet Module JavaScript ClearScript V8</tags>
	</metadata>
</package>
"@

	exec { NuGet pack z\Package.nuspec }
}

task test {
	Start-Far "ps: Test.far.ps1 * -Quit" $env:FarNetCode\$ModuleName\Tests -ReadOnly -Title JavaScriptFar\test
}

task . build, clean
