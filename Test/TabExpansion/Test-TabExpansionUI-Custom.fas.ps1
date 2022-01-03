<#
.Synopsis
	Test TabExpansion UI of custom pattern
#>

job {
	$Far.CommandLine.Text = ''
}

macro 'Keys"A s s e r t - F = F11 2 7"'

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
