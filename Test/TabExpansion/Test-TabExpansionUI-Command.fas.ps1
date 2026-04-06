<#
.Synopsis
	TabExpansion UI with a command.
#>

job {
	$Far.CommandLine.Text = 'get-ex'
}

macro @'
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "7DEF4106-570A-41AB-8ECB-40605339E6F7")
Keys"7" -- Complete
'@

job {
	# 2+ items
	Assert-Far -Dialog
	$r = @($__[1].Items).ForEach('Text')
	Assert-Far ($r.Contains('Get-ExecutionPolicy'))
	Assert-Far ($r.Contains('Get-ExperimentalFeature'))
}

keys Esc Esc # exit list, drop command line
