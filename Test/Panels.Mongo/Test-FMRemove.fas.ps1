<#
.Synopsis
	Test Open-MongoPanel removing operations.
#>

if ([System.Environment]::OSVersion.Version.Major -lt 10) {return}

job {
	Assert-Far -Title Ensure -NoError

	$null = Start-Mongo
	Import-Module FarMongo

	Import-Module Mdbc
	Connect-Mdbc .
	Remove-MdbcDatabase z
	$Database = Get-MdbcDatabase z

	# add two collections, or after removing x database z is removed, too
	$Collection = Get-MdbcCollection a
	@{_id = 3} | Add-MdbcData
	$Data.x = $Collection = Get-MdbcCollection x
	@{_id = 1}, @{_id = 2} | Add-MdbcData

	# open databases
	Open-MongoPanel
}
job {
	# go to database z
	Find-FarFile z
}
macro 'Keys"Del Enter" -- try delete z'
job {
	Assert-Far -Dialog
	Assert-Far $__[1].Text -eq "Database 'z' is not empty, 2 collections."
	Assert-Far $global:Error
	$global:Error.Clear()
}
keys Esc
macro 'Keys"Enter" -- open z collections'
job {
	# go to collection x
	Find-FarFile x
}
macro 'Keys"Enter" -- open x documents'
job {
	# go to file 2
	Find-FarFile 2
}
macro 'Keys"Del Enter" -- try delete 2'
job {
	Assert-Far -FileName 1

	Import-Module Mdbc
	Assert-Far 1L -eq (Get-MdbcData -Count -Collection $Data.x)
}
macro 'Keys"CtrlPgUp" -- back to collections'
job {
	Assert-Far -FileName x
}
macro 'Keys"Del Enter" -- try delete not empty collection'
job {
	Assert-Far -Dialog
	Assert-Far $__[1].Text -eq "Collection 'x' is not empty."
	Assert-Far $global:Error
	$global:Error.Clear()
}
keys Esc
macro 'Keys"ShiftDel Enter" -- delete not empty collection'
job {
	Assert-Far -Panels -FileName a
}
macro 'Keys"CtrlPgUp" -- back to databases'
job {
	Assert-Far -FileName z
}
macro 'Keys"ShiftDel Enter" -- delete not empty database'
job {
	Assert-Far -Panels
	Assert-Far ($__.CurrentFile.Name -ne 'z')
}
keys Esc
