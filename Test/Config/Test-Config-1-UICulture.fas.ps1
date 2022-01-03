# UICulture/FarNet.Demo, run it first, it creates the module.
# Keep the module due to `ru`, used for other testing.

$Data.getModule = {
	[FarNet.Works.Config]::Default.Reset()
	[FarNet.Works.Config]::Default.GetData().GetModule('FarNet.Demo')
}

macro 'Keys "F9 o u 1 u" Menu.Select("FarNet.Demo", 3) Keys "Enter"'
job {
	# set Culture '' ~ default
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'FarNet.Demo'
	$Far.Dialog[2].Text = ''
}
keys Enter
job {
	# data removed
	$module = & $Data.getModule
	Assert-Far $null -eq $module
}

macro 'Menu.Select("FarNet.Demo", 3) Keys "Enter"'
job {
	# set Culture `ru`
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'FarNet.Demo'
	$Far.Dialog[2].Text = 'ru'
}
keys Enter
job {
	# data exist
	$module = & $Data.getModule
	Assert-Far $module.Culture -eq 'ru'
}

keys Esc Esc Esc
