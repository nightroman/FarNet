# Tools/FarNet.Demo
# assume the module exists (due to `ru`)

$Data.getModule = {
	[FarNet.Works.Config]::Default.Reset()
	[FarNet.Works.Config]::Default.GetData().GetModule('FarNet.Demo')
}

macro 'Keys "F9 o u 1 t" Menu.Select("Трассировка", 3) Keys "Enter"'
job {
	# uncheck Panels
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq 'Трассировка'
	$__[1].Selected = 0
}
keys Enter
job {
	# data exist
	$module = & $Data.getModule
	$action = $module.GetTool("a10218a8-76b3-47f7-8900-3a162bf16c49")
	Assert-Far @(
		$action
		$action.Options -eq '55'
	)
}

macro 'Menu.Select("Трассировка", 3) Keys "Enter"'
job {
	# check Panels ~ default
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq 'Трассировка'
	Assert-Far $__[1].Selected -eq 0
	$__[1].Selected = 1
}
keys Enter
job {
	# data removed
	$module = & $Data.getModule
	Assert-Far $module.Culture -eq 'ru'
}

keys Esc Esc Esc
