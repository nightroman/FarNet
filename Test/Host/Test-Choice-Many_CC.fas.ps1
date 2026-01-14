<#
.Synopsis
	PromptForChoice() many
#>

### Help, Enter with default
fun { $Psf.RunCommandConsole() }

$Data.result = $null
run {
	$Data.result = & $PSScriptRoot\Get-Choice2.ps1
}
job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq 'Choice[0]: '
}

# help
keys ? Enter
job {
	#??Assert-Far -Viewer
}

# Select 0
job {
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq 'Choice[0]: '
}
keys Enter Esc
job {
	Assert-Far -Panels
	Assert-Far $Data.result -eq 0
}

### Select 1, 0
fun { $Psf.RunCommandConsole() }

$Data.result = $null
run {
	$Data.result = & $PSScriptRoot\Get-Choice2.ps1
}
keys n Enter y Enter Enter Esc
job {
	Assert-Far -Panels
	Assert-Far ($Data.result -join '|') -eq '1|0'
}
