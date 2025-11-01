<#
.Synopsis
	Test-Go-Selection.ps1
#>

job {
	Open-FarEditor 'Test-Go-Selection.ps1.tmp'
}
job {
	$global:Editor = $__
	Assert-Far -EditorFileName *\Test-Go-Selection.ps1.tmp
}

### one line, to start
job {
	# make caret at the selection end
	$Editor.SetText("01234567890")
	$Editor.SelectText(3, 0, 7, 0)
	$Editor.GoToColumn(8)
	Assert-Far @(
		$Editor.Caret.X -eq 8
		$Editor.GetSelectedText() -eq '34567'
	)
}
job {
	& $env:FarNetCode\Samples\Tests\Test-Go-Selection.ps1
	Assert-Far @(
		!$Editor.SelectionExists
		$Editor.Caret.X -eq 3
	)
}

### one line, to end
job {
	# make caret at the selection start
	$Editor.SetText("01234567890")
	$Editor.SelectText(3, 0, 7, 0)
	$Editor.GoToColumn(3)
	Assert-Far @(
		$Editor.Caret.X -eq 3
		$Editor.GetSelectedText() -eq '34567'
	)
}
job {
	& $env:FarNetCode\Samples\Tests\Test-Go-Selection.ps1 -End
	Assert-Far @(
		!$Editor.SelectionExists
		$Editor.Caret.X -eq 8
	)
}

# exit
macro 'Keys"Esc n"'
job {
	Assert-Far ($Far.Window.Kind -ne 'Editor')
	Remove-Variable -Scope global Editor
}
