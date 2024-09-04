
ps: {
	Invoke-Build * $PSScriptRoot\Reformat-Selection.test.ps1
}

job {
	Open-FarEditor Test-Editor-Reformat-Selection..ps1.tmp
}
job {
	$global:Editor = $Far.Editor
	Assert-Far -EditorFileName *\Test-Editor-Reformat-Selection..ps1.tmp
}

### one line
job {
	$Editor.Add("`t" * 10 + '///  This is a single line to be reformatted as two lines')
	Assert-Far $Editor.Caret.Y -eq 1
}
job {
	$Editor.GoToLine(0)
	Assert-Far $Editor.Caret.Y -eq 0

	# test with margins and tabulation
	Reformat-Selection.ps1 79 4

	# 3 lines, 2 reformatted, 2nd is current
	Assert-Far @(
		$Editor.Count -eq 3
		$Editor[0].Text -eq "`t" * 10 + '///  This is a single line to be'
		$Editor[1].Text -eq "`t" * 10 + '///  reformatted as two lines'
		$Editor.Caret.Y -eq 1
	)

	# the caret is at the end
	$Line = $Editor.Line
	Assert-Far $Line.Caret -eq $Line.Length
}
# test Undo
keys CtrlZ
job {
	Assert-Far @(
		$Editor.Count -eq 2
		$Editor[0].Text -eq "`t" * 10 + '///  This is a single line to be reformatted as two lines'
	)
}

### many lines
job {
	$Editor.SetText(@"
` /// 12345 12345 12345 12345
` /// 12345 12345 12345 12345
` /// </para>
"@)
	$Editor.SelectText(0, 0, -1, 2)
	$Editor.GoTo(0, 2)
}
job {
	# test
	Reformat-Selection.ps1 20
}
job {
	$Caret = $Editor.Caret
	Assert-Far @(
		$Caret.X -eq 0
		$Caret.Y -eq 4
		$Editor.GetText() -eq @"
` /// 12345 12345
` /// 12345 12345
` /// 12345 12345
` /// 12345 12345
` /// </para>
"@)
}

# exit
macro 'Keys"Esc n"'
job {
	Assert-Far ($Far.Window.Kind -ne 'Editor')
	Remove-Variable -Scope global Editor
}
