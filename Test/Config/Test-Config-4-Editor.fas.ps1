# Editors/FSharpFar
# assume it keeps defaults, to be removed by test
# assume default mask = *.fs;*.fsx;*.fsscript

$Data.getModule = {
	[FarNet.Works.Config]::Default.Reset()
	[FarNet.Works.Config]::Default.GetData().GetModule('FSharpFar')
}

macro 'Keys "F9 o u 1 e" Menu.Select("b7916b53-2c17-4086-8f13-5ffcf0d82900", 3) Keys "Enter"'
job {
	# set mask to not default
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'FSharpFar'
	Assert-Far $Far.Dialog[2].Text -eq '*.fs;*.fsx;*.fsscript'
	$Far.Dialog[2].Text = '*.fs;*.fsx'
}
keys Enter
job {
	# data exist
	$module = & $Data.getModule
	$action = $module.GetEditor("b7916b53-2c17-4086-8f13-5ffcf0d82900")
	Assert-Far @(
		$action
		$action.Mask -eq '*.fs;*.fsx'
	)
}

macro 'Menu.Select("b7916b53-2c17-4086-8f13-5ffcf0d82900", 3) Keys "Enter"'
job {
	# set mask to default
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'FSharpFar'
	Assert-Far $Far.Dialog[2].Text -eq '*.fs;*.fsx'
	$Far.Dialog[2].Text = '*.fs;*.fsx;*.fsscript'
}
keys Enter
job {
	# data removed
	$module = & $Data.getModule
	Assert-Far $null -eq $module
}

keys Esc Esc Esc
