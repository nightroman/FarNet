
### Invoke commands

keys F11 2 1 g l Enter
job {
	Assert-Far -Viewer
	$__.Close()
	Assert-Far $r.Path -eq $PWD.Path
}
keys F11 2 1 $ _ = 5
job {
	Assert-Far -Dialog

	#! $__ "skips" input dialog
	Assert-Far ($__ -is [FarNet.IPanel])

	#! hence use $Far.Dialog
	Assert-Far $Far.Dialog[2].Text -eq '$_=5'
}
keys Enter
job {
	Assert-Far -Panels
	Assert-Far $r.Path -eq $PWD.Path
}

### Invoke selected

keys 4 2 ShiftLeft ShiftLeft F11 2 2 CtrlY
job {
	Assert-Far $r -eq 42
}
keys $ _ = 5 ShiftLeft ShiftLeft ShiftLeft ShiftLeft F11 2 2 CtrlY
job {
	Assert-Far $r -eq 42
}

### Command console

keys F11 2 3
[FarNet.Tasks]::WaitForDialog(999).Wait()
keys g l Enter
[FarNet.Tasks]::WaitForDialog(999).Wait()
job {
	Assert-Far -Dialog
	Assert-Far $r.Path -eq $PWD.Path
}
keys $ _ = 5 Enter
[FarNet.Tasks]::WaitForDialog(999).Wait()
job {
	Assert-Far -Dialog
	Assert-Far $r.Path -eq $PWD.Path
}
keys Esc

### Interactive

keys F11 2 5
job {
	$var.e = $Psf.Editor()
	Assert-Far ($var.e.Title.StartsWith('PS main session'))
}
keys 1 5 ShiftEnter
job {
	Assert-Far $r -eq 15
	Assert-Far $var.e[-4].Text -eq '15'
	Assert-Far $var.e[-3].Text -eq '>#>'
}
keys $ _ = 1 ShiftEnter
job {
	Assert-Far $r -eq 15
	Assert-Far $var.e[-4].Text -eq '$_=1'
	Assert-Far $var.e[-3].Text -eq '<##>'
}
keys Esc
job {
	Assert-Far -Panels
}

### Inter.async

keys F11 2 a
job {
	$var.e = $Psf.Editor()
	Assert-Far ($var.e.Title.StartsWith('PS async session'))
}
keys 1 1 ShiftEnter
[FarNet.Tasks]::Wait(9, 999, {$e[-4].Text -eq '11'}).Wait()
job {
	Assert-Far $var.e[-3].Text -eq '>#>'
}
keys $ _ = 1 ShiftEnter
[FarNet.Tasks]::Wait(9, 999, {$e[-5].Text -eq '$_=1'}).Wait()
job {
	Assert-Far $var.e[-3].Text -eq '<##>'
}
keys $ r ShiftEnter
[FarNet.Tasks]::Wait(9, 999, {$e[-4].Text -eq '11'}).Wait()
job {
	Assert-Far $var.e[-3].Text -eq '>#>'
}
keys Esc
job {
	Assert-Far -Panels
}
