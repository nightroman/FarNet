<#
.Synopsis
	Tests Snippet.ps1 indent, tabs, escaping.
#>

job {
	Open-FarEditor "$env:TEMP\$([guid]::NewGuid()).ps1" -DisableHistory -DeleteSource File
}

job {
	$Editor = $Far.Editor
	$Editor.ExpandTabs = 'All'
	$Editor.TabSize = 2
	$Editor.InsertText('  ')
	Snippet.ps1 foreach
}

job {
	Assert-Far -Editor
	$Editor = $Far.Editor
	Assert-Far $Editor.Count -eq 3
	Assert-Far $Editor.Caret.X -eq 16
	Assert-Far $Editor[0].Text -eq '  foreach($_ in ) {'
	Assert-Far $Editor[1].Text -eq "    `$_"
	Assert-Far $Editor[2].Text -eq '  }'
}

job {
	$Far.Editor.Close()
}
