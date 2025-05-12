<#
.Synopsis
	Tests window methods and features.
#>

job {
	if ($global:Error) {throw 'Please remove errors.'}

	# IsModal
	Assert-Far (!$Far.Window.IsModal)

	# GetKindAt
	Assert-Far ($Far.Window.GetKindAt(-1) -eq 'Panels')
	Assert-Far ($Far.Window.GetKindAt(1) -eq 'Panels')

	# GetNameAt
	$name1 = $Far.Window.GetNameAt(-1)
	$name2 = $Far.Window.GetNameAt(1)
	Assert-Far $name1 -eq $name2
	Assert-Far (Test-Path -LiteralPath $name1)
}

### Editor

job {
	# open editor
	Open-FarEditor "$env:FarHome\Far.exe.example.ini"
}
job {
	# IsModal
	Assert-Far (!$Far.Window.IsModal)

	# GetKindAt
	Assert-Far ($Far.Window.GetKindAt(-1) -eq 'Editor')
	Assert-Far ($Far.Window.GetKindAt(2) -eq 'Editor')

	# GetNameAt
	$name1 = $Far.Window.GetNameAt(-1)
	$name2 = $Far.Window.GetNameAt(2)
	Assert-Far $name1 -eq $name2
	Assert-Far $name1 -eq "$env:FarHome\Far.exe.example.ini"
}

### Viewer

job {
	Open-FarViewer "$env:FarHome\FarEng.lng"

	#! used to be null
	Assert-Far $Far.Viewer.Title -eq $Far.Viewer.FileName
}
job {
	# IsModal
	Assert-Far (!$Far.Window.IsModal)

	# GetKindAt
	Assert-Far ($Far.Window.GetKindAt(-1) -eq 'Viewer')
	Assert-Far ($Far.Window.GetKindAt(3) -eq 'Viewer')

	# GetNameAt
	$name1 = $Far.Window.GetNameAt(-1)
	$name2 = $Far.Window.GetNameAt(3)
	Assert-Far $name1 -eq $name2
	Assert-Far $name1 -eq "$env:FarHome\FarEng.lng"
}

### Switching

job {
	# switch to editor
	$Far.Window.SetCurrentAt(2)
	Assert-Far $Far.Window.GetNameAt(-1) -eq "$env:FarHome\Far.exe.example.ini"
}
macro 'Keys"Esc" -- exit editor'
job {
	# viewer, switch to panels
	Assert-Far -Viewer
	$Far.Window.SetCurrentAt(1)
	Assert-Far -Panels
}
job {
	# switch to viewer
	$Far.Window.SetCurrentAt(1)
	Assert-Far -Viewer
}
macro 'Keys"Esc" -- exit viewer'
job {
	Assert-Far $Far.Window.Count -eq 2
}

### Dialog

keys CtrlG
job {
	# IsModal
	Assert-Far ($Far.Window.IsModal)

	# Far 3.0.2539: 2 windows, was 1
	# Far 3.0.4042: 3 windows, was 2
	Assert-Far $Far.Window.Count -eq 3

	# GetKindAt
	Assert-Far ($Far.Window.GetKindAt(-1) -eq 'Dialog')

	# GetNameAt
	$name = $Far.Window.GetNameAt(-1)
	Assert-Far $name -eq 'Apply command'

	# try to set panels, it fails
	$4 = ''
	try { $Far.Window.SetCurrentAt(1) }
	catch { $4 = "$_" }
	Assert-Far ($4 -like '*"FarNet::Window::SetCurrentAt failed, index = 1"')
	$global:Error.RemoveAt(0)
}
keys Esc
