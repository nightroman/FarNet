<#
.Synopsis
	Panel keys in command console.
#>

### init
job { $__.GoToPath($PSCommandPath) }
fun { $Psf.RunCommandConsole() }

### Up/Down
keys Up
fun {
	[FarNet.Works.Tasks2]::Wait('Wait', { $Far.Window.Kind -eq 'Dialog' -and $Far.Dialog.TypeId -eq ([PowerShellFar.Guids]::ReadCommandDialog) })
	Assert-Far ($__.CurrentFile -ne 'Test-CC-KeyPanel.fas.ps1')
}
keys Down
fun {
	[FarNet.Works.Tasks2]::Wait('Wait', { $Far.Window.Kind -eq 'Dialog' -and $Far.Dialog.TypeId -eq ([PowerShellFar.Guids]::ReadCommandDialog) })
	Assert-Far -FileName 'Test-CC-KeyPanel.fas.ps1'
}

keys Esc # exit CC
