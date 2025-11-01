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
		[ordered]@{_id = 1; line1 = "value1"; line2 = "value2"} | Add-LiteData $test
	}

	# open collection documents
	Open-LitePanel $Data.FileName test
}

job {
	Find-FarFile 1
}
macro 'Keys"F4 Down Down" -- edit document, go to "line1"'
job {
	# line with comma
	Assert-Far $__.Line.Text -eq '  "line1": "value1",'
}
macro 'Keys"F4" -- edit line'
job {
	# another editor with value1
	Assert-Far $__.Line.Text -eq "value1"
}
macro 'Keys"CtrlEnd Enter f o o Esc Enter" -- edit and exit'
job {
	# edited
	Assert-Far $__.Line.Text -eq '  "line1": "value1\r\nfoo",'
}
macro 'Keys"Down" -- go to next line'
job {
	# line without comma
	Assert-Far $__.Line.Text -eq '  "line2": "value2"'
}
macro 'Keys"F4" -- edit line'
job {
	# another editor with value2
	Assert-Far $__.Line.Text -eq "value2"
}
macro 'Keys"CtrlEnd Enter f o o Esc Enter" -- edit and exit'
job {
	# edited
	Assert-Far $__.Line.Text -eq '  "line2": "value2\r\nfoo"'
}
macro 'Keys"Esc Enter" -- exit editor, save'
job {
	Import-Module Ldbc
	$r = Use-LiteDatabase $Data.FileName {
		$test = Get-LiteCollection test
		Get-LiteData $test
	}
	Assert-Far -Panels
	Assert-Far $(
		$r.line1 -eq "value1`r`nfoo"
		$r.line2 -eq "value2`r`nfoo"
	)
}

### test in create (F7)
#!(1) Also covers why we call EditorControl_ECTL_GETSTRING for EOL to use it on setting strings.
keys F7
job {
	Assert-Far -Editor
	Assert-Far $__.GetText() -eq ''
	$__.SetText(@'
{
  p1: "new value 1"
}
'@
	)
	$__.GoToLine(1)
}
macro 'Keys"F4" -- edit line'
job {
	# another editor with text
	Assert-Far $__.Line.Text -eq "new value 1"
	# change it
	$__.Line.Text = "new value 2"
}
macro 'Keys"Esc Enter" -- exit, save'
job {
	# changed
	#!(1) because it's the last line, without EOL (using null/default), result would be `"p1": "new value 2\r\n"`
	Assert-Far $__.Line.Text -eq '  "p1": "new value 2"'
}
macro 'Keys"Esc n" -- exit, no save'
macro 'Keys"Esc" -- exit panel'
