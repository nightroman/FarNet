# Common for collection aggregate and view panels

job {
	Find-FarFile 2
}
macro 'Keys"Enter" -- file 2 properties'
job {
	Assert-Far -Panels
	Assert-Far $(
		$files = $Far.Panel.GetFiles()
		$files.Count -eq 2
		$files[0].Name -eq '_id'
		$files[1].Name -eq 'x'
	)
}
macro 'Keys"Esc" -- back to files'
macro 'Keys"F4" -- edit 2'
job {
	Assert-Far -Editor
	Assert-Far $(
		$Far.Editor[1].Text -eq '  "_id" : 2,'
		$Far.Editor[2].Text -eq '  "x" : 2,'
		$Far.Editor[3].Text -eq '  "y" : 2'
	)
	# change y = 3
	$Far.Editor[3].Text = '  "y" : 3'
}
macro 'Keys"F2 Esc" -- save and exit'
job {
	# y changed in the collection
	Import-Module Mdbc
	$r = Get-MdbcData @{_id = 2} -Collection $Data.Collection
	Assert-Far $r.y -eq 3
}
macro 'Keys"Del" -- delete'
job {
	Assert-Far -Dialog
	Assert-Far @(
		$Far.Dialog[0].Text -eq 'Delete'
 		$Far.Dialog[1].Text -eq '1 documents(s)'
 	)
}
macro 'Keys"Enter" -- confirm delete'
job {
	Assert-Far -Panels

	Import-Module Mdbc
	Assert-Far @(
		$Far.Panel.CurrentFile.Name -eq 1
		$r = Get-MdbcData -Collection $Data.Collection
		1 -eq $r._id
	)
}
keys Esc
