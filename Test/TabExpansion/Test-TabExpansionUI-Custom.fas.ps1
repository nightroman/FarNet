<#
.Synopsis
	Test TabExpansion UI of custom pattern
#>

job {
	$Far.CommandLine.Text = ''
}

macro @'
Keys"A s s e r t - F ="
Plugin.Menu("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "7DEF4106-570A-41AB-8ECB-40605339E6F7")
Keys"7"
'@

job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Items[0].Text -eq 'Assert-Far (#)'
	Assert-Far $Far.Dialog[1].Items[1].Text -eq 'Assert-Far @('
}
macro 'Keys"Enter" -- take Assert-Far (#)'
job {
	Assert-Far -Panels
	Assert-Far $Far.CommandLine.Text -eq 'Assert-Far ()'
	Assert-Far $Far.CommandLine.Caret -eq 12
}
keys Esc
job {
	Assert-Far $Far.CommandLine.Length -eq 0
}
