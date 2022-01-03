<#
.Synopsis
	Initialize test environment.
#>

### Set panel modes assumed by tests
$p1 = $Far.Panel
$p2 = $Far.Panel2
Assert-Far (!$p1.ShowHidden -and !$p2.ShowHidden) -Message 'Set ShowHidden off in panels'
$p1.SortMode = $p2.SortMode = 'Name'

### SelectedFirst must be off
Assert-Far (!$p1.SelectedFirst -and !$p2.SelectedFirst) -Message "Panel mode 'SelectedFirst' must be turned OFF manually"

# clear
Clear-Session
