<#
.Synopsis
	PromptForChoice() many
#>

### Help, CtrlEnter with default
$Data.result = $null
run {
	$Data.result = & $PSScriptRoot\Get-Choice2.ps1
}
job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq Confirm
	Assert-Far $__[1].Text -eq "Are you sure you want to continue?"
}

# help
keys ? Enter
job {
	Assert-Far -Viewer
}
keys Esc

# CtrlEnter closes even on <Help>
job {
	Assert-Far -Dialog
	$box = $__[2]
	Assert-Far $box.Items[$box.Selected].Text -eq '&? Help'
}
keys CtrlEnter
job {
	Assert-Far -Panels
	Assert-Far $Data.result -eq 0
}

### Select 1, 0
$Data.result = $null
run {
	$Data.result = & $PSScriptRoot\Get-Choice2.ps1
}
keys n Enter y Enter CtrlEnter
job {
	Assert-Far -Panels
	Assert-Far ($Data.result -join '|') -eq '1|0'
}
