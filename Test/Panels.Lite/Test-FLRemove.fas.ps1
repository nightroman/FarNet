<#
.Synopsis
	Test Open-LitePanel removing operations.
#>

job {
	if ($global:Error) {throw 'Please remove errors.'}

	Import-Module FarLite
	$Data.FileName = "$env:TEMP\z.LiteDB"
	if (Test-Path $Data.FileName) {Remove-Item $Data.FileName}

	Import-Module Ldbc
	Use-LiteDatabase $Data.FileName {
		$test = Get-LiteCollection a
		@{_id = 3} | Add-LiteData $test
		$test = Get-LiteCollection x
		@{_id = 1}, @{_id = 2} | Add-LiteData $test
	}

	# open database
	Open-LitePanel $Data.FileName
}
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

	Import-Module Ldbc
	Use-LiteDatabase $Data.FileName {
		$test = Get-LiteCollection x
		Assert-Far 1 -eq $test.Count()
	}
}
macro 'Keys"CtrlPgUp" -- back to collections'
job {
	Assert-Far -FileName x
}
macro 'Keys"Del Enter" -- try delete not empty collection'
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq "Collection 'x' is not empty."
	Assert-Far $global:Error
	$global:Error.Clear()
}
keys Esc
macro 'Keys"ShiftDel Enter" -- delete not empty collection'
job {
	Assert-Far -Panels -FileName a
}

### F7 create/delete collection

macro 'Keys"F7 z Enter" -- new collection z'
job {
	Assert-Far -FileName z
}
macro 'Keys"Del Enter" -- delete collection z'
job {
	Assert-Far -FileName a
}
keys Esc
