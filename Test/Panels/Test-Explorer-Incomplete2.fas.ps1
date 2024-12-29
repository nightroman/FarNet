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
	$Far.Panel.CurrentDirectory = $env:FARHOME
	$Far.Panel.SelectNames(@('FarNet', 'Plugins', 'FarEng.hlf', 'FarRus.hlf'))
}

# copy files to the empty panel
macro 'Keys"F5 Enter"'
job {
	# files "to stay" are selected
	Assert-Far 'Plugins FarRus.hlf' -eq ($Far.Panel.SelectedFiles -join ' ')
	$Far.Panel.UnselectAll()
}

# back to 1st
keys Tab
job {
	Assert-Far 'FarNet FarEng.hlf' -eq ($Far.Panel.Files -join ' ')
}

# exit
keys Esc
