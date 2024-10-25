# rk: simple operations

### create

job {
	$Far.InvokeCommand('rk:')
}
keys F7
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'Create String'
	Assert-Far $Far.Dialog[2].Text -eq ''
	Assert-Far $Far.Dialog[4].Text -eq ''
}
keys 1 - t e s t - o p s Enter
job {
	Assert-Far -FileName 1-test-ops
}

### rename

keys ShiftF6
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'Rename key'
	Assert-Far $Far.Dialog[2].Text -eq 1-test-ops
	Assert-Far $Far.Dialog[4].Text -eq ''
}
keys z - t e s t - o p s Enter
job {
	Assert-Far -FileName z-test-ops
}

### clone

keys ShiftF5
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq "Clone 'z-test-ops'"
	Assert-Far $Far.Dialog[2].Text -eq z-test-ops
	Assert-Far $Far.Dialog[4].Text -eq ''
}
keys 2 - t e s t - o p s Enter
job {
	Assert-Far -FileName z-test-ops
}

### delete, go to new, delete

keys Del
job {
	Assert-Far $Far.Dialog[1].Text -eq 'Delete 1 key(s):'
	Assert-Far $Far.Dialog[2].Text -eq 'z-test-ops'
}
keys Enter
job {
	Assert-Far (Test-RedisKey z-test-ops) -eq 0L
	Find-FarFile 2-test-ops
}
keys Del
job {
	Assert-Far $Far.Dialog[1].Text -eq 'Delete 1 key(s):'
	Assert-Far $Far.Dialog[2].Text -eq '2-test-ops'
}
keys Enter Esc

