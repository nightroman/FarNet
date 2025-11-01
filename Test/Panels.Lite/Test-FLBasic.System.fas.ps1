<#
.Synopsis
	Tests Open-LitePanel with system collections.

.Description
	Requires "C:\TEMP\Files.LiteDB".
	Use "Update-LiteFiles.ps1" in order to make it.
#>

job {
	Import-Module FarLite

	# open database
	Open-LitePanel C:\TEMP\Files.LiteDB -System
}
job {
	# go to `$indexes` then enter
	Assert-Far $__.Title -eq 'Collections'
	Find-FarFile '$cols'
}
keys Enter
job {
	Assert-Far $__.Title -eq '$cols'
	Find-FarFile Files
}
job {
	Assert-Far -FileDescription user
}
keys Esc
job {
	Assert-Far -FileName '$cols'
}
keys Esc
