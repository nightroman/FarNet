<#
.Synopsis
	Far: empty Unicode file with BOM used to be opened as 1251 with яю
#>

$Data.File = 'C:\TEMP\test-edit.tmp'

job {
	# write empty Unicode
	[System.IO.File]::WriteAllText($Data.File, '', [System.Text.Encoding]::Unicode)

	# setup editor, use -DeleteSource, just to test
	$editor = New-FarEditor $Data.File -DeleteSource 'File' -DisableHistory
	$editor.Open()
}
job {
	# fixture: it is Unicode and file is empty
	Assert-Far -Editor
	Assert-Far $__.CodePage -eq 1200
	Assert-Far $__.GetText() -eq ''
}
# exit not modified editor
keys Esc
job {
	# test: file is deleted automatically
	Assert-Far -Panels
	Assert-Far (!(Test-Path $Data.File))
}
