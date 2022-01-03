<#
.Synopsis
	TabExpansion UI with a command.
#>

job {
	$Far.CommandLine.Text = 'Get-Hist'
}

macro 'Keys"F11 2 7" -- expand'

job {
	# two items shown differently
	Assert-Far -Dialog
	$r = $Far.Dialog[1].Items
	Assert-Far $r.Count -eq 2
	Assert-Far $r[0].Text -eq 'Get-History'
	Assert-Far $r[1].Text -eq 'Microsoft.PowerShell.Core\Get-History'
}

macro 'Keys"Esc Esc" -- exit list, drop command line'
