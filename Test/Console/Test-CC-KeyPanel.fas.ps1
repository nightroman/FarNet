<#
.Synopsis
	Panel keys in command console.
#>

### init
job { $Far.Panel.GoToPath($PSCommandPath) }
job { $Psf.RunCommandConsole() }

### Up/Down
keys Up
job {
	[FarNet.Works.Tasks2]::Wait('Wait', { $Far.Window.Kind -eq 'Dialog' -and $Far.Dialog.TypeId -eq ([PowerShellFar.Guids]::ReadCommandDialog) })
	Assert-Far ($Far.Panel.CurrentFile -ne 'Test-CC-KeyPanel.fas.ps1')
}
keys Down
job {
	[FarNet.Works.Tasks2]::Wait('Wait', { $Far.Window.Kind -eq 'Dialog' -and $Far.Dialog.TypeId -eq ([PowerShellFar.Guids]::ReadCommandDialog) })
	Assert-Far -FileName 'Test-CC-KeyPanel.fas.ps1'
}

### kill
job { $Psf.StopCommandConsole() }
