# rk: tree operations

### create

job {
	Remove-RedisKey (Search-RedisKey test-ops-3:*)
	Set-RedisString test-ops-3:dummy 1
	$Far.InvokeCommand('rk:tree root=test-ops-3:')
}
keys F7
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'Create String'
	Assert-Far $Far.Dialog[2].Text -eq ''
	Assert-Far $Far.Dialog[4].Text -eq 'test-ops-3:'
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
	Assert-Far $Far.Dialog[4].Text -eq 'test-ops-3:'
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
	Assert-Far $Far.Dialog[4].Text -eq 'test-ops-3:'
}
keys 2 - t e s t - o p s Enter
job {
	Assert-Far -FileName z-test-ops
}

### test new, delete all

job {
	Find-FarFile 2-test-ops
}
keys Multiply Del
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Delete 0 folder(s), 3 key(s):'
	Assert-Far $Far.Dialog[2].Text -eq '2-test-ops'
	Assert-Far $Far.Dialog[3].Text -eq dummy
	Assert-Far $Far.Dialog[4].Text -eq z-test-ops
}
keys Enter Esc
