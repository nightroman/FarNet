<#
.Synopsis
	JobResult.Incomplete, import
#>

job {
	. $PSScriptRoot\_Explorer.ps1

	# open empty panel
	$Explorer = New-TestExplorerIncomplete
	$Explorer.CreatePanel().Open()
}

# go to FARHOME, select files
keys Tab
job {
	$__.CurrentDirectory = $env:FARHOME
	$__.SelectNames(@('FarNet', 'Plugins', 'FarEng.hlf', 'FarRus.hlf'))
}

# copy files to the empty panel
macro 'Keys"F5 Enter"'
job {
	# files "to stay" are selected
	Assert-Far 'Plugins FarRus.hlf' -eq ($__.SelectedFiles -join ' ')
	$__.UnselectAll()
}

# back to 1st
keys Tab
job {
	Assert-Far 'FarNet FarEng.hlf' -eq ($__.Files -join ' ')
}

# exit
keys Esc
