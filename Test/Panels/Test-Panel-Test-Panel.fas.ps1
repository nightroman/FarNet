<#
.Synopsis
	Test bare FarNet panels
#>

function Test-F1 {
	keys F1
	job {
		Assert-Far -Dialog
		Assert-Far @(
			$__[0].Text -eq 'KeyPressed'
			$__[1].Text -eq '[F1] has been pressed'
		)
	}
}

function Test-Copy {
	keys F5
}

function Test-Move {
	keys F6
}

### open panel 1
job {
	& "$env:FarNetCode\Samples\Tests\Test-Panel.far.ps1"
}
job {
	Assert-Far $__.Title -eq "Test Panel"
}

### KeyPressed, handle it
Test-F1
keys h
job {
	Assert-Far -Dialog
	Assert-Far @(
		$__[0].Text -eq 'KeyPressed'
		$__[1].Text -eq '[F1] has been handled'
	)
}
keys Esc
job {
	Assert-Far -Panels
}

### KeyPressed, let it go: it opens Help, [Esc] it
Test-F1
macro 'Keys"d Esc"'
job {
	Assert-Far -Panels
}

# covers: F8 when there are no files
job {
	# no files, zero counter
	Assert-Far $__.GetFiles().Count -eq 0
	$__.Explorer.Data.DeleteFiles = 0
}
keys F8
job {
	# check counter: event has not happend
	Assert-Far $__.Explorer.Data.DeleteFiles -eq 0
}

# make new file
keys F7
job {
	# new item exists and it is current
	Assert-Far -FileName 'Item1'
	Assert-Far @(
		$__.GetFiles().Count -eq 1
		$__.Explorer.Data.CreateFile -eq 1
	)
	$__.Explorer.Data.CreateFile = 0
}

# push
# NB: cannot use F11
# Update: now we can
job {
	$Data.Panel = $__
	$Data.Panel.Push()
}
job {
	Assert-Far -Native -FileName '..'
}

# pop
# NB: cannot use F11
# Update: now we can
job {
	$Data.Panel.Open()
}
job {
	Assert-Far -Plugin -FileName 'Item1'
}

# quick view, info
# ??? to test
keys CtrlQ
keys CtrlL
keys CtrlL
macro 'Keys"CtrlL"'
macro 'Keys"CtrlL"'
job {
	Assert-Far $__.Explorer.Data.GetContent -eq 1
	$__.Explorer.Data.GetContent = 1
}

# delete it
keys F8
job {
	Assert-Far @(
		$__.GetFiles().Count -eq 0
		$__.Explorer.Data.DeleteFiles -eq 1
	)
	$__.Explorer.Data.DeleteFiles = 0
}

# make 2 files
macro 'Keys"F7 F7"'
job {
	Assert-Far @(
		$__.GetFiles().Count -eq 2
		$__.Explorer.Data.CreateFile -eq 2
	)
	$__.Explorer.Data.CreateFile = 0
}

# delete all
macro 'Keys"Multiply F8"'
job {
	Assert-Far @(
		$__.GetFiles().Count -eq 0
		$__.Explorer.Data.DeleteFiles -eq 1
	)
	$__.Explorer.Data.DeleteFiles = 0
}

### open panel 2
keys Tab
job {
	& "$env:FarNetCode\Samples\Tests\Test-Panel.far.ps1" -NoDots
}
job {
	Assert-Far $__.Title -eq "Test Panel"
}

# F8 when panel is empty
keys F8
job {
	# event has not happend
	Assert-Far $__.Explorer.Data.DeleteFiles -eq 0
}

# make new file
keys F7
job {
	Assert-Far @(
		$__.GetFiles().Count -eq 1
		$__.Explorer.Data.CreateFile -eq 1
	)
	$__.Explorer.Data.CreateFile = 0
}

# delete it
keys F8
job {
	Assert-Far @(
		$__.GetFiles().Count -eq 0
		$__.Explorer.Data.DeleteFiles -eq 1
	)
	$__.Explorer.Data.DeleteFiles = 0
}

