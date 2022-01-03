
job {
	$Psf.Settings.MaximumPanelFileCount = 1000
}

### Abort

run {
	1..3000 | Out-FarPanel
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq "There are 3000 panel files, the limit is 1000."
}

macro 'Keys"Enter"'# [Abort]

job {
	Assert-Far -Panels
	$1 = $Far.Panel.ShownList
	Assert-Far @(
		$1.Count -eq 1001
		$1[0].Name -eq '..'
	)
}

macro 'Keys"Esc"'# exit panel
job {
	Assert-Far -Native
}

### Retry & Ignore

run {
	1..3000 | Out-FarPanel
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq "There are 3000 panel files, the limit is 1000."
}

macro 'Keys"Right Enter"'# [Retry]
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq "There are 3000 panel files, the limit is 2000."
}

macro 'Keys"Right Right Enter"'# [Ignore]
job {
	Assert-Far -Panels
	$1 = $Far.Panel.ShownList
	Assert-Far @(
		$1.Count -eq 3001
		$1[0].Name -eq '..'
	)
}

### Enumerable, Abort

keys Tab
run {
	Assert-Far -Native
	$Data.Panel = New-Object PowerShellFar.ObjectPanel
	$Data.Panel.AddObjects($Far.Panel2.ShownItems)
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq "There are more than 1000 panel files."
}

macro 'Keys"Enter"'# Abort
job {
	# panel is not yet shown!
	Assert-Far -Panels -Native
}
job {
	$Data.Panel.Open()
}
job {
	Assert-Far -Panels -Plugin -Plugin2
	$1 = $Far.Panel.ShownList
	Assert-Far @(
		$1.Count -eq 1001
		$1[0].Name -eq '..'
	)
}

keys Esc
job {
	Assert-Far -Panels -Native -Plugin2
}

### Enumerable, Retry, Ignore

run {
	$Data.Panel = New-Object PowerShellFar.ObjectPanel
	$Data.Panel.AddObjects($Far.Panel2.ShownItems)
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq "There are more than 1000 panel files."
}

macro 'Keys"Right Enter"'# Retry
run {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq "There are more than 2000 panel files."
}

macro 'Keys"Right Right Enter"'# Ignore
job {
	# panel is not yet shown!
	Assert-Far -Panels -Native
}
job {
	$Data.Panel.Open()
}
job {
	Assert-Far -Panels -Plugin -Plugin2
	$1 = $Far.Panel.ShownList
	Assert-Far @(
		$1.Count -eq 3001
		$1[0].Name -eq '..'
	)
}

### end

macro 'Keys"Esc"'# exit panel 2
job {
	Assert-Far -Panels -Native -Plugin2
}
keys Tab
job {
	Assert-Far -Panels -Plugin -Native2
}
keys Esc
