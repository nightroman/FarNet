<#
.Synopsis
	Test Select* and Push/Pop for FarNet panels.
#>

$Unshelve = { job {
	# unshelve the last shelved
	$top = @([FarNet.Works.ShelveInfo]::Stack)[-1]
	$null = [FarNet.Works.ShelveInfo]::Stack.Remove($top)
	$top.Pop()
}}

job {
	# open the test panel
	& "$env:FarNetCode\Samples\Tests\Test-Panel.far.ps1"
}

# insert 4 items
macro 'Keys"F7 F7 F7 F7"'
job {
	# Item4
	Assert-Far -FileName Item4

	# 5 items with dots, nothing is selected
	Assert-Far $__.Files.Count -eq 5
	Assert-Far $__.SelectedFiles.Count -eq 1
}

keys Home
job {
	Assert-Far $__.SelectedFiles.Count -eq 0
}

### SelectNames\UnselectNames
job {
	# select 3 names
	$__.SelectNames(@('Item4', 'Item1', 'Item2'))
}
job {
	$files = $__.GetSelectedFiles()
	Assert-Far $files.Count -eq 3
	Assert-Far @(
		$files[0].Name -eq 'Item1'
		$files[1].Name -eq 'Item2'
		$files[2].Name -eq 'Item4'
	)
}
job {
	# unselect 2 names
	$__.UnselectNames(@('Item4', 'Item1'))
}
job {
	$files = $__.GetSelectedFiles()
	Assert-Far $files.Count -eq 1
	Assert-Far $files[0].Name -eq 'Item2'
}
job {
	# unselect 1 name
	$__.UnselectNames(@('Item2'))
}
job {
	Assert-Far $__.GetSelectedFiles().Count -eq 0
}

### SelectAt
job {
	# select 1 item
	$__.SelectAt(@(2))
}

$Test = { job {
	# no current, 1 selected
	$1 = $__
	Assert-Far @(
		!$1.CurrentFile
		($1.SelectedFiles.Count -eq 1) -and ($1.SelectedFiles[0].Name -eq "Item2")
	)
}}
& $Test

job {
	# push
	$__.Push()
}
job {
	Assert-Far -Native
}

& $Unshelve

& $Test

job {
	# go to item 2, select 2 items
	Find-FarFile Item2
	$__.SelectAt(@(4, 2))
}

$Test = { job {
	$1 = $__
	Assert-Far $1.CurrentFile.Name -eq 'Item2'
	Assert-Far $1.SelectedFiles.Count -eq 2
	Assert-Far $1.SelectedFiles[0].Name -eq "Item2"
	Assert-Far $1.SelectedFiles[1].Name -eq "Item4"
}}
& $Test

job {
	# push
	$__.Push()
}
job {
	Assert-Far -Native
}

& $Unshelve

& $Test

job {
	Find-FarFile '..'
}

job {
	# select all items
	$__.SelectAll()
	Assert-Far $__.SelectedFiles.Count -eq 4
}

job {
	# unselect 1 item
	$__.UnselectAt(@(1))
	Assert-Far $__.SelectedFiles.Count -eq 3
	Assert-Far $__.SelectedFiles[0].Name -eq "Item2"
}

job {
	# unselect 2 items
	$__.UnselectAt(@(2, 1))
	Assert-Far $__.SelectedFiles.Count -eq 2
	Assert-Far $__.SelectedFiles[0].Name -eq "Item3"
}

job {
	# unselect all items
	$__.UnselectAll()
	Assert-Far $__.SelectedFiles.Count -eq 0
}

# exit
macro 'Keys"Esc 1"'
