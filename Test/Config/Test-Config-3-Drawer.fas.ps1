# Drawers/PowerShell breakpoints
# assume it keeps defaults, to be removed by test
# assume default Mask = *.ps1;*.psm1 Priority = 1

$Data.getModule = {
	[FarNet.Works.Config]::Default.Reset()
	[FarNet.Works.Config]::Default.GetData().GetModule('PowerShellFar')
}

macro 'Keys "F9 o u 1 d" Menu.Select("67db13c5-6b7b-4936-b984-e59db08e23c7", 3) Keys "Enter"'
job {
	# set mask *.ps1
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'PowerShell breakpoints'
	Assert-Far $Far.Dialog[2].Text -eq '*.ps1;*.psm1'
	Assert-Far $Far.Dialog[4].Text -eq '1'
	$Far.Dialog[2].Text = '*.ps1'
}
keys Enter
job {
	# data exist
	$module = & $Data.getModule
	$action = $module.GetDrawer("67db13c5-6b7b-4936-b984-e59db08e23c7")
	Assert-Far @(
		$action
		$action.Mask -eq '*.ps1'
		$action.Priority -eq $null
	)
}

macro 'Menu.Select("67db13c5-6b7b-4936-b984-e59db08e23c7", 3) Keys "Enter"'
job {
	# set mask to default, priority to not default
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'PowerShell breakpoints'
	Assert-Far $Far.Dialog[2].Text -eq '*.ps1'
	Assert-Far $Far.Dialog[4].Text -eq '1'
	$Far.Dialog[2].Text = '*.ps1;*.psm1'
	$Far.Dialog[4].Text = '2'
}
keys Enter
job {
	# data exist
	$module = & $Data.getModule
	$action = $module.GetDrawer("67db13c5-6b7b-4936-b984-e59db08e23c7")
	Assert-Far @(
		$action
		$action.Mask -eq $null
		$action.Priority -eq '2'
	)
}

macro 'Menu.Select("67db13c5-6b7b-4936-b984-e59db08e23c7", 3) Keys "Enter"'
job {
	# set priority to default
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'PowerShell breakpoints'
	Assert-Far $Far.Dialog[2].Text -eq '*.ps1;*.psm1'
	Assert-Far $Far.Dialog[4].Text -eq '2'
	$Far.Dialog[4].Text = '1'
}
keys Enter
job {
	# data removed
	$module = & $Data.getModule
	Assert-Far $null -eq $module
}

keys Esc Esc Esc
