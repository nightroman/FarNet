<#
.Synopsis
	Tests Open-MongoPanel, requires `test.files`.
#>

if ([System.Environment]::OSVersion.Version.Major -lt 10) {return}

job {
	$null = Start-Mongo
	Import-Module FarMongo

	# open databases
	Open-MongoPanel
}
job {
	# go to `test` then enter
	Assert-Far $Far.Panel.Title -eq 'Databases'
	Find-FarFile test
}
keys Enter
job {
	# go to `files` then enter
	Assert-Far $Far.Panel.Title -eq 'Collections'
	Find-FarFile files
}
keys Enter
job {
	Assert-Far $Far.Panel.Title -eq 'files'
}
# go to end and keep the last file name
keys End
job {
	$data.LastName = $Far.Panel.CurrentFile.Name
}
# next data page
keys PgDn
job {
	Assert-Far $Far.Panel.CurrentIndex -eq ($Far.Panel.Files.Count - 1)
}
# prev data page, go to end
macro 'Keys"Home PgUp End"'
job {
	Assert-Far $data.LastName -eq $Far.Panel.CurrentFile.Name
}
# exit all panels
keys ShiftEsc
