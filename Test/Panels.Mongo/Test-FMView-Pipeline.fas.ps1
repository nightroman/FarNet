<#
.Synopsis
	Test Open-MongoPanel aggregate.
#>

if ([System.Environment]::OSVersion.Version.Major -lt 10) {return}

job {
	$null = Start-Mongo
	Import-Module FarMongo

	Import-Module Mdbc
	Connect-Mdbc -NewCollection
	$Data.Collection = $Collection
	[ordered]@{_id = 1; x = 1; y = 1}, [ordered]@{_id = 2; x = 2; y = 2} | Add-MdbcData

	# open collection with pipeline, include just x, exclude y
	Open-MongoPanel . test test -Pipeline '[{$project: {x: 1}}]'
}
job {
	Assert-Far -Panels
	Assert-Far $Far.Panel.Title -eq 'test (aggregate)'
}
# common steps
. $PSScriptRoot\FMView.ps1
