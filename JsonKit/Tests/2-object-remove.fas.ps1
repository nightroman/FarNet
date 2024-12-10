
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

keys Del # remove
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Remove items (1)?'
}
keys Enter
job {
	Assert-Far -FileName name
}

keys ShiftEsc # try exit
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'JSON has been modified. Save?'
}
keys Esc # [Cancel] exit
job {
	Assert-Far -FileName name
}

keys Esc
job {
	Assert-Far -FileDescription '{"name":"Joe"}'
}

keys Esc
job {
	Assert-Far -FileDescription '{"nest2":{"name":"Joe"}}'
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
