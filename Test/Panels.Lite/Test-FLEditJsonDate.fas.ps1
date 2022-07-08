<#
.Synopsis
	Test the command for editing a JSON line.
#>

job {
	Import-Module FarLite
	$Data.FileName = "$env:TEMP\z.LiteDB"
	if (Test-Path $Data.FileName) {Remove-Item $Data.FileName}

	Import-Module Ldbc
	Use-LiteDatabase $Data.FileName {
		$test = Get-LiteCollection test
		[ordered]@{_id = 1; line1 = [datetime]'2020-02-17 06:10:00'; end=''} | Add-LiteData $test
	}

	# open collection documents
	Open-LitePanel $Data.FileName test
}

job {
	Find-FarFile 1
}
macro 'Keys"F4" -- edit document'
job {
	# go to line to test
	Assert-Far -Editor
	$Far.Editor.GoToLine(2)
}
job {
	# line with date and comma
	Assert-Far $Far.Editor.Line.Text -eq '  "line1": {"$date": "2020-02-17T06:10:00.0000000Z"},'
}
macro 'Keys"F4" -- edit line'
job {
	# another editor with simpler date
	Assert-Far $Far.Editor.Line.Text -eq '2020-02-17 06:10:00'
	# change
	$Far.Editor.Line.Text = '2020-01-01'
}
macro 'Keys"Esc Enter" -- exit, save'
job {
	# edited
	Assert-Far $Far.Editor.Line.Text -eq '  "line1": {"$date": "2020-01-01T00:00:00.0000000Z"},'
}
macro 'Keys"Esc n" -- exit, no save'
job {
	Assert-Far -Plugin
}
keys Esc
