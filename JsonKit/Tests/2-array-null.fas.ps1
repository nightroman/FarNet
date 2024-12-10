
# to test it's not saved
$Data.time = (Get-Item $PSScriptRoot\x-array.json).LastWriteTime

job {
	$Far.InvokeCommand('jk:open file=x-array.json')
}

job {
	Find-FarFile '[[1,2]]'
}

keys Enter

job {
	Find-FarFile '[1,2]'
}

keys Enter

job {
	Find-FarFile '1'
}

keys ShiftDel # set null
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Set nulls (1)?'
}
keys Enter
job {
	Assert-Far -FileName null
}

keys ShiftEsc # try exit
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'JSON has been modified. Save?'
}
keys Esc # [Cancel] exit
job {
	Assert-Far -FileName null
}

keys Esc
job {
	Assert-Far -FileName '[null,2]'
}

keys Esc
job {
	Assert-Far -FileName '[[null,2]]'
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
	Assert-Far $Data.time -eq (Get-Item $PSScriptRoot\x-array.json).LastWriteTime
}
