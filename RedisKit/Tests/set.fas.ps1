
job {
	Import-Module $PSScriptRoot\zoo.psm1
	Remove-RedisKey test:set
	$Far.InvokeCommand('rk:set key=test:set')
}
job {
	$r = $Far.Panel
	Assert-Far $r.GetType().Name -eq SetPanel
	Assert-Far $r.Title -eq 'Set test:set'
}

keys Esc
job {
	Assert-Far -Native
	Assert-Far (Test-RedisKey test:set) -eq 0L
	$Far.InvokeCommand('rk:set key=test:set')
}
job {
	Assert-Far -ExplorerTypeId 75bbcfef-c464-4c80-a602-83b15bf404f9
}

### F7: create q2
keys F7 q 2 Enter
job {
	Assert-Far -FileName q2
	Assert-Far (Get-RedisSet test:set).Contains('q2')
}

### ShiftF6: rename to q1
keys ShiftF6 q 1 Enter
job {
	Assert-Far -FileName q1
	Assert-Far (Get-RedisSet test:set).Contains('q1')
}

### F7: create q3
keys F7 q 3 Enter
job {
	Assert-Far -FileName q3
	$r = Get-RedisSet test:set
	Assert-Far $r.Contains('q1')
	Assert-Far $r.Contains('q3')
	$r = $Far.Panel.Files
	Assert-Far $r[0].Name -eq q1
	Assert-Far $r[1].Name -eq q3
}

### ShiftF5: clone q3 -> q2
keys ShiftF5
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[2].Text -eq q3
}
keys q 2 Enter
job {
	Assert-Far -FileName q3
	$r = Get-RedisSet test:set
	Assert-Far $r.Count -eq 3
	Assert-Far $r.Contains('q2')
	$r = $Far.Panel.Files
	Assert-Far $r[0].Name -eq q1
	Assert-Far $r[1].Name -eq q2
	Assert-Far $r[2].Name -eq q3
}

### F8: delete q3
keys F8
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[2].Text -eq q3
}
keys Enter
job {
	Assert-Far -FileName q2
	$r = Get-RedisSet test:set
	Assert-Far ($r -join '|') -eq 'q1|q2'
}

### exit
job {
	$Far.Panel.Close()
	Remove-RedisKey test:set
}
