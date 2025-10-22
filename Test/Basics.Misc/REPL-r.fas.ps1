
### Invoke commands

keys F11 2 1 g l Enter
job {
	Assert-Far -Viewer
	$Far.Viewer.Close()
	Assert-Far $r.Path -eq $PWD.Path
}
keys F11 2 1 $ _ = 5
job {
	Assert-Far -Dialog
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
[FarNet.Tasks]::WaitForWindow('Dialog', 999).Wait()
keys g l Enter
[FarNet.Tasks]::WaitForWindow('Dialog', 999).Wait()
job {
	Assert-Far -Dialog
	Assert-Far $r.Path -eq $PWD.Path
}
keys $ _ = 5 Enter
[FarNet.Tasks]::WaitForWindow('Dialog', 999).Wait()
job {
	Assert-Far -Dialog
	Assert-Far $r.Path -eq $PWD.Path
}
keys Esc
