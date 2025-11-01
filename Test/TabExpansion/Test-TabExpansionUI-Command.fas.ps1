<#
.Synopsis
	TabExpansion UI with a command.
#>

job {
	$Far.CommandLine.Text = 'Get-Hist'
}

macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "7DEF4106-570A-41AB-8ECB-40605339E6F7")
Keys"7" -- expand
'@

job {
	# two items shown differently
	Assert-Far -Dialog
	$r = $__[1].Items
	Assert-Far $r.Count -eq 2
	Assert-Far $r[0].Text -eq 'Get-History'
	Assert-Far $r[1].Text -eq 'Microsoft.PowerShell.Core\Get-History'
}

macro 'Keys"Esc Esc" -- exit list, drop command line'
