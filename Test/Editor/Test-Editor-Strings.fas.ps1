<#
.Synopsis
	Test editor Strings list
#>

job {
	Open-FarEditor Test-Editor-Strings..ps1.tmp
}
job {
	Assert-Far -EditorFileName *\Test-Editor-Strings..ps1.tmp
}
job {
	### Add
	$Strings = $Far.Editor.Strings
	$Strings.Add('line 0')
	$Strings.Add('line 1')
	$Strings.Add('line 2')
	$Strings.Add('line 3')
	$Strings.Add('line 4')
	Assert-Far @(
		$Strings.Count -eq 6
		$Strings[0] -eq 'line 0'
		$Strings[4] -eq 'line 4'
		$Strings[5] -eq ''
	)
	Assert-Far $Far.Editor.GetText() -eq @'
line 0
line 1
line 2
line 3
line 4

'@
}
job {
	### RemoveAt
	$Strings = $Far.Editor.Strings
	$Strings.RemoveAt(3)
	$Strings.RemoveAt(1)
	Assert-Far $Strings.Count -eq 4
	Assert-Far $Far.Editor.GetText() -eq @'
line 0
line 2
line 4

'@
}
job {
	### RemoveAt last, Insert the first
	$Strings = $Far.Editor.Strings
	$Strings.RemoveAt(3)
	$Strings.Insert(0, 'first')
	Assert-Far $Strings.Count -eq 4
	Assert-Far $Far.Editor.GetText() -eq @'
first
line 0
line 2
line 4
'@
}
job {
	### RemoveAt first, Add when last is not empty
	$Strings = $Far.Editor.Strings
	$Strings.RemoveAt(0)
	$Strings.Add('last')
	Assert-Far $Strings.Count -eq 4
	Assert-Far $Far.Editor.GetText() -eq @'
line 0
line 2
line 4
last
'@
}
job {
	### Clear - not the same as for normal IList<string>
	$Strings = $Far.Editor.Strings
	$Strings.Clear()
	Assert-Far @(
		$Strings.Count -eq 1
		$Strings[0] -eq ''
	)
}
macro 'Keys"Esc n"'
job {
	Assert-Far ($Far.Window.Kind -ne 'Editor')
}
