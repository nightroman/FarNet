
# to test it's not saved
$Data.time = (Get-Item $PSScriptRoot\x-object.json).LastWriteTime

job {
	$Far.InvokeCommand('jk:open file=x-object.json')
}

job {
	Find-FarFile nest1
}

keys Enter

job {
	Find-FarFile nest2
}

keys Enter

job {
	Find-FarFile id
}

keys ShiftDel # set null
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Set nulls (1)?'
}
keys Enter
job {
	Assert-Far -FileName id -FileDescription null
}

keys ShiftEsc # try exit
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'JSON has been modified. Save?'
}
keys Esc # [Cancel] exit
job {
	Assert-Far -FileName id -FileDescription null
}

keys Esc
job {
	Assert-Far -FileDescription '{"id":null,"name":"Joe"}'
}

keys Esc
job {
	Assert-Far -FileDescription '{"nest2":{"id":null,"name":"Joe"}}'
}

keys Esc # try exit
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'JSON has been modified. Save?'
}

keys Esc # [Cancel] exit
job {
	Assert-Far -Plugin
}

keys Esc # try exit
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'JSON has been modified. Save?'
}

keys Right Enter # [No], exit without saving
job {
	Assert-Far -Native
	Assert-Far $Data.time -eq (Get-Item $PSScriptRoot\x-object.json).LastWriteTime
}
