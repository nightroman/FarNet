<#
.Synopsis
	Command console history tests.
#>

### run 2 dummy commands, explore 2 history items and exit

job { [PowerShellFar.Zoo]::StartCommandConsole() }
keys 1 Enter 2 Enter # run 2 commands
job {
	Assert-Far $Far.Dialog[0].Text -eq ''
}
keys Up
job {
	Assert-Far $Far.Dialog[0].Text -eq '2'
}
keys Up
job {
	Assert-Far $Far.Dialog[0].Text -eq '1'
}
keys Esc # test clear prompt
job {
	Assert-Far -DialogTypeId ([PowerShellFar.Guids]::ReadCommandDialog)
	Assert-Far $Far.Dialog[0].Text -eq ''
}
job { [PowerShellFar.Zoo]::ExitCommandConsole() }

### repeat and compare, do not exit

job { [PowerShellFar.Zoo]::StartCommandConsole() }
keys Up
job {
	Assert-Far $Far.Dialog[0].Text -eq '2'
}
keys Up
job {
	Assert-Far $Far.Dialog[0].Text -eq '1'
}

### run a dummy command, test reset navigation

keys Esc 1 Enter
job {
	Assert-Far $Far.Dialog[0].Text -eq ''
}
keys Up
job {
	Assert-Far $Far.Dialog[0].Text -eq '1'
}
keys Up
job {
	Assert-Far $Far.Dialog[0].Text -eq '2'
}
job { [PowerShellFar.Zoo]::ExitCommandConsole() }
