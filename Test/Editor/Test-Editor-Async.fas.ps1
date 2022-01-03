<#
.Synopsis
	Async mode in editor.
#>

if (Test-Path C:\TEMP\Test-EditorAsync*.txt) {
	Remove-Item C:\TEMP\Test-EditorAsync*.txt
}

job {
	# open editor 1
	Open-FarEditor C:\TEMP\Test-EditorAsync1.txt
	$Data.Editor1 = $Far.Editor
	Assert-Far ($Data.Editor1.FileName -eq 'C:\TEMP\Test-EditorAsync1.txt')
}
job {
	# open editor 2
	Open-FarEditor C:\TEMP\Test-EditorAsync2.txt
	$Data.Editor2 = $Far.Editor
	Assert-Far ($Data.Editor2.FileName -eq 'C:\TEMP\Test-EditorAsync2.txt')
}
job {
	# write async to the editor 1
	$Data.Editor1.BeginAsync()
	$Data.Editor1.InsertText('Hello 1')
	$Data.Editor1.InsertChar('!')
	$Data.Editor1.InsertLine()
	$Data.Editor1.EndAsync()
}
# go to editor 1
macro 'Keys"F12 2"'
job {
	# test text 1
	$e = $Far.Editor
	Assert-Far @(
		$e -eq $Data.Editor1
		$e.Count -eq 2
		$e[0].Text -eq 'Hello 1!'
	)
}
job {
	# write async to the editor 2
	$Data.Editor2.BeginAsync()
	$Data.Editor2.InsertText('Hello 2')
	$Data.Editor2.InsertChar('!')
	$Data.Editor2.InsertLine()
	$Data.Editor2.EndAsync()
}
macro 'Keys"Esc n" -- exit editor 1, do not save'
job {
	# test text 2
	$e = $Far.Editor
	Assert-Far @(
		$e -eq $Data.Editor2
		$e.Count -eq 2
		$e[0].Text -eq 'Hello 2!'
	)
}
# exit editor 2, do not save
macro 'Keys"Esc n"'
job {
	Assert-Far $Far.Window.Count -eq 2
}
