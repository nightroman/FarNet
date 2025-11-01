<#
.Synopsis
	Test the command for editing a JSON line.
#>

if ([System.Environment]::OSVersion.Version.Major -lt 10) {return}

job {
	$null = Start-Mongo
	Import-Module FarMongo

	Import-Module Mdbc
	Connect-Mdbc -NewCollection
	$Data.Collection = $Collection
	[ordered]@{_id = 1; line1 = "value1"; line2 = "value2"} | Add-MdbcData

	# open collection documents
	Open-MongoPanel . test test
}

job {
	Find-FarFile 1
}
macro 'Keys"F4 Down Down" -- edit document, go to "line1"'
job {
	# line with comma
	Assert-Far $__.Line.Text -eq '  "line1" : "value1",'
}
macro 'Keys"F4" -- edit line'
job {
	# another editor with value1
	Assert-Far $__.Line.Text -eq "value1"
}
macro 'Keys"CtrlEnd Enter f o o Esc Enter" -- edit and exit'
job {
	# edited
	Assert-Far $__.Line.Text -eq '  "line1" : "value1\r\nfoo",'
}
macro 'Keys"Down" -- go to next line'
job {
	# line without comma
	Assert-Far $__.Line.Text -eq '  "line2" : "value2"'
}
macro 'Keys"F4" -- edit line'
job {
	# another editor with value2
	Assert-Far $__.Line.Text -eq "value2"
}
macro 'Keys"CtrlEnd Enter f o o Esc Enter" -- edit and exit'
job {
	# edited
	Assert-Far $__.Line.Text -eq '  "line2" : "value2\r\nfoo"'
}
macro 'Keys"Esc Enter" -- exit editor, save'
job {
	Import-Module Mdbc
	$r = Get-MdbcData -Collection $Data.Collection
	Assert-Far -Panels
	Assert-Far $(
		$r.line1 -eq "value1`r`nfoo"
		$r.line2 -eq "value2`r`nfoo"
	)
}

### test in create (F7)
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
	Assert-Far $__.Line.Text -eq '  "p1" : "new value 2"'
}
macro 'Keys"Esc n" -- exit, no save'

### end
macro 'Keys"Esc" -- exit panel'
