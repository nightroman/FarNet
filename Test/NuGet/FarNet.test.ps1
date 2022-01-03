<#
.Synopsis
	Tests FarNet updates.

.Description
	** Order of some tests is important **
#>

Enter-Build {
	Import-Module FarPackage
	Set-StrictMode -Version Latest

	$FarHome = "C:\TEMP\FarHome"
	$Packages = @(
		'FarNet'
		'FarNet.PowerShellFar'
		'FarNet.RightWords'
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
		assert ($s -eq 'https://www.nuget.org/api/v2')
		assert ($v -eq $Version[$_])
		assert ($f.Count -ge 2)
	}

	# test particular items
	Set-Location $FarHome

	assert (Test-Path FarNet)
	assert (Test-Path Plugins)
	assert (Test-Path Far.exe.config)
	assert (Test-Path FarNet\Modules)
	assert (Test-Path FarNet\NHunspell)
}
#! fixed: "Cannot validate -Platform" (because '' is passed in nested calls)
task UpdateUpToDate {
	Update-FarPackage -FarHome $FarHome
}

task Reinstall.FarNet {
	Set-Location $FarHome

	Uninstall-FarPackage FarNet
	assert (Test-Path FarNet)
	assert (!(Test-Path Plugins))
	assert (!(Test-Path Far.exe.config))
	assert (!(Test-Path Update.FarNet.info))

	Install-FarPackage FarNet -Platform x64
	assert (Test-Path Plugins)
	assert (Test-Path Far.exe.config)
	assert (Test-Path Update.FarNet.info)
}

task Reinstall.PowerShellFar {
	Set-Location $FarHome

	Uninstall-FarPackage FarNet.PowerShellFar
	assert (Test-Path FarNet)
	assert (!(Test-Path FarNet\Modules\PowerShellFar))
	assert (!(Test-Path Update.FarNet.PowerShellFar.info))

	Install-FarPackage FarNet.PowerShellFar
	assert (Test-Path FarNet\Modules\PowerShellFar)
	assert (Test-Path Update.FarNet.PowerShellFar.info)
}

task Reinstall.RightWords {
	Set-Location $FarHome

	Uninstall-FarPackage FarNet.RightWords
	assert (Test-Path FarNet)
	assert (!(Test-Path FarNet\NHunspell))
	assert (!(Test-Path FarNet\Modules\RightWords))
	assert (!(Test-Path Update.FarNet.RightWords.info))

	Install-FarPackage FarNet.RightWords
	assert (Test-Path FarNet\NHunspell)
	assert (Test-Path FarNet\Modules\RightWords)
	assert (Test-Path Update.FarNet.RightWords.info)
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
		assert ($v -eq $Version[$_])
	}
}

task PreserveSource {
	Set-Location $FarHome

	# to test updated
	Remove-Item FarNet\Modules\RightWords\RightWords.dll

	# get any nupkg
	$nupkg = @(Get-Item "$env:LOCALAPPDATA\NuGet\Cache\FarNet.RightWords.*.nupkg")[0]

	# update from file
	Restore-FarPackage $nupkg.FullName -FarHome .

	# updated?
	assert (Test-Path FarNet\Modules\RightWords\RightWords.dll)

	# Source preserved?
	$Source, $null = Get-Content Update.FarNet.RightWords.info
	assert ($Source -eq 'https://www.nuget.org/api/v2')
}
