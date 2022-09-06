<#
.Synopsis
	Tests FarNet updates.

.Description
	** Order of some tests is important **
#>

$ProgressPreference=0

Enter-Build {
	Import-Module $env:FarNetCode\PowerShellFar\Modules\FarPackage
	Set-StrictMode -Version Latest

	$FarHome = "C:\TEMP\FarHome"
	$Packages = @(
		'FarNet'
		'FarNet.PowerShellFar'
		'FarNet.ScottPlot'
	)
	$Version = @{}
}

task GetVersions {
	$Packages | %{
		$r = Install-FarPackage $_ -Version ?
		assert ($r -match '^\d+\.\d+\.\d+$')
		$Version[$_] = $r
	}
	$Version
}

task Install {
	remove $FarHome

	# install and test common items
	$Packages | %{
		# install
		Install-FarPackage $_ -FarHome $FarHome -Platform x64

		# info file
		assert ([System.IO.File]::Exists("$FarHome\Update.$_.info"))
		$s, $v, $f = [System.IO.File]::ReadAllLines("$FarHome\Update.$_.info")
		equals $s NuGet
		equals $v ($Version[$_])
		assert ($f.Count -ge 2)
	}

	# test particular items
	Set-Location $FarHome

	requires -Path FarNet
	requires -Path Plugins
	requires -Path FarNet\Modules
}

#! fixed: "Cannot validate -Platform" (because '' is passed in nested calls)
task UpdateUpToDate {
	Update-FarPackage -FarHome $FarHome
}

task Reinstall.FarNet {
	Set-Location $FarHome

	Uninstall-FarPackage FarNet
	requires -Path FarNet
	assert (!(Test-Path Plugins))
	assert (!(Test-Path Update.FarNet.info))

	Install-FarPackage FarNet -Platform x64
	requires -Path Plugins
	requires -Path Update.FarNet.info
}

task Reinstall.PowerShellFar {
	Set-Location $FarHome

	Uninstall-FarPackage FarNet.PowerShellFar
	requires -Path FarNet
	assert (!(Test-Path FarNet\Modules\PowerShellFar))
	assert (!(Test-Path Update.FarNet.PowerShellFar.info))

	Install-FarPackage FarNet.PowerShellFar
	requires -Path FarNet\Modules\PowerShellFar
	requires -Path Update.FarNet.PowerShellFar.info
}

task Reinstall.ScottPlot {
	Set-Location $FarHome

	Uninstall-FarPackage FarNet.ScottPlot
	requires -Path FarNet
	assert (!(Test-Path FarNet\Lib\ScottPlot))
	assert (!(Test-Path Update.FarNet.ScottPlot.info))

	Install-FarPackage FarNet.ScottPlot
	requires -Path FarNet\Lib\FarNet.ScottPlot
	requires -Path Update.FarNet.ScottPlot.info
}

task FakeOldAndUpdate {
	Set-Location $FarHome

	$Packages[1..2] | %{
		Write-Build Cyan $_

		# fake old
		$info = "$FarHome\Update.$_.info"
		[System.IO.File]::WriteAllText($info, "https://www.nuget.org/api/v2`r`n<fake>")

		# update
		Update-FarPackage -Platform x64

		# updated info
		$s, $v, $f = [System.IO.File]::ReadAllLines($info)
		equals $v ($Version[$_])
	}
}

task PreserveSource {
	Set-Location $FarHome

	# to test updated
	Remove-Item FarNet\Lib\FarNet.ScottPlot\ScottPlot.dll

	# get any nupkg
	$nupkg = @(Get-Item "$env:LOCALAPPDATA\NuGet\Cache\FarNet.ScottPlot.*.nupkg")[0]

	# update from file
	Restore-FarPackage $nupkg.FullName -FarHome .

	# updated?
	requires -Path FarNet\Lib\FarNet.ScottPlot\ScottPlot.dll

	# Source preserved?
	$Source, $null = Get-Content Update.FarNet.ScottPlot.info
	equals $Source NuGet
}
