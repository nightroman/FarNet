<#
.Synopsis
	F3/F4 in command console.

.Description
	!! Do not `Start-FarTask -Confirm`
#>

### init
job { $Far.Panel.GoToPath($PSCommandPath) }
job { $Psf.RunCommandConsole() }

### F3
keys F3
job {
	[FarNet.Works.Tasks2]::Wait('See .Description', { $Far.Window.Kind -eq 'Viewer' })
}
keys Esc
job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadCommandDialog)
}

### F4
keys F4
job {
	[FarNet.Works.Tasks2]::Wait('See .Description', { $Far.Window.Kind -eq 'Editor' })
}
keys Esc
job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadCommandDialog)
}

### kill
job { $Psf.StopCommandConsole() }
