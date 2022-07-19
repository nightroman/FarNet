﻿<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Configuration = (property Configuration Release),
	$FarHome = (property FarHome C:\Bin\Far\x64)
)

Set-StrictMode -Version 3
$ModuleName = "PowerShellFar"
$ModuleHome = "$FarHome\FarNet\Modules\$ModuleName"

task clean {
	remove z, bin, obj, About-PowerShellFar.htm
}

# Install all. Run after Build.
task publish installBin, installRes

task uninstall {
	if (Test-Path $ModuleHome) { Remove-Item $ModuleHome -Recurse -Force }
}

task markdown {
	# HLF
	exec { pandoc.exe README.md --output=About-PowerShellFar.htm --from=gfm }
	exec { HtmlToFarHelp from=About-PowerShellFar.htm to=$ModuleHome\PowerShellFar.hlf }

	# HTM
	assert (Test-Path $env:MarkdownCss)
	exec {
		pandoc.exe @(
			'README.md'
			'--output=About-PowerShellFar.htm'
			'--from=gfm'
			'--self-contained'
			"--css=$env:MarkdownCss"
			'--metadata=pagetitle:PowerShellFar'
		)
	}
}

task installBin {
	exec { dotnet publish "$ModuleName.csproj" -c $Configuration -o $ModuleHome --no-build }
	Remove-Item "$ModuleHome\PowerShellFar.deps.json"

	#rk-0 or cannot compile C# in PS ~ cannot find '...\PowerShellFar\runtimes\win\lib\net6.0\ref'.
	#?? is it PS issue? use my plugin repo and report
	# move `ref` folder to "expected" location
	exec { robocopy "$ModuleHome\ref" "$ModuleHome\runtimes\win\lib\net6.0\ref" /s } (0..2)
	Remove-Item -LiteralPath "$ModuleHome\ref" -Force -Recurse

	Set-Location $ModuleHome
	remove cs, de, es, fr, it, ja, ko, pl, pt-BR, ru, tr, zh-Hans, zh-Hant

	#! keep unix
	Set-Location runtimes
	remove freebsd, illumos, ios, linux*, osx*, solaris, tvos, win-arm*
}

task installRes {
	exec { robocopy . $ModuleHome PowerShellFar.ps1 TabExpansion2.ps1 TabExpansion.txt } (0..2)
	exec { robocopy Modules\FarInventory $ModuleHome\Modules\FarInventory about_FarInventory.help.txt FarInventory.psm1 } (0..2)
}

# Build PowerShell help if FarHost else Write-Warning.
task help -Inputs {Get-Item Commands\*} -Outputs "$ModuleHome\PowerShellFar.dll-Help.xml" {
	if ($Host.Name -eq 'FarHost') {
		. Helps.ps1
		Convert-Helps "$BuildRoot\Commands\PowerShellFar.dll-Help.ps1" "$Outputs"
	}
	else {
		# let the caller know
		$env:FarNetToBuildPowerShellFarHelp = 1
		Write-Warning "Run task 'help' with PowerShellFar."
	}
}

# Make package files
task package markdown, {
	remove z
	$toModule = mkdir 'z\tools\FarHome\FarNet\Modules\PowerShellFar'

	# module
	exec { robocopy $ModuleHome $toModule /s /xf *.pdb } (0..2)
	equals 320 (Get-ChildItem $toModule -Recurse -File).Count

	# logo
	Copy-Item -Destination z ..\Zoo\FarNetLogo.png

	Copy-Item -Destination $toModule -Recurse -Force @(
		"Bench"
		"About-PowerShellFar.htm"
		"History.txt"
		"..\LICENSE"
		"PowerShellFar.macro.lua"
	)
}

# Set version
task version {
	. ..\Get-Version.ps1
	($script:Version = $PowerShellFarVersion)
}

# Make NuGet package
task nuget package, version, {
	$description = @'
PowerShellFar is the FarNet module for Far Manager, the file manager.
It is the Windows PowerShell host in the genuine console environment.

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
		<id>FarNet.PowerShellFar</id>
		<version>$Version</version>
		<authors>Roman Kuzmin</authors>
		<owners>Roman Kuzmin</owners>
		<projectUrl>https://github.com/nightroman/FarNet</projectUrl>
		<icon>FarNetLogo.png</icon>
		<license type="expression">BSD-3-Clause</license>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<description>$description</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/master/PowerShellFar/History.txt</releaseNotes>
		<tags>FarManager FarNet PowerShell Module Plugin</tags>
	</metadata>
</package>
"@
	# pack
	exec { nuget pack z\Package.nuspec -NoPackageAnalysis }
}
