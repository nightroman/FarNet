
job {
	# Not 2-3 Name column mode because Shift+Arrow works differently
	Assert-Far -Panels
	Assert-Far ((1 -ne $__.ViewMode) -and (2 -ne $__.ViewMode))
}

### common tests
function GetTestLine {
	job {
		Assert-Far @(
			$Data.Line -is [FarNet.ILine]
			$Data.Line.Text -eq ''
			$Data.Line.SelectedText -eq $null
		)
	}

	# type text
	macro 'Keys"q w e r t y"'
	job {
		$SelectionSpan = $Data.Line.SelectionSpan
		Assert-Far @(
			$Data.Line.Text -eq 'qwerty'
			$Data.Line.SelectedText -eq $null
			$Data.Line.Caret -eq 6
			$SelectionSpan.Length -eq -1
			$SelectionSpan.Start -eq -1
			$SelectionSpan.End -eq -2
		)
	}

	# home, right
	$kind = fun {$Far.Window.Kind}
	if ($kind -eq 'Editor') {
		macro 'Keys("Home Right")'
	}
	else {
		macro 'Keys("CtrlHome CtrlD")'
	}

	job {
		Assert-Far @(
			$Data.Line.Text -eq 'qwerty'
			$Data.Line.SelectedText -eq $null
			$Data.Line.Caret -eq 1
		)
	}

	# select 4 chars
	macro 'Keys"ShiftRight ShiftRight ShiftRight ShiftRight"'
	job {
		$SelectionSpan = $Data.Line.SelectionSpan
		Assert-Far @(
			$Data.Line.Text -eq 'qwerty'
			$Data.Line.SelectedText -eq 'wert'
			$Data.Line.Caret -eq 5
			$SelectionSpan.Length -eq 4
			$SelectionSpan.Start -eq 1
			$SelectionSpan.End -eq 5
		)
	}

	# set selected text
	job {
		$Data.Line.SelectedText = '12345'
	}
	job {
		$SelectionSpan = $Data.Line.SelectionSpan
		Assert-Far @(
			$Data.Line.Text -eq 'q12345y'
			$Data.Line.SelectedText -eq '12345'
			$Data.Line.Caret -eq 5
			$SelectionSpan.Length -eq 5
			$SelectionSpan.Start -eq 1
			$SelectionSpan.End -eq 6
		)
	}
}

### editor line
job {
	Open-FarEditor 'Test-Line..ps1.tmp'
}
job {
	Assert-Far ($Far.Line.WindowKind -eq 'Editor')
	$Data.Line = $Far.Line
	Assert-Far ($Data.Line.WindowKind -eq 'Editor')
	$Data.Line.Text = ''
}
GetTestLine
macro 'Keys"Esc n"'
job {
	Assert-Far -Panels
}

### dialog line
keys CtrlG
job {
	Assert-Far ($Far.Line.WindowKind -eq 'Dialog')
	$Data.Line = $Far.Line
	Assert-Far ($Data.Line.WindowKind -eq 'Dialog')
	$Data.Line.Text = ''
}
GetTestLine
keys Esc
job {
	Assert-Far -Panels
}

### command line
job {
	Assert-Far ($Far.Line.WindowKind -eq 'Panels')
	$Data.Line = $Far.CommandLine
	Assert-Far ($Data.Line.WindowKind -eq 'Panels')
	$Data.Line.Text = ''
}
GetTestLine
keys Esc
job {
	Assert-Far $Data.Line.Length -eq 0
}
