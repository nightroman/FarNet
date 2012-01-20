
<#
.Synopsis
	Build script (https://github.com/nightroman/Invoke-Build)
#>

param
(
	$FarHome = (property FarHome),
	$Configuration = (property Configuration Release)
)

task Clean {
	Remove-Item -Force -Recurse -ErrorAction 0 -LiteralPath @(
		'bin'
		'obj'
		'Modules\FarDescription\bin'
		'Modules\FarDescription\obj'
	)
}

# Install all. Run after Build.
task Install InstallBin, InstallRes, BuildPowerShellFarHelp

task Uninstall {
	$dir = "$FarHome\FarNet\Modules\PowerShellFar"
	if (Test-Path $dir) { Remove-Item $dir -Recurse -Force }
}

task Zip {
	. ..\Get-Version.ps1
	$dir = 'z\FarNet\Modules\PowerShellFar'
	$draw = 'C:\ROM\FarDev\Draw'

	Remove-Item [z] -Force -Recurse
	$null = mkdir $dir\Extras

	Copy-Item Readme.txt, Install.txt z
	Copy-Item History.txt, LICENSE, PowerShellFar.farconfig $dir
	Copy-Item $FarHome\FarNet\Modules\PowerShellFar\* $dir -Recurse
	Copy-Item Bench $dir -Recurse -Force
	Copy-Item $draw\PowerShell.hrc, $draw\RomanConsole.hrd, $draw\RomanRainbow.hrd $dir\Extras

	Push-Location z
	exec { & 7z a ..\PowerShellFar.$PowerShellFarVersion.7z * }
	Pop-Location

	Remove-Item z -Recurse -Force
}

task InstallBin {
	$dir = "$FarHome\FarNet\Modules\PowerShellFar"
	exec { robocopy Bin\$Configuration $dir PowerShellFar.dll PowerShellFar.xml /np } (0..2)
	exec { robocopy Modules\FarDescription\Bin\$Configuration $dir\Modules\FarDescription FarDescription.dll /np } (0..2)
}

task InstallRes {
	$dir = "$FarHome\FarNet\Modules\PowerShellFar"
	exec { robocopy . $dir PowerShellFar.hlf TabExpansion.ps1 TabExpansion#.txt /np } (0..2)
	exec { robocopy Modules\FarDescription $dir\Modules\FarDescription about_FarDescription.help.txt FarDescription.psd1 FarDescription.psm1 FarDescription.Types.ps1xml /np } (0..2)
	exec { robocopy Modules\FarInventory $dir\Modules\FarInventory about_FarInventory.help.txt FarInventory.psm1 /np } (0..2)
}

task BuildPowerShellFarHelp -Incremental @{{ Get-Item Commands\* } = "$FarHome\FarNet\Modules\PowerShellFar\PowerShellFar.dll-Help.xml"} {
	Add-Type -Path $FarHome\FarNet\FarNet.dll
	Add-Type -Path $FarHome\FarNet\FarNet.Settings.dll
	Add-Type -Path $FarHome\FarNet\FarNet.Tools.dll
	Add-Type -Path $FarHome\FarNet\Modules\PowerShellFar\PowerShellFar.dll
	$ps = [Management.Automation.PowerShell]::Create()
	$configuration = [Management.Automation.Runspaces.RunspaceConfiguration]::Create()
	[PowerShellFar.Zoo]::Initialize($Configuration)
	$ps.Runspace = [Management.Automation.Runspaces.RunspaceFactory]::CreateRunspace($Configuration)
	$ps.Runspace.Open()
	$null = $ps.AddScript(@"
. Helps.ps1
Convert-Helps "$BuildRoot\Commands\PowerShellFar.dll-Help.ps1" "$Outputs"
"@)
	$ps.Invoke()
}
