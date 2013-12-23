
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param
(
	$Platform = (property Platform Win32),
	$Configuration = (property Configuration Release)
)
$FarHome = "C:\Bin\Far\$Platform"
$PsfHome = "$FarHome\FarNet\Modules\PowerShellFar"

task Clean {
	Remove-Item -Force -Recurse -ErrorAction 0 -LiteralPath `
	bin, obj, Modules\FarDescription\bin, Modules\FarDescription\obj
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

task Zip Help, {
	. ..\Get-Version.ps1
	$dir = 'z\FarNet\Modules\PowerShellFar'
	$draw = 'C:\ROM\FarDev\Draw'

	Remove-Item [z] -Force -Recurse
	$null = mkdir $dir

	Move-Item About-PowerShellFar.htm z
	Copy-Item Install.txt z
	Copy-Item History.txt, LICENSE, PowerShellFar.macro.lua $dir
	Copy-Item $FarHome\FarNet\Modules\PowerShellFar\* $dir -Recurse
	Copy-Item Bench $dir -Recurse -Force

	Push-Location z
	exec { & 7z a ..\PowerShellFar.$PowerShellFarVersion.7z * }
	Pop-Location

	Remove-Item z -Recurse -Force
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
