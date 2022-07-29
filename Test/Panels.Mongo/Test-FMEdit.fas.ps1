<#
.Synopsis
	Test F7 F4 in documents panel.
#>

if ([System.Environment]::OSVersion.Version.Major -lt 10) {return}

job {
	$null = Start-Mongo
	Import-Module FarMongo

	Import-Module Mdbc
	Connect-Mdbc -NewCollection
	$Data.Collection = $Collection

	# open collection documents
	Open-MongoPanel . test test
}
job {
	Assert-Far -Panels
	Assert-Far $Far.Panel.GetFiles().Count -eq 0
}

### new and cancel
macro 'Keys"F7" -- new document'
job {
	Assert-Far -Editor
}
macro 'Keys"Esc" -- cancel new document'
job {
	Assert-Far -Panels
	Assert-Far $Far.Panel.GetFiles().Count -eq 0
}

### new and save
macro 'Keys"F7" -- new document'
job {
	Assert-Far -Editor
	$Far.Editor.SetText('{_id: "id1", x: 1}')
}
macro 'Keys"Esc Enter" -- exit, save'
job {
	Import-Module Mdbc
	$r = Get-MdbcData -Collection $Data.Collection
	Assert-Far -Panels
	Assert-Far @(
		$Far.Panel.CurrentFile.Name -ceq 'id1'
		$Far.Panel.CurrentFile.Description -ceq '1'
		"$r" -ceq '{ "_id" : "id1", "x" : 1 }'
	)
}

### edit
macro 'Keys"F4" -- edit document'
job {
	Assert-Far -Editor
	Assert-Far @(
		$Far.Editor[1].Text -ceq '  "_id" : "id1",'
		$Far.Editor[2].Text -ceq '  "x" : 1'
	)
	$Far.Editor[2].Text = 'x: 2'
}
macro 'Keys"Esc Enter" -- exit, save'
job {
	Import-Module Mdbc
	$r = Get-MdbcData -Collection $Data.Collection
	Assert-Far -Panels
	Assert-Far @(
		$Far.Panel.CurrentFile.Name -ceq 'id1'
		$Far.Panel.CurrentFile.Description -ceq '2'
		"$r" -ceq '{ "_id" : "id1", "x" : 2 }'
	)
}

### new with no _id
macro 'Keys"F7" -- new document'
job {
	Assert-Far -Editor
	$Far.Editor.SetText('{x: 25}')
}
macro 'Keys"Esc Enter" -- exit, save'
job {
	Assert-Far -Panels
	Assert-Far $(
		$Far.Panel.GetFiles().Count -eq 2

		$data = $Far.Panel.CurrentFile.Data
		$data._id.GetType() -eq ([MongoDB.Bson.ObjectId])
		$data.x -eq 25
	)
}
macro 'Keys"Esc" -- exit panel'
