<#
.Synopsis
	Test panel menu
#>

# open panel, call menu, select item 1
job { & "$env:FarNetCode\Samples\Tests\Test-Panel-Menu-.ps1" }
macro 'Keys"ShiftF3 1"'

# just call item 2
macro 'Keys"ShiftF3 2 Esc"'

# check value directly
job {
	$e = $__.Value
	Assert-Far $e.Any -eq 'String 1'
}

# go to Item, select
job {
	Find-FarFile 'Item'
	Assert-Far ($__.CurrentFile.Description -like '*Far.exe')
}
keys Ins
# go to Process, select
job {
	Find-FarFile 'Process'
	Assert-Far ($__.CurrentFile.Description -like '*(Far)')
}
keys Ins
# go home, call item 2
macro 'Keys"Home ShiftF3 2 Esc"'

# checks that a handler has global scope: $tmp1,2
job {
	Remove-Variable tmp*
	Find-FarFile 'Any'
}
macro 'Keys"F1 3"'

# done
keys Esc
