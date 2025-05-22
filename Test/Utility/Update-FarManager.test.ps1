# manual read only tests

Set-StrictMode -Version 3
. Update-FarManager.ps1

task archive_pattern {
	archive_pattern -Platform x64 -PackageType 7z
}

task get_archives {
	get_archives -Platform x64 -PackageType 7z
}

task get_old_versions {
	get_old_versions -Platform x64 -PackageType 7z -MaxVersions 1
}
