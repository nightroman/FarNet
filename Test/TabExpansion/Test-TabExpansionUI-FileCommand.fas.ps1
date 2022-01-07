<#
.Synopsis
	TabExpansion UI with file/command items.
#>

job {
	$Data.Path0 = $Far.Panel.CurrentDirectory
	$Far.Panel.CurrentDirectory = 'C:\ROM\APS'
}

job {
	$Far.CommandLine.Text = 'Test-Fa'
}

macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "7DEF4106-570A-41AB-8ECB-40605339E6F7")
Keys"7" -- expand
'@

job {
	# items shown differently
	Assert-Far -Dialog
	$r = $Far.Dialog[1].Items
	Assert-Far $r.Count -eq 2
	Assert-Far $r[0].Text -eq '.\Test-Far.ps1'
	Assert-Far $r[1].Text -eq 'Test-Far.ps1'
}

macro 'Keys"Esc Esc" -- exit list, drop command line'

job {
	$Far.Panel.CurrentDirectory = $Data.Path0
}
