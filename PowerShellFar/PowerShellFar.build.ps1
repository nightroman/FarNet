<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$FarHome = (property FarHome C:\Bin\Far\x64),
	$Configuration = (property Configuration Release)
)
$PsfHome = "$FarHome\FarNet\Modules\PowerShellFar"

task clean {
	remove z, bin, obj, About-PowerShellFar.htm
}

# Install all. Run after Build.
task install installBin, installRes

task uninstall {
	if (Test-Path $PsfHome) { Remove-Item $PsfHome -Recurse -Force }
}

task help {
	# HLF
	exec { pandoc.exe README.md --output=About-PowerShellFar.htm --from=gfm }
	exec { HtmlToFarHelp from=About-PowerShellFar.htm to=$PsfHome\PowerShellFar.hlf }

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
	exec { robocopy Bin\$Configuration $PsfHome PowerShellFar.dll PowerShellFar.xml /r:0 } (0..2)
}

task installRes {
	exec { robocopy . $PsfHome PowerShellFar.ps1 TabExpansion2.ps1 TabExpansion.txt } (0..2)
	exec { robocopy Modules\FarInventory $PsfHome\Modules\FarInventory about_FarInventory.help.txt FarInventory.psm1 } (0..2)
	exec { robocopy Modules\FarPackage $PsfHome\Modules\FarPackage } (0..2)
}

# Run when FarNet and PowerShellFar are installed.
task buildPowerShellFarHelp -Inputs {Get-Item Commands\*} -Outputs "$PsfHome\PowerShellFar.dll-Help.xml" {
	Add-Type -Path $FarHome\FarNet\FarNet.dll
	Add-Type -Path $FarHome\FarNet\FarNet.Settings.dll
	Add-Type -Path $FarHome\FarNet\FarNet.Tools.dll
	Add-Type -Path $PsfHome\PowerShellFar.dll
	$ps = [Management.Automation.PowerShell]::Create()
	$state = [Management.Automation.Runspaces.InitialSessionState]::CreateDefault()
	[PowerShellFar.Zoo]::Initialize($state)
	$ps.Runspace = [Management.Automation.Runspaces.RunspaceFactory]::CreateRunspace($state)
	$ps.Runspace.Open()
	#! $ErrorActionPreference = 1 in Convert-Helps does not help to catch errors
	$null = $ps.AddScript(@"
`$ErrorActionPreference = 1
. Helps.ps1
Convert-Helps "$BuildRoot\Commands\PowerShellFar.dll-Help.ps1" "$Outputs"
"@)
	$ps.Invoke()
}

# Make package files
task package help, {
	remove z
	$dirMain = mkdir 'z\tools\FarHome\FarNet\Modules\PowerShellFar'

	Copy-Item -Destination $dirMain About-PowerShellFar.htm, History.txt, LICENSE.txt, PowerShellFar.macro.lua
	Copy-Item -Destination $dirMain $FarHome\FarNet\Modules\PowerShellFar\* -Recurse
	Copy-Item -Destination $dirMain Bench -Recurse -Force
	Copy-Item ..\Zoo\FarNetLogo.png z
}

# Set version
task version {
	. ..\Get-Version.ps1
	($script:Version = $PowerShellFarVersion)
}

# Make NuGet package
task nuget package, version, {
	$text = @'
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
		<summary>$text</summary>
		<description>$text</description>
		<releaseNotes>https://github.com/nightroman/FarNet/blob/master/PowerShellFar/History.txt</releaseNotes>
		<tags>FarManager FarNet PowerShell Module Plugin</tags>
	</metadata>
</package>
"@
	# pack
	exec { nuget pack z\Package.nuspec -NoPackageAnalysis }
}
