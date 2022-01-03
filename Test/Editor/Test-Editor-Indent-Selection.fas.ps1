
job {
	Open-FarEditor 'Test-Editor-Indent-Selection..ps1.tmp'
}
job {
	$global:Editor = $Far.Editor
	Assert-Far -Editor
	Assert-Far @(
		$Editor.FileName.EndsWith('\Test-Editor-Indent-Selection..ps1.tmp')
		$Editor.ExpandTabs -eq 'None'
		$Editor.TabSize -eq 4
	)
}
job {
	$Editor.SetText(@'
{
 2
  3
   }
'@)
	$Editor.SelectText(0, 0, 0, 3)
}
job {
	Indent-Selection-.ps1
	Assert-Far $Editor.GetText() -eq @'
	{
	 2
	  3
	   }
'@
}
job {
	Indent-Selection-.ps1 -Back
	Assert-Far $Editor.GetText() -eq @'
{
 2
  3
   }
'@
}
job {
	Indent-Selection-.ps1 -Back
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
