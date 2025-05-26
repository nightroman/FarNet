#! $HOME should have 2+ packages "Far*.7z".

Set-StrictMode -Version 3
. Update-FarManager.ps1

task pattern {
	($r = archive_pattern -Platform x64 -PackageType 7z)
	equals $r '^Far\.x64\.(\d+\.\d+\.\d+\.\d+)\.\w+\.7z$'
}

task archives {
	'get_archives'
	$r1 = @(get_archives -Platform x64 -PackageType 7z)
	$r1 | Out-String
	assert ($r1.Count -ge 2)

	'get_old_versions'
	$MaxVersions = 1
	$r2 = @(get_old_versions -Platform x64 -PackageType 7z -MaxVersions $MaxVersions)
	$r2 | Out-String
	equals $r2.Count ($r1.Count - $MaxVersions)
}
