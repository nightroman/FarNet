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
	Assert-Far $__.GetFiles().Count -eq 0
}

### new and cancel
macro 'Keys"F7" -- new document'
job {
	Assert-Far -Editor
}
macro 'Keys"Esc" -- cancel new document'
job {
	Assert-Far -Panels
	Assert-Far $__.GetFiles().Count -eq 0
}

### new and save
macro 'Keys"F7" -- new document'
job {
	Assert-Far -Editor
	$__.SetText('{_id: "id1", x: 1}')
}
macro 'Keys"Esc Enter" -- exit, save'
job {
	Import-Module Mdbc
	$r = Get-MdbcData -Collection $Data.Collection
	Assert-Far -Panels
	Assert-Far @(
		$__.CurrentFile.Name -ceq 'id1'
		$__.CurrentFile.Description -ceq '1'
		"$r" -ceq '{ "_id" : "id1", "x" : 1 }'
	)
}

### edit
macro 'Keys"F4" -- edit document'
job {
	Assert-Far -Editor
	Assert-Far @(
		$__[1].Text -ceq '  "_id" : "id1",'
		$__[2].Text -ceq '  "x" : 1'
	)
	$__[2].Text = 'x: 2'
}
macro 'Keys"Esc Enter" -- exit, save'
job {
	Import-Module Mdbc
	$r = Get-MdbcData -Collection $Data.Collection
	Assert-Far -Panels
	Assert-Far @(
		$__.CurrentFile.Name -ceq 'id1'
		$__.CurrentFile.Description -ceq '2'
		"$r" -ceq '{ "_id" : "id1", "x" : 2 }'
	)
}

### new with no _id
macro 'Keys"F7" -- new document'
job {
	Assert-Far -Editor
	$__.SetText('{x: 25}')
}
macro 'Keys"Esc Enter" -- exit, save'
job {
	Assert-Far -Panels
	Assert-Far $(
		$__.GetFiles().Count -eq 2

		$data = $__.CurrentFile.Data
		$data._id.GetType() -eq ([MongoDB.Bson.ObjectId])
		$data.x -eq 25
	)
}
macro 'Keys"Esc" -- exit panel'
