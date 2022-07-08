<#
.Synopsis
	Test Open-MongoPanel aggregate.
#>

if ([System.Environment]::OSVersion.Version.Major -lt 10) {return}

job {
	$null = Start-Mongo

	Import-Module Mdbc
	Connect-Mdbc -NewCollection
	$Data.Collection = $Collection
	[ordered]@{_id = 1; x = 1; y = 1}, [ordered]@{_id = 2; x = 2; y = 2} | Add-MdbcData

	# new view, include just x
	Remove-MdbcCollection test_view
	$null = Invoke-MdbcCommand @'
{
	create: "test_view",
	viewOn: "test",
	pipeline: [{$project: {x: 1}}]
}
'@

	# open view
	Import-Module FarMongo
	Open-MongoPanel . test test_view
}
job {
	Assert-Far -Panels
	Assert-Far $Far.Panel.Title -eq 'test_view (test)'
}
# common steps
. $PSScriptRoot\FMView.ps1
job {
	Import-Module Mdbc
	Remove-MdbcCollection test_view -Database $Data.Collection.Database
}
