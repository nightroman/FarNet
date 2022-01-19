<#
.Synopsis
	Tests Open-TextLink.ps1 and Get-TextLink.ps1

.Notes
	Mind header lines.
	Mind tests order.

*** TEST DATA
Text1
Text2
F# error
***
#>

$Data.File = "$PSScriptRoot\Test-TextLink.fas.ps1"

job {
	# VS line link
	Open-TextLink.ps1 "noise $($Data.File)(10) noise"
}
job {
	Assert-Far (Get-TextLink.ps1) -eq "$($Data.File)(10): Text1"
}
job {
	# test with $env:TextLinkEnv
	$old = $env:TextLinkEnv
	$env:TextLinkEnv = 'Missing, FarNetCode'
	#! with try/finally fails, why?
	$r = Get-TextLink.ps1
	$env:TextLinkEnv = $old
	Assert-Far $r -eq "%FarNetCode%\Test\Utility\Test-TextLink.fas.ps1(10): Text1"
}
keys Esc
job {
	# VS line link with %var%
	Open-TextLink.ps1 "noise %FarNetCode%\Test\Utility\Test-TextLink.fas.ps1(11) noise"
}
job {
	Assert-Far (Get-TextLink.ps1) -eq "$($Data.File)(11): Text2"
}
keys Esc
job {
	# VS line-char link
	Open-TextLink.ps1 "noise $($Data.File)(9,9) noise"
}
job {
	Assert-Far -Editor
	$Caret = $Far.Editor.Caret
	Assert-Far ($Caret.X -eq 8 -and $Caret.Y -eq 8)
}
keys Esc
job {
	# VS text link, nearest text is after the target
	Open-TextLink.ps1 "noise $($Data.File)(9): Text2"
}
job {
	Assert-Far (Get-TextLink.ps1) -eq "$($Data.File)(11): Text2"
}
keys Esc
job {
	# PS line link
	Open-TextLink.ps1 "noise $($Data.File):10"
}
job {
	Assert-Far (Get-TextLink.ps1) -eq "$($Data.File)(10): Text1"
}
keys Esc
job {
	# PS line link with %var%
	Open-TextLink.ps1 "noise %FarNetCode%\Test\Utility\Test-TextLink.fas.ps1:11"
}
job {
	Assert-Far (Get-TextLink.ps1) -eq "$($Data.File)(11): Text2"
}
keys Esc
job {
	# PS line-char link
	Open-TextLink.ps1 "noise $($Data.File):9 char:9"
}
job {
	Assert-Far -Editor
	$Caret = $Far.Editor.Caret
	Assert-Far ($Caret.X -eq 8 -and $Caret.Y -eq 8)
}
keys Esc
job {
	# VS text link, nearest text is before the target
	Open-TextLink.ps1 "noise $($Data.File)(9): Text2"
}
job {
	Assert-Far (Get-TextLink.ps1) -eq "$($Data.File)(11): Text2"
}
keys Esc
### Select-String links

job {
	# ^
	Open-TextLink.ps1 "$($Data.File):10: Text1"
}
job {
	Assert-Far (Get-TextLink.ps1) -eq "$($Data.File)(10): Text1"
}
keys Esc
job {
	# ^\s+
	Open-TextLink.ps1 "  $($Data.File):11: Text2"
}
job {
	Assert-Far (Get-TextLink.ps1) -eq "$($Data.File)(11): Text2"
}
keys Esc
job {
	# ^>\s+, wrong line, to search
	Open-TextLink.ps1 "> $($Data.File):2: Text1"
}
job {
	Assert-Far (Get-TextLink.ps1) -eq "$($Data.File)(10): Text1"
}
keys Esc
### F# exception messages

job {
	$text = '   at <StartupCode$FSI_0009>.$FSI_0009_Z.2.invalidOp.far$fsx.main@() in {0}:line {1}' -f $Data.File, 12
	Open-TextLink.ps1 $text
}
job {
	Assert-Far (Get-TextLink.ps1) -eq "$($Data.File)(12): F# error"
}
keys Esc
### Modal mode with text link search

keys CtrlG
job {
	Assert-Far -Dialog
}
run {
	# VS text link, nearest text is before the target
	Open-TextLink.ps1 "noise $($Data.File)(9): Text2"
}
job {
	Assert-Far (Get-TextLink.ps1) -eq "$($Data.File)(11): Text2"
}
# exit editor
keys Esc
job {
	Assert-Far -Dialog
}
# exit dialog
keys Esc
