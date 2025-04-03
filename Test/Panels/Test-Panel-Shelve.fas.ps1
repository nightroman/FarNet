<#
.Synopsis
	Test Push/Shelve for Far panels.

.Description
	Push/Pop of FarNet panels is tested in "Test-Panel-Select..ps1"
#>

$GoHome = { job {
	# go home
	$Panel = $Far.Panel
	$Panel.CurrentDirectory = $HOME
	Assert-Far $Panel.CurrentDirectory -eq $HOME
	Assert-Far $Panel.CurrentIndex -eq 0
}}

$Unshelve = { job {
	# unshelve the last shelved
	$top = @([FarNet.Works.ShelveInfo]::Stack)[-1]
	$null = [FarNet.Works.ShelveInfo]::Stack.Remove($top)
	$top.Pop()
}}

### Shelve with a current and none really selected

job {
	# go to Far.exe
	$Far.Panel.GoToPath("$env:FARHOME\Far.exe")
}

$Test = { job {
	# current == Far.exe, selected == Far.exe
	$Panel = $Far.Panel
	Assert-Far $Panel.CurrentFile.Name -eq 'Far.exe'
	$selected = $Panel.SelectedFiles
	Assert-Far (($selected.Count -eq 1) -and ($selected[0].Name -eq 'Far.exe'))
}}
& $Test

job {
	# shelve
	$Far.Panel.Push()
	$top = @([FarNet.Works.ShelveInfo]::Stack)[-1]
	Assert-Far @(
		$top.Current -eq 'Far.exe'
		!$top.Selected
	)
}

& $GoHome

& $Unshelve

& $Test

### Shelve with one really selected

# select 1
keys ShiftDown
$Test = { job {
	# current != Far.exe, selected == Far.exe
	$Panel = $Far.Panel
	Assert-Far ($Panel.CurrentFile.Name -ne 'Far.exe')
	$selected = $Panel.SelectedFiles
	Assert-Far (($selected.Count -eq 1) -and ($selected[0].Name -eq 'Far.exe'))
}}
& $Test

job {
	# shelve
	$Far.Panel.Push()
	$top = @([FarNet.Works.ShelveInfo]::Stack)[-1]
	Assert-Far $top.GetSelectedNames().Count -eq 1
	Assert-Far $top.GetSelectedNames()[0] -eq 'Far.exe'
}

& $GoHome

& $Unshelve

& $Test

### Shelve with many selected

# select 2 more, go to Far.exe
macro 'Keys"ShiftDown ShiftDown Up Up Up"'

$Test = { job {
	# current == Far.exe and 3 selected items with Far.exe
	$Panel = $Far.Panel
	Assert-Far $Panel.CurrentFile.Name -eq 'Far.exe'
	$selected = $Panel.SelectedFiles
	Assert-Far (($selected.Count -eq 3) -and ($selected[0].Name -eq 'Far.exe'))
}}
& $Test

job {
	# shelve
	$Far.Panel.Push()
	$top = @([FarNet.Works.ShelveInfo]::Stack)[-1]
	Assert-Far $top.GetSelectedNames().Count -eq 3
	Assert-Far $top.GetSelectedNames()[0] -eq 'Far.exe'
}

& $GoHome

& $Unshelve

& $Test

### Open/close FarNet panel, original panel should be restored

job {
	# open FarNet panel
	& "$env:FarNetCode\Samples\Tests\Test-Panel.far.ps1"
}
job {
	Assert-Far -Plugin
}

# close by Close()
macro 'Keys"Esc 1"'

& $Test

### Open FarNet panel and unshelve a file panel

job {
	# shelve
	$Far.Panel.Push()
}

job {
	# open FarNet panel
	& "$env:FarNetCode\Samples\Tests\Test-Panel.far.ps1"
}
job {
	Assert-Far -Plugin
}

& $Unshelve

& $Test
