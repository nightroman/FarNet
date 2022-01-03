
job {
	Open-FarEditor 'Test-Editor-Reindent-Selection..ps1.tmp'
}
job {
	$global:Editor = $Far.Editor
	Assert-Far -Editor
	Assert-Far @(
		$Editor.FileName.EndsWith('\Test-Editor-Reindent-Selection..ps1.tmp')
		$Editor.ExpandTabs -eq 'None'
		$Editor.TabSize -eq 4
	)
}

### Reindent 'second line after {', current
job {
	$Editor.SetText(@'
{
line1
line2
}
'@)
	$Editor.GoToLine(1)
	Assert-Far $Editor.Line.Text -eq 'line1'
}
job {
	Reindent-Selection-.ps1
	Assert-Far @(
		$Editor[0].Text -eq "{"
		$Editor[1].Text -eq "`tline1"
		$Editor[2].Text -eq "line2"
	)
}
keys CtrlZ
job {
	Assert-Far $Editor.Line.Text -eq "line1"
}

### Reindent 'second line after {', selection
job {
	$Editor.SelectText(0, 1, -1, 3)
}
job {
	Reindent-Selection-.ps1
	Assert-Far @(
		$Editor[0].Text -eq "{"
		$Editor[1].Text -eq "`tline1"
		$Editor[2].Text -eq "`tline2"
		$Editor[3].Text -eq "}"
	)
}
keys CtrlZ
job {
	Assert-Far @(
		$Editor[1].Text -eq "line1"
		$Editor[2].Text -eq "line2"
	)
}

### Original simple test
job {
	$Editor.SetText(@'
{
2
3
}
'@)
	$Editor.SelectAllText()
}
job {
	Reindent-Selection-.ps1
	Assert-Far $Editor.GetText() -eq @'
{
	2
	3
}
'@
}

# exit
macro 'Keys"Esc n"'
job {
	Assert-Far ($Far.Window.Kind -ne 'Editor')
	Remove-Variable -Scope global Editor
}
