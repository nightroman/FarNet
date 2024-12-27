<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

Set-StrictMode -Version 3
$ModuleName = 'FolderChart'
$ModuleRoot = "$FarHome\FarNet\Modules\$ModuleName"
$Description = 'Shows folder sizes chart. FarNet module for Far Manager.'

task build meta, {
	exec { dotnet build -c $Configuration -p:FarHome=$FarHome }
}

task publish {
	exec { dotnet publish -c $Configuration -o $ModuleRoot --no-build }

	Set-Location $ModuleRoot
	remove runtimes\unix, runtimes\win-arm64
}

task clean {
	remove z, bin, obj, README.htm, *.nupkg
}

task version {
	($script:Version = switch -regex -file History.txt { '^= (\d+\.\d+\.\d+) =$' { $matches[1]; break } })
	assert $script:Version
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

task meta -Inputs .build.ps1, History.txt -Outputs Directory.Build.props -Jobs version, {
	Set-Content Directory.Build.props @"
<Project>
	<PropertyGroup>
		<Company>https://github.com/nightroman/FarNet</Company>
		<Copyright>Copyright (c) Roman Kuzmin</Copyright>
		<Description>$Description</Description>
		<Product>FarNet.$ModuleName</Product>
		<Version>$Version</Version>
		<FileVersion>$Version</FileVersion>
		<AssemblyVersion>$Version</AssemblyVersion>
	</PropertyGroup>
</Project>
"@
}

task package version, markdown, {
	equals $Version (Get-Item $ModuleRoot\$ModuleName.dll).VersionInfo.FileVersion

	remove z
	$toModule = mkdir "z\tools\FarHome\FarNet\Modules\$ModuleName"

	exec { robocopy $ModuleRoot $toModule /s /xf *.pdb } 1

	Copy-Item -Destination z @(
		'README.md'
		'..\Zoo\FarNetLogo.png'
	)

	Copy-Item -Destination $toModule @(
		"README.htm"
		"History.txt"
		"..\LICENSE"
	)

	$result = Get-ChildItem $toModule -Recurse -File -Name | Out-String
	$sample = @'
FolderChart.deps.json
FolderChart.dll
FolderChart.runtimeconfig.json
History.txt
LICENSE
README.htm
System.Data.OleDb.dll
System.Data.SqlClient.dll
System.Windows.Forms.DataVisualization.dll
runtimes\win\lib\net8.0\System.Data.SqlClient.dll
runtimes\win\lib\net9.0\System.Data.OleDb.dll
runtimes\win-x64\native\sni.dll
runtimes\win-x86\native\sni.dll
'@
	Assert-SameFile.ps1 -Text $sample $result $env:MERGE
}

task nuget package, {
	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.FolderChart</id>
		<version>$Version</version>
		<owners>Roman Kuzmin</owners>
		<authors>Roman Kuzmin</authors>
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<icon>FarNetLogo.png</icon>
		<readme>README.md</readme>
		<license type="expression">BSD-3-Clause</license>
		<description>$Description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/main/FolderChart/History.txt</releaseNotes>
		<tags>FarManager FarNet Module</tags>
	</metadata>
</package>
"@

	exec { NuGet pack z\Package.nuspec }
}

task . build, clean
