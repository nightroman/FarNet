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
	& "$env:PSF\Samples\Tests\Test-Panel-.ps1"
}

# insert 4 items
macro 'Keys"F7 F7 F7 F7"'
job {
	# Item4
	Assert-Far (Get-FarFile).Name -eq 'Item4'

	# 5 items with dots, nothing is selected
	Assert-Far $Far.Panel.Files.Count -eq 5
	Assert-Far $Far.Panel.SelectedFiles.Count -eq 1
}

keys Home
job {
	Assert-Far $Far.Panel.SelectedFiles.Count -eq 0
}

### SelectNames\UnselectNames
job {
	# select 3 names
	$Far.Panel.SelectNames(@('Item4', 'Item1', 'Item2'))
}
job {
	$files = $Far.Panel.GetSelectedFiles()
	Assert-Far $files.Count -eq 3
	Assert-Far @(
		$files[0].Name -eq 'Item1'
		$files[1].Name -eq 'Item2'
		$files[2].Name -eq 'Item4'
	)
}
job {
	# unselect 2 names
	$Far.Panel.UnselectNames(@('Item4', 'Item1'))
}
job {
	$files = $Far.Panel.GetSelectedFiles()
	Assert-Far $files.Count -eq 1
	Assert-Far $files[0].Name -eq 'Item2'
}
job {
	# unselect 1 name
	$Far.Panel.UnselectNames(@('Item2'))
}
job {
	Assert-Far $Far.Panel.GetSelectedFiles().Count -eq 0
}

### SelectAt
job {
	# select 1 item
	$Far.Panel.SelectAt(@(2))
}

$Test = { job {
	# no current, 1 selected
	$1 = $Far.Panel
	Assert-Far @(
		!$1.CurrentFile
		($1.SelectedFiles.Count -eq 1) -and ($1.SelectedFiles[0].Name -eq "Item2")
	)
}}
& $Test

job {
	# push
	$Far.Panel.Push()
}
job {
	Assert-Far -Native
}

& $Unshelve

& $Test

job {
	# go to item 2, select 2 items
	Find-FarFile Item2
	$Far.Panel.SelectAt(@(4, 2))
}

$Test = { job {
	$1 = $Far.Panel
	Assert-Far $1.CurrentFile.Name -eq 'Item2'
	Assert-Far $1.SelectedFiles.Count -eq 2
	Assert-Far $1.SelectedFiles[0].Name -eq "Item2"
	Assert-Far $1.SelectedFiles[1].Name -eq "Item4"
}}
& $Test

job {
	# push
	$Far.Panel.Push()
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
	$Far.Panel.SelectAll()
	Assert-Far $Far.Panel.SelectedFiles.Count -eq 4
}

job {
	# unselect 1 item
	$Far.Panel.UnselectAt(@(1))
	Assert-Far $Far.Panel.SelectedFiles.Count -eq 3
	Assert-Far $Far.Panel.SelectedFiles[0].Name -eq "Item2"
}

job {
	# unselect 2 items
	$Far.Panel.UnselectAt(@(2, 1))
	Assert-Far $Far.Panel.SelectedFiles.Count -eq 2
	Assert-Far $Far.Panel.SelectedFiles[0].Name -eq "Item3"
}

job {
	# unselect all items
	$Far.Panel.UnselectAll()
	Assert-Far $Far.Panel.SelectedFiles.Count -eq 0
}

# exit
macro 'Keys"Esc 1"'
