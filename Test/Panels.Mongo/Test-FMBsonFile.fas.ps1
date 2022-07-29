<#
.Synopsis
	Test Open-MongoPanel -BsonFile.
#>

if ([System.Environment]::OSVersion.Version.Major -lt 10) {return}

### just open and exit
job {
	$null = Start-Mongo
	Import-Module FarMongo

	# this data
	$Data.File = 'c:\temp\bson.db.json'

	# new json file
	Set-Content $Data.File @'
{_id: 1, name: "name1"}
{_id: 2}
'@

	Open-MongoPanel -BsonFile $Data.File
}
job {
	Assert-Far -Plugin
	Assert-Far $(
		$Far.Panel.CurrentIndex -eq 0
		$Far.Panel.Title -eq ([IO.Path]::GetFileName($Data.File))
	)
	$r = $Far.Panel.GetFiles()
	Assert-Far $(
		$r.Count -eq 2
		$r[0].Name -eq 'name1'
		$r[0].Description -eq '1'
		$r[1].Name -eq $null
		$r[1].Description -eq '2'
	)
}
macro 'Keys"Esc" -- exit no changes'
job {
	Assert-Far -Native
}

### open and create
job {
	Open-MongoPanel -BsonFile $Data.File
}
keys F7
job {
	Assert-Far -Editor
	$Far.Editor.SetText('{_id: 3}')
	$Far.Editor.Save()
	$Far.Editor.Close()
}
#! Far 3.0.5839 change in updating panels?
keys CtrlR
job {
	Assert-Far -Panels -FileDescription 3
}
macro 'Keys"Esc" -- try exit'
job {
	Assert-Far -Dialog
	Assert-Far @(
		$Far.Dialog[0].Text -eq 'Export'
		$Far.Dialog[1].Text -eq 'Export data to file?'
	)
}
macro 'Keys"Esc" -- do not exit'
job {
	Assert-Far -Panels -FileDescription 3
}
macro 'Keys"Esc Enter" -- repeat and exit'
job {
	Assert-Far -Native

	Import-Module Mdbc
	$r = Import-MdbcData $Data.File
	Assert-Far $(
		$r.Count -eq 3
		$r[2]._id -eq 3
	)
}

### open and delete
job {
	Open-MongoPanel -BsonFile $Data.File
}
job {
	Find-FarFile name1
}
macro 'Keys"Del Enter"'
job {
	#! Name because 1 column
	Assert-Far -Panels -FileName 2
}
macro 'Keys"Esc" -- try exit'
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'Export'
}
macro 'Keys"Enter" -- yes'
job {
	Assert-Far -Native

	Import-Module Mdbc
	$r = Import-MdbcData $Data.File
	Assert-Far $(
		$r.Count -eq 2
		$r[0]._id -eq 2
	)
}

### open and edit
job {
	Open-MongoPanel -BsonFile $Data.File
}
job {
	Find-FarFile 2
}
macro 'Keys"F4" -- edit'
job {
	Assert-Far -Editor
	$Far.Editor.SetText('{_id: 2, name: "name2"}')
}
macro 'Keys"Esc Enter" -- exit editor'
job {
	Assert-Far -Panels -FileName name2
}
macro 'Keys"Esc" -- try exit'
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'Export'
}
macro 'Keys"Enter" -- yes'
job {
	Assert-Far -Native

	Import-Module Mdbc
	$r = Import-MdbcData $Data.File
	Assert-Far $(
		$r.Count -eq 2
		$r[0]._id -eq 2
		$r[0].name -eq 'name2'
	)
}

### open, edit, save by menu
job {
	Open-MongoPanel -BsonFile $Data.File
}
job {
	Find-FarFile name2
}
macro 'Keys"F4" -- edit, change name'
job {
	Assert-Far -Editor
	$Far.Editor.SetText('{_id: 2, name: "name3"}')
}
macro 'Keys"Esc Enter" -- exit editor'
job {
	Assert-Far -Panels -FileName name3
}
macro 'Keys"F1 e" -- menu export to file'
job {
	# file saved
	Import-Module Mdbc
	$r = Import-MdbcData $Data.File
	Assert-Far $(
		$r.Count -eq 2
		$r[0]._id -eq 2
		$r[0].name -eq 'name3'
	)
}
macro 'Keys"Esc" -- exit, should not ask'
