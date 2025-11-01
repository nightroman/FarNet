# Commands/FarNet.Demo
# assume module exists (due to `ru`)

$Data.getModule = {
	[FarNet.Works.Config]::Default.Reset()
	[FarNet.Works.Config]::Default.GetData().GetModule('FarNet.Demo')
}

macro 'Keys "F9 o u 1 c" Menu.Select("FarNet.Demo", 3) Keys "Enter"'
job {
	# set Prefix=demo2
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq 'FarNet.Demo Command'
	$__[2].Text = 'demo2'
}
keys Enter
job {
	# data exist
	$module = & $Data.getModule
	$action = $module.GetCommand("e3b61c33-a71f-487d-bad3-5542aed112d6")
	Assert-Far $action
	Assert-Far $action.Prefix -eq 'demo2'
}

macro 'Menu.Select("FarNet.Demo", 3) Keys "Enter"'
job {
	# set Prefix='' ~ default
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq 'FarNet.Demo Command'
	$__[2].Text = ''
}
keys Enter
job {
	# data removed
	$module = & $Data.getModule
	Assert-Far @(
		$module.Culture -eq 'ru'
		$module.Commands.Count -eq 0
		$module.Drawers.Count -eq 0
		$module.Editors.Count -eq 0
		$module.Tools.Count -eq 0
	)
}

keys Esc Esc Esc
