
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param(
	$Platform = (property Platform Win32),
	$Configuration = (property Configuration Release)
)
$FarHome = "C:\Bin\Far\$Platform"
$PsfHome = "$FarHome\FarNet\Modules\PowerShellFar"

task Clean {
	Remove-Item -Force -Recurse -ErrorAction 0 -LiteralPath `
	z, bin, obj, Modules\FarDescription\bin, Modules\FarDescription\obj, About-PowerShellFar.htm
}

# Install all. Run after Build.
task Install InstallBin, InstallRes, BuildPowerShellFarHelp

task Uninstall {
	if (Test-Path $PsfHome) { Remove-Item $PsfHome -Recurse -Force }
}

task Help {
	exec { MarkdownToHtml "From=About-PowerShellFar.text" "To=About-PowerShellFar.htm" }
	exec { HtmlToFarHelp "From=About-PowerShellFar.htm" "To=$PsfHome\PowerShellFar.hlf" }
}

task InstallBin {
	exec { robocopy Bin\$Configuration $PsfHome PowerShellFar.dll PowerShellFar.xml /np } (0..2)
	exec { robocopy Modules\FarDescription\Bin\$Configuration $PsfHome\Modules\FarDescription FarDescription.dll /np } (0..2)
}

task InstallRes {
	exec { robocopy . $PsfHome TabExpansion.ps1 TabExpansion2.ps1 TabExpansion.txt /np } (0..2)
	exec { robocopy Modules\FarDescription $PsfHome\Modules\FarDescription about_FarDescription.help.txt FarDescription.psd1 FarDescription.psm1 FarDescription.Types.ps1xml /np } (0..2)
	exec { robocopy Modules\FarInventory $PsfHome\Modules\FarInventory about_FarInventory.help.txt FarInventory.psm1 /np } (0..2)
}

task BuildPowerShellFarHelp -Inputs {Get-Item Commands\*} -Outputs "$PsfHome\PowerShellFar.dll-Help.xml" {
	Add-Type -Path $FarHome\FarNet\FarNet.dll
	Add-Type -Path $FarHome\FarNet\FarNet.Settings.dll
	Add-Type -Path $FarHome\FarNet\FarNet.Tools.dll
	Add-Type -Path $FarHome\FarNet\Modules\PowerShellFar\PowerShellFar.dll
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
task Package Help, {
	$dirRoot = 'z\tools'
	$dirMain = 'z\tools\FarHome\FarNet\Modules\PowerShellFar'

	Remove-Item [z] -Force -Recurse
	$null = mkdir $dirMain

	Copy-Item -Destination $dirRoot About-PowerShellFar.htm, History.txt
	Copy-Item -Destination $dirMain LICENSE.txt, PowerShellFar.macro.lua
	Copy-Item -Destination $dirMain $FarHome\FarNet\Modules\PowerShellFar\* -Recurse
	Copy-Item -Destination $dirMain Bench -Recurse -Force
}

# Set version
task Version {
	. ..\Get-Version.ps1
	$script:Version = $PowerShellFarVersion
	$Version
}

# Make NuGet package
task NuGet Package, Version, {
	$text = @'
PowerShellFar is the FarNet module for Far Manager, the file manager.
It is the Windows PowerShell host in the genuine console environment.

---

To install and update FarNet packages, follow these steps:

https://farnet.googlecode.com/svn/trunk/Install-FarNet.en.txt

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
		<projectUrl>https://code.google.com/p/farnet</projectUrl>
		<licenseUrl>http://opensource.org/licenses/BSD-3-Clause</licenseUrl>
		<requireLicenseAcceptance>false</requireLicenseAcceptance>
		<summary>$text</summary>
		<description>$text</description>
		<releaseNotes>https://farnet.googlecode.com/svn/trunk/PowerShellFar/History.txt</releaseNotes>
		<tags>FarManager FarNet PowerShell Module Plugin</tags>
	</metadata>
</package>
"@
	# pack
	exec { NuGet pack z\Package.nuspec -NoPackageAnalysis }
}
