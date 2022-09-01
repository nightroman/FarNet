
run {
	# register tool
	& "$env:PSF\Samples\Tests\Test-RegisterTool.far.ps1"
}

job {
	# info dialog?
	Assert-Far ($Far.Dialog[1].Text -match "^Test tool is registered")
}

macro 'Keys("F11") Menu.Select("PSF test tool", 2) Keys("Enter") -- invoke from menu'

job {
	# tool result dialog?
	Assert-Far $Far.Dialog[1].Text -eq 'Hello from Dialog'
}

macro 'Keys"Esc Esc" -- exit both dialogs'

job {
	# not dialog?
	Assert-Far ($Far.Window.Kind -ne 'Dialog')
}

run {
	# unregister tool
	& "$env:PSF\Samples\Tests\Test-RegisterTool.far.ps1"
}

job {
	# info dialog?
	Assert-Far $Far.Dialog[1].Text -eq "Test tool is unregistered"
}

# exit dialog
keys Esc
