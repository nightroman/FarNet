<#
.Synopsis
	Build script, https://github.com/nightroman/Invoke-Build
#>

param(
	$FarHome = (property FarHome C:\Bin\Far\x64),
	$Configuration = (property Configuration Release)
)

$ModuleName = 'EditorKit'
$ModuleHome = "$FarHome\FarNet\Modules\$ModuleName"

task build meta, {
	exec {dotnet build "$ModuleName.csproj" "/p:FarHome=$FarHome" "/p:Configuration=$Configuration"}
}

task publish {
	Copy-Item -Destination $ModuleHome @(
		"bin\$Configuration\net6.0\$ModuleName.dll"
		"bin\$Configuration\net6.0\$ModuleName.pdb"
		"bin\$Configuration\net6.0\EditorConfig.Core.dll"
	)
}

task clean {
	remove z, bin, obj, README.htm, Directory.Build.props, "*$ModuleName.*.nupkg"
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

task meta -Inputs .build.ps1, History.txt -Outputs Directory.Build.props -Jobs version, {
	Set-Content Directory.Build.props @"
<Project>
	<PropertyGroup>
		<Company>https://github.com/nightroman/FarNet</Company>
		<Copyright>Copyright (c) Roman Kuzmin</Copyright>
		<Description>FarNet module for Far Manager editor configuration</Description>
		<Product>FarNet.$ModuleName</Product>
		<Version>$Version</Version>
		<FileVersion>$Version</FileVersion>
		<AssemblyVersion>$Version</AssemblyVersion>
	</PropertyGroup>
</Project>
"@
}

task package markdown, {
	remove z
	$toModule = mkdir "z\tools\FarHome\FarNet\Modules\$ModuleName"
	$fromModule = "$FarHome\FarNet\Modules\$ModuleName"

	# logo
	Copy-Item -Destination z ..\Zoo\FarNetLogo.png

	# module
	Copy-Item -Destination $toModule @(
		'README.htm'
		'History.txt'
		'..\LICENSE'
		"$fromModule\$ModuleName.dll"
		"$fromModule\EditorConfig.core.dll"
	)
}

task nuget package, version, {
	# test versions
	$dllPath = "$FarHome\FarNet\Modules\$ModuleName\$ModuleName.dll"
	($dllVersion = (Get-Item $dllPath).VersionInfo.FileVersion.ToString())
	equals $dllVersion $Version

	$text = @'
FarNet module for Far Manager editor configuration

---

How to install and update FarNet and modules:

https://github.com/nightroman/FarNet#readme

---
'@
	# nuspec
	Set-Content z\Package.nuspec @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
	<metadata>
		<id>FarNet.$ModuleName</id>
		<version>$Version</version>
		<authors>Roman Kuzmin</authors>
		<owners>Roman Kuzmin</owners>
		<projectUrl>https://github.com/nightroman/FarNet/tree/master/$ModuleName</projectUrl>
		<icon>FarNetLogo.png</icon>
		<license type="expression">BSD-3-Clause</license>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<description>$text</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/master/$ModuleName/History.txt</releaseNotes>
		<tags>FarManager FarNet Module EditorConfig</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack z\Package.nuspec }
}

task . build, clean
