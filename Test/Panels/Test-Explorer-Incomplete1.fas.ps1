<#
.Synopsis
	JobResult.Incomplete, delete, copy, move
#>

job {
	. $PSScriptRoot\_Explorer.ps1
}

### Incomplete delete

job {
	# panel with 10 files
	$Explorer = New-TestExplorerIncomplete
	0..9 | %{ $Explorer.Cache.Add((New-FarFile $_)) }
	$Explorer.CreatePanel().Open()
}
job {
	# select 5 files
	Assert-Far -ExplorerTypeId c33db20e-7477-4301-ba5d-4c6b3b81f66b
	$__.SelectNames((1, 3, 5, 7, 9))
}
keys Del
job {
	# check
	Assert-Far @(
		$__.Files.Count -eq 7
		$__.SelectedFiles.Count -eq 2
		$__.SelectedFiles[0].Name -eq '3'
		$__.SelectedFiles[1].Name -eq '7'
	)
}

### Incomplete copy
keys Tab
job {
	# another panel with 4 files
	$Explorer = New-TestExplorerIncomplete
	11..16 | %{ $Explorer.Cache.Add((New-FarFile $_)) }
	$Explorer.CreatePanel().Open()
}
job {
	# select all
	$__.SelectAll()
	Assert-Far -Plugin
	Assert-Far @(
		$__.Explorer.TypeId -eq 'c33db20e-7477-4301-ba5d-4c6b3b81f66b'
		$__.SelectedFiles.Count -eq 6
	)
}
keys F5
job {
	# this panel (2nd)
	$selected = $__.SelectedFiles
	Assert-Far @(
		$__.GetFiles().Count -eq 6
		$selected.Count -eq 3
		$selected[0].Name -eq '12'
		$selected[1].Name -eq '14'
		$selected[2].Name -eq '16'
	)

	# that panel (1st)
	$selected = $Far.Panel2.SelectedFiles
	$shown = $Far.Panel2.Files
	Assert-Far @(
		# selection is not changed
		$selected.Count -eq 2
		$selected[0].Name -eq '3'
		$selected[1].Name -eq '7'
		# new files are added
		$shown.Count -eq 10
	)
}

### Incomplete move = incomplete copy + incomplete delete
# 12, 14, 16 are moved
# 14 is not accepted -> to stay
# 12, 16 are deleted
# 16 is not deleted -> to stay
# so: 12 moved, 14 failed, 16 copied
keys F6
job {
	# this panel (2nd)
	$selected = $__.SelectedFiles
	Assert-Far @(
		$__.GetFiles().Count -eq 5
		$selected.Count -eq 2
		$selected[0].Name -eq '14'
		$selected[1].Name -eq '16'
	)

	# that panel (1st)
	$selected = $Far.Panel2.SelectedFiles
	$shown = $Far.Panel2.Files
	Assert-Far @(
		# selection is not changed
		$selected.Count -eq 2
		$selected[0].Name -eq '3'
		$selected[1].Name -eq '7'
		# +2 files (12 and 16)
		$shown.Count -eq 12
	)
}

# exit 2nd, go back to 1st
keys Esc
job {
	Assert-Far -Native
}
keys Tab
# exit
keys Esc
