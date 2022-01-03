<#
.Synopsis
	Tests Open-LitePanel.

.Description
	Requires "C:\TEMP\Files.LiteDB".
	Use "Update-LiteFiles.ps1" in order to make it.
#>

job {
	Import-Module FarLite

	# open database
	Open-LitePanel C:\TEMP\Files.LiteDB
}
job {
	# go to `files` then enter
	Assert-Far $Far.Panel.Title -eq 'Collections'
	Find-FarFile Files
}
keys Enter
job {
	Assert-Far $Far.Panel.Title -eq 'Files'
}
# go to end and keep the last file name
keys End
job {
	$data.LastId = $Far.Panel.CurrentFile.Data._id
}
# next data page
keys PgDn
job {
	Assert-Far $(
		$data.LastId -ne $Far.Panel.CurrentFile.Data._id
		$Far.Panel.CurrentIndex -eq $Far.Panel.ShownList.Count - 1
	)
}
# prev data page, go to end
macro 'Keys"Home PgUp End"'
job {
	Assert-Far $data.LastId -eq $Far.Panel.CurrentFile.Data._id
}
# exit all panels
keys ShiftEsc