# make 2 files
macro 'Keys"F7 F7"'
job {
	Assert-Far @(
		$__.GetFiles().Count -eq 2
		$__.Explorer.Data.CreateFile -eq 2
	)
	$__.Explorer.Data.CreateFile = 0
}

# delete all
macro 'Keys"Multiply F8"'
job {
	Assert-Far @(
		$__.GetFiles().Count -eq 0
		$__.Explorer.Data.DeleteFiles -eq 1
	)
	$__.Explorer.Data.DeleteFiles = 0
}

### make and copy 1 file
keys F7
Test-Copy
job {
	Assert-Far @(
		$__.GetFiles().Count -eq 1
		$__.Explorer.Data.CreateFile -eq 1
		$Far.Panel2.GetFiles().Count -eq 1
		$Far.Panel2.Explorer.Data.AcceptFiles -eq 1
	)
	$__.Explorer.Data.CreateFile = 0
	$Far.Panel2.Explorer.Data.AcceptFiles = 0
}

### move 1 file
Test-Move
job {
	# delete at source and import at target
	Assert-Far @(
		$__.GetFiles().Count -eq 0
		$__.Explorer.Data.DeleteFiles -eq 1
		$Far.Panel2.GetFiles().Count -eq 2
		$Far.Panel2.Explorer.Data.AcceptFiles -eq 1
	)
	$__.Explorer.Data.DeleteFiles = 0
	$Far.Panel2.Explorer.Data.AcceptFiles = 0
}

### copy many files
macro 'Keys"F7 F7 Multiply"'
job {
	Assert-Far @(
		$__.GetSelectedFiles().Count -eq 2
		$__.Explorer.Data.CreateFile -eq 2
	)
	$__.Explorer.Data.CreateFile = 0
}
Test-Copy
job {
	Assert-Far @(
		$__.GetSelectedFiles().Count -eq 1
		$Far.Panel2.GetFiles().Count -eq 4
		$Far.Panel2.Explorer.Data.AcceptFiles -eq 1
	)
	$Far.Panel2.Explorer.Data.AcceptFiles = 0
}

### select and move 2 files
keys Multiply
Test-Move
job {
	Assert-Far @(
		$__.GetFiles().Count -eq 0
		$__.Explorer.Data.DeleteFiles -eq 1
		$Far.Panel2.GetFiles().Count -eq 6
		$Far.Panel2.Explorer.Data.AcceptFiles -eq 1
	)
	$__.Explorer.Data.DeleteFiles = 0
	$Far.Panel2.Explorer.Data.AcceptFiles = 0
}

# close
keys Esc
job {
	Assert-Far -Dialog
}
keys 1
job {
	Assert-Far -Native
}

#
macro 'Keys"Tab Multiply"'
job {
	Assert-Far @(
		$__.GetFiles().Count -eq 6
		$__.GetSelectedFiles().Count -eq 6
	)
}
keys F8
job {
	Assert-Far @(
		$__.GetFiles().Count -eq 0
		$__.Explorer.Data.DeleteFiles = 1
	)
	$__.Explorer.Data.DeleteFiles = 0
}

### AcceptFiles from a native panel
keys Tab
job {
	$__.CurrentDirectory = $env:FARHOME
	$__.SelectNames(@('Far.exe', 'Far.exe.ini'))
}
keys F5
job {
	# Copy dialog
	Assert-Far -DialogTypeId fcef11c4-5490-451d-8b4a-62fa03f52759
}
keys Enter
keys Enter
macro 'Keys"Tab"'
job {
	$files = $__.GetFiles()
	Assert-Far @(
		$files.Count -eq 2
		$files[0].Name -eq 'Far.exe'
		$files[1].Name -eq 'Far.exe.ini'
	)
}

# close
keys Esc
job {
	Assert-Far -Dialog
}
keys 1
