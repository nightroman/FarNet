<#
.Synopsis
	F3/F4 in command console.
#>

### init
job { $__.GoToPath($PSCommandPath) }
job { $Psf.RunCommandConsole() }

### F4
keys F4
job {
	Assert-Far -Editor
	$__.SetText("'text from editor'")
	$__.Save()
	$__.Close()
}

job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadCommandDialog)
	Assert-Far $Far.Dialog[1].Text -eq "'text from editor'"
	$Far.Dialog[1].Text = ''
}

keys Esc # exit CC
