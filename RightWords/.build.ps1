<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

Set-StrictMode -Version 3
$ModuleName = 'RightWords'
$ModuleRoot = "$FarHome\FarNet\Modules\$ModuleName"
$Description = 'Spell-checker. FarNet module for Far Manager.'

task build meta, {
	exec { dotnet build -c $Configuration /p:FarHome=$FarHome }
}

task publish {
	exec { dotnet publish "$ModuleName.csproj" -c $Configuration -o $ModuleRoot --no-build }
},
help,
resgen

task help @{
	Inputs = 'README.md'
	Outputs = "$ModuleRoot\RightWords.hlf"
	Jobs = {
		exec { pandoc.exe README.md --output=z.htm --from=gfm }
		exec { HtmlToFarHelp from=z.htm to=$ModuleRoot\RightWords.hlf }
		remove z.htm
	}
}

# https://github.com/nightroman/PowerShelf/blob/main/Invoke-Environment.ps1
task resgen @{
	Inputs = 'RightWords.restext', 'RightWords.ru.restext'
	Outputs = "$ModuleRoot\RightWords.resources", "$ModuleRoot\RightWords.ru.resources"
	Partial = $true
	Jobs = {
		begin {
			$VsDevCmd = @(Get-Item "$env:ProgramFiles\Microsoft Visual Studio\2022\*\Common7\Tools\VsDevCmd.bat")
			Invoke-Environment.ps1 -File ($VsDevCmd[0])
		}
		process {
			exec {resgen.exe $_ $2}
		}
	}
}

task clean {
	remove z, bin, obj, README.htm, *.nupkg
}

task version {
	($script:Version = switch -regex -file History.txt {'^= (\d+\.\d+\.\d+) =$' {$matches[1]; break}})
	assert $script:Version
}

task meta -Inputs .build.ps1, History.txt -Outputs Directory.Build.props -Jobs version, {
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

task markdown {
	assert (Test-Path $env:MarkdownCss)
	exec { pandoc.exe @(
		'README.md'
		'--output=README.htm'
		'--from=gfm'
		'--embed-resources'
		'--standalone'
		"--css=$env:MarkdownCss"
		'--standalone', '--metadata=pagetitle=RightWords'
	)}
}

task package markdown, version, {
	remove z
	$toModule = mkdir "z\tools\FarHome\FarNet\Modules\$ModuleName"

	# module
	exec { robocopy $ModuleRoot $toModule /s /xf *.pdb } (0..2)
	equals 7 (Get-ChildItem $toModule -Recurse -File).Count

	# meta
	Copy-Item -Destination z @(
		'README.md'
		'..\Zoo\FarNetLogo.png'
	)

	# module
	Copy-Item -Destination $toModule @(
		"README.htm"
		"History.txt"
		"..\LICENSE"
		"RightWords.macro.lua"
	)

	$result = Get-ChildItem $toModule -Recurse -File -Name | Out-String
	$sample = @'
History.txt
LICENSE
README.htm
RightWords.deps.json
RightWords.dll
RightWords.hlf
RightWords.macro.lua
RightWords.resources
RightWords.ru.resources
RightWords.runtimeconfig.json
WeCantSpell.Hunspell.dll
'@
	Assert-SameFile.ps1 -Text $sample $result $env:MERGE
}

task nuget package, version, {
	equals $Script:Version (Get-Item "$ModuleRoot\$ModuleName.dll").VersionInfo.ProductVersion

	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.RightWords</id>
		<version>$script:Version</version>
		<authors>Roman Kuzmin</authors>
		<owners>Roman Kuzmin</owners>
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<icon>FarNetLogo.png</icon>
		<readme>README.md</readme>
		<license type="expression">BSD-3-Clause</license>
		<description>$Description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/main/RightWords/History.txt</releaseNotes>
		<tags>FarManager FarNet Module Hunspell</tags>
	</metadata>
</package>
"@

	exec { NuGet pack z\Package.nuspec }
}

task . build, clean
