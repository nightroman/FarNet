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

# (@1) Use MSBuild to work around missed assembly version
# see https://github.com/Microsoft/visualfsharp/issues/3113
task Build {
	# dotnet build misses version info (@1)
	#exec {dotnet build $ProjectRoot\$ModuleName.sln /p:FarHome=$FarHome /p:Configuration=$Configuration /v:n}

	# workaround (@1)
	Set-Alias MSBuild (Resolve-MSBuild x86)
	exec {dotnet restore $ProjectRoot\$ModuleName.sln}
	exec {MSBuild $ProjectRoot\$ModuleName.sln /p:FarHome=$FarHome /p:Configuration=$Configuration /v:n}
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
	function Convert-Markdown($Name) {pandoc.exe --standalone --from=gfm "--output=$Name.htm" "--metadata=pagetitle=$Name" "$Name.md"}
	exec { Convert-Markdown README }
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
	$toHome = "z\tools\FarHome"
	$toFarNet = "z\tools\FarHome\FarNet"
	$toModule = "$toHome\FarNet\Modules\$ModuleName"
	$fromModule = "$FarHome\FarNet\Modules\$ModuleName"

	remove z
	$null = mkdir $toModule

	Copy-Item -Destination $toHome @(
		"$FarHome\FSharp.Core.dll"
		"$FarHome\FSharp.Core.optdata"
		"$FarHome\FSharp.Core.sigdata"
	)

	Copy-Item -Destination $toFarNet @(
		"$FarHome\FarNet\FarNet.FSharp.dll"
		"$FarHome\FarNet\FarNet.FSharp.xml"
	)

	Copy-Item -Destination $toModule @(
		'README.htm'
		'History.txt'
		'LICENSE.txt'
		"$fromModule\$ModuleName.dll"
		"$fromModule\FSharp.Compiler.Service.dll"
		"$fromModule\System.Reflection.Metadata.dll"
		"$fromModule\System.ValueTuple.dll"
	)

	# icon
	$null = mkdir z\images
	Copy-Item ..\Zoo\FarNetLogo.png z\images
}

#! dotnet made assembly: FileVersion is null (@1); so we used this command:
# ($dllVersion = [Reflection.Assembly]::ReflectionOnlyLoadFrom($dllPath).GetName().Version.ToString())
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
		<icon>images\FarNetLogo.png</icon>
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
