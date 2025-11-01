﻿
run {
	# register tool
	& "$env:FarNetCode\Samples\Tests\Test-RegisterTool.far.ps1"
}

job {
	# info dialog?
	Assert-Far $__[1].Text -eq "Tool 'PSF test tool' is registered. Try it in F11 menus, e.g. now."
}

macro 'Keys("F11") Menu.Select("PSF test tool", 2) Keys("Enter") -- invoke from menu'

job {
	# tool result dialog?
	Assert-Far $__[1].Text -eq 'Hello from Dialog'
}

macro 'Keys"Esc Esc" -- exit both dialogs'

job {
	# not dialog?
	Assert-Far ($Far.Window.Kind -ne 'Dialog')
}

run {
	# unregister
	$Far.GetModuleAction('f2a1fc38-35d0-4546-b67c-13d8bb93fa2e').Unregister()
}
