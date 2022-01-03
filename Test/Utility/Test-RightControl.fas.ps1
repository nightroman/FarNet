<#
.Synopsis
	Test FarNet.RightControl
#>

job {
	# RightControl helpers
	$global:RightControl = $Far.GetModuleAction('1b42c03e-40c4-45db-a3ce-eb0825fe16d1')
	function global:Invoke-RightControl($command) {
		$global:RightControl.Invoke($null, ([FarNet.ModuleCommandEventArgs]$command))
	}
	function global:StepL { Invoke-RightControl 'step-left' }
	function global:StepR { Invoke-RightControl 'step-right' }
	function global:SelectL { Invoke-RightControl 'select-left' }
	function global:SelectR { Invoke-RightControl 'select-right' }
	function global:DeleteL { Invoke-RightControl 'delete-left' }
	function global:DeleteR { Invoke-RightControl 'delete-right' }
	function global:SelectLA { Invoke-RightControl 'vertical-left' }
	function global:SelectRA { Invoke-RightControl 'vertical-right' }
	function global:GoToSmartHome { Invoke-RightControl 'go-to-smart-home' }
	function global:SelectToSmartHome { Invoke-RightControl 'select-to-smart-home' }
}

job {
	Open-FarEditor 'Test-RightControl..ps1.tmp'
	$global:Editor = $Far.Editor
	$global:Line = $Editor.Line
	Assert-Far -EditorFileName *\Test-RightControl..ps1.tmp
}

### From line home to text home
job {
	#                1   2
	$Editor.SetText("    456789")
	$Editor.GoToColumn(0)
	Assert-Far $Editor.Caret.X -eq 0
}
job {
	# Home ~ 4
	GoToSmartHome
	Assert-Far @(
		!$Editor.SelectionExists
		$Editor.Caret.X -eq 4
	)
}
job {
	# Home ~ 0
	GoToSmartHome
	Assert-Far @(
		!$Editor.SelectionExists
		$Editor.Caret.X -eq 0
	)
}
job {
	# ShiftHome ~ 4
	SelectToSmartHome
	$span = $Editor.Line.SelectionSpan
	Assert-Far @(
		$Editor.SelectionExists
		$Editor.Caret.X -eq 4
		$span.Start -eq 0
		$span.End -eq 4
	)
}
job {
	# ShiftHome ~ 0
	SelectToSmartHome
	Assert-Far @(
		!$Editor.SelectionExists
		$Editor.Caret.X -eq 0
	)
}

### From text home to line home
job {
	#                2   1
	$Editor.SetText("    456789")
	$Editor.GoToColumn(4)
	Assert-Far $Editor.Caret.X -eq 4
}
job {
	# Home ~ 0
	GoToSmartHome
	Assert-Far @(
		!$Editor.SelectionExists
		$Editor.Caret.X -eq 0
	)
}
job {
	# Home ~ 4
	GoToSmartHome
	Assert-Far @(
		!$Editor.SelectionExists
		$Editor.Caret.X -eq 4
	)
}
job {
	# ShiftHome ~ 0
	SelectToSmartHome
	$span = $Editor.Line.SelectionSpan
	Assert-Far @(
		$Editor.SelectionExists
		$Editor.Caret.X -eq 0
		$span.Start -eq 0
		$span.End -eq 4
	)
}
job {
	# ShiftHome ~ 4
	SelectToSmartHome
	Assert-Far @(
		!$Editor.SelectionExists
		$Editor.Caret.X -eq 4
	)
}

### Go from text body to homes
job {
	#                3   2   1
	$Editor.SetText("    456789")
	$Editor.GoToColumn(8)
	Assert-Far $Editor.Caret.X -eq 8
}
job {
	# Home ~ 4
	GoToSmartHome
	Assert-Far @(
		!$Editor.SelectionExists
		$Editor.Caret.X -eq 4
	)
}
job {
	# Home ~ 0
	GoToSmartHome
	Assert-Far @(
		!$Editor.SelectionExists
		$Editor.Caret.X -eq 0
	)
}

### Select from text body homes
job {
	#                3   2   1
	$Editor.SetText("    456789")
	$Editor.GoToColumn(8)
	Assert-Far $Editor.Caret.X -eq 8
}
job {
	# ShiftHome ~ 4
	SelectToSmartHome
	$span = $Editor.Line.SelectionSpan
	Assert-Far @(
		$Editor.SelectionExists
		$Editor.Caret.X -eq 4
		$span.Start -eq 4
		$span.End -eq 8
	)
}
job {
	# ShiftHome ~ 0
	SelectToSmartHome
	$span = $Editor.Line.SelectionSpan
	Assert-Far @(
		$Editor.SelectionExists
		$Editor.Caret.X -eq 0
		$span.Start -eq 0
		$span.End -eq 8
	)
}
job {
	# ShiftHome ~ 4
	SelectToSmartHome
	$span = $Editor.Line.SelectionSpan
	Assert-Far @(
		$Editor.SelectionExists
		$Editor.Caret.X -eq 4
		$span.Start -eq 4
		$span.End -eq 8
	)
}

### Select from white
job {
	#                3 1 2
	$Editor.SetText("    456789")
	$Editor.GoToColumn(2)
	Assert-Far $Editor.Caret.X -eq 2
}
job {
	# ShiftHome -> select to right
	SelectToSmartHome
	$span = $Editor.Line.SelectionSpan
	Assert-Far @(
		$Editor.SelectionExists
		$Editor.Caret.X -eq 4
		$span.Start -eq 2
		$span.End -eq 4
	)
}
job {
	# ShiftHome -> flip to left
	SelectToSmartHome
	$span = $Editor.Line.SelectionSpan
	Assert-Far @(
		$Editor.SelectionExists
		$Editor.Caret.X -eq 0
		$span.Start -eq 0
		$span.End -eq 2
	)
}
job {
	# ShiftHome -> flip to right
	SelectToSmartHome
	$span = $Editor.Line.SelectionSpan
	Assert-Far @(
		$Editor.SelectionExists
		$Editor.Caret.X -eq 4
		$span.Start -eq 2
		$span.End -eq 4
	)
}

### Expand white selection to right
job {
	#                 []
	$Editor.SetText("    456789")
	$Editor.Line.SelectText(1, 2)
	$Editor.GoToColumn(2)
	$span = $Editor.Line.SelectionSpan
	Assert-Far @(
		$Editor.Caret.X -eq 2
		$span.Start -eq 1
		$span.End -eq 2
	)
}
job {
	# ShiftHome -> flip to right
	SelectToSmartHome
	$span = $Editor.Line.SelectionSpan
	Assert-Far @(
		$Editor.SelectionExists
		$Editor.Caret.X -eq 4
		$span.Start -eq 1
		$span.End -eq 4
	)
}

### Flip white selection to right
job {
	#                 []
	$Editor.SetText("    456789")
	$Editor.Line.SelectText(1, 2)
	$Editor.GoToColumn(1)
	$span = $Editor.Line.SelectionSpan
	Assert-Far @(
		$Editor.Caret.X -eq 1
		$span.Start -eq 1
		$span.End -eq 2
	)
}
job {
	# ShiftHome -> flip to right
	SelectToSmartHome
	$span = $Editor.Line.SelectionSpan
	Assert-Far @(
		$Editor.SelectionExists
		$Editor.Caret.X -eq 4
		$span.Start -eq 2
		$span.End -eq 4
	)
}

### Step right/left, select right/left
job {
	#               | ||  | | | | | |
	#                0123456789012345
	$Editor.SetText(' $xx = yy::zz( ')
	$Editor.GoToColumn(0)
}
job {
	StepR; Assert-Far $Editor.Caret.X -eq 1
	StepR; Assert-Far $Editor.Caret.X -eq 2
	StepR; Assert-Far $Editor.Caret.X -eq 5
	StepR; Assert-Far $Editor.Caret.X -eq 7
	StepR; Assert-Far $Editor.Caret.X -eq 9
	StepR; Assert-Far $Editor.Caret.X -eq 11
	StepR; Assert-Far $Editor.Caret.X -eq 13
	StepR; Assert-Far $Editor.Caret.X -eq 15
}
job {
	StepL; Assert-Far $Editor.Caret.X -eq 13
	StepL; Assert-Far $Editor.Caret.X -eq 11
	StepL; Assert-Far $Editor.Caret.X -eq 9
	StepL; Assert-Far $Editor.Caret.X -eq 7
	StepL; Assert-Far $Editor.Caret.X -eq 5
	StepL; Assert-Far $Editor.Caret.X -eq 2
	StepL; Assert-Far $Editor.Caret.X -eq 1
	StepL; Assert-Far $Editor.Caret.X -eq 0
}
job {
	# increase selection from left to right
	SelectR; Assert-Far ($Editor.Caret.X -eq 1 -and $Line.SelectionSpan.Length -eq 1)
	SelectR; Assert-Far ($Editor.Caret.X -eq 2 -and $Line.SelectionSpan.Length -eq 2)
	SelectR; Assert-Far ($Editor.Caret.X -eq 5 -and $Line.SelectionSpan.Length -eq 5)
	SelectR; Assert-Far ($Editor.Caret.X -eq 7 -and $Line.SelectionSpan.Length -eq 7)
	SelectR; Assert-Far ($Editor.Caret.X -eq 9 -and $Line.SelectionSpan.Length -eq 9)
	SelectR; Assert-Far ($Editor.Caret.X -eq 11 -and $Line.SelectionSpan.Length -eq 11)
	SelectR; Assert-Far ($Editor.Caret.X -eq 13 -and $Line.SelectionSpan.Length -eq 13)
	SelectR; Assert-Far ($Editor.Caret.X -eq 15 -and $Line.SelectionSpan.Length -eq 15)
}
job {
	# decrease selection from right to left
	SelectL; Assert-Far ($Editor.Caret.X -eq 13 -and $Line.SelectionSpan.Length -eq 13)
	SelectL; Assert-Far ($Editor.Caret.X -eq 11 -and $Line.SelectionSpan.Length -eq 11)
	SelectL; Assert-Far ($Editor.Caret.X -eq 9 -and $Line.SelectionSpan.Length -eq 9)
	SelectL; Assert-Far ($Editor.Caret.X -eq 7 -and $Line.SelectionSpan.Length -eq 7)
	SelectL; Assert-Far ($Editor.Caret.X -eq 5 -and $Line.SelectionSpan.Length -eq 5)
	SelectL; Assert-Far ($Editor.Caret.X -eq 2 -and $Line.SelectionSpan.Length -eq 2)
	SelectL; Assert-Far ($Editor.Caret.X -eq 1 -and $Line.SelectionSpan.Length -eq 1)
	SelectL; Assert-Far ($Editor.Caret.X -eq 0 -and $Line.SelectionSpan.Length -lt 0)
}

### Flip selection from the middle of a word
job {
	$Line.Text = '01-3456-89'
	$Line.Caret = 5

	SelectR; Assert-Far $Line.SelectedText -eq '56'
	SelectL; Assert-Far $Line.SelectedText -eq '34'
	SelectR; Assert-Far $Line.SelectedText -eq '56'
}

### Delete right/left
job {
	$Editor.SetText('012(456)789')
	$Editor.GoToColumn(5)
}
job {
	DeleteR
	Assert-Far @(
		$Line.Text -eq '012(4)789'
		$Line.Caret -eq 5
	)
}
job {
	DeleteL
	Assert-Far @(
		$Line.Text -eq '012()789'
		$Line.Caret -eq 4
	)
}
job {
	DeleteR
	Assert-Far @(
		$Line.Text -eq '012(789'
		$Line.Caret -eq 4
	)
}
job {
	DeleteL
	Assert-Far @(
		$Line.Text -eq '012789'
		$Line.Caret -eq 3
	)
}
job {
	DeleteR
	Assert-Far @(
		$Line.Text -eq '012'
		$Line.Caret -eq 3
	)
}
job {
	DeleteL
	Assert-Far @(
		$Line.Text -eq ''
		$Line.Caret -eq 0
	)
}

### Vertical
job {
	$Editor.SetText(@'
0123456789 - qwerty
    45678901234

'@)
	### select from top to bottom
	$Editor.SelectText(6, 0, 5, 1, 'Column')
	$Editor.GoTo(6, 1)
}
job {
	$text =@'
6789 - qw
678901234
'@

	# 1st
	SelectRA
	Assert-Far @(
		$Line.Caret -eq 15
		$Editor.GetSelectedText() -eq $text
	)

	# 2nd ~ nothing to change
	SelectRA
	Assert-Far @(
		$Line.Caret -eq 15
		$Editor.GetSelectedText() -eq $text
	)
}
job {
	SelectLA
	Assert-Far @(
		$Line.Caret -eq 4
		$Editor.GetSelectedText() -eq @'
45
45
'@)

	SelectLA
	Assert-Far @(
		$Line.Caret -eq 0
		$Editor.GetSelectedText() -eq @'
012345
    45
'@)
}
job {
	### select from bottom to top
	$Editor.SelectText(6, 0, 5, 1, 'Column')
	$Editor.GoTo(6, 0)
}
job {
	SelectRA
	SelectRA
	SelectRA

	# _101210_192119: short lines are padded by spaces, not garbage
	Assert-Far @(
		$Line.Caret -eq 19
		$Editor.GetSelectedText() -eq "6789 - qwerty`r`n678901234    "
	)
}

# exit
macro 'Keys"Esc n"'
job {
	Assert-Far ($Far.Window.Kind -ne 'Editor')
	Remove-Variable -Scope global Editor, Line, RightControl
	Push-Location function:
	Remove-Item Invoke-RightControl, StepR, StepL, SelectR, SelectL, DeleteR, DeleteL, SelectRA, SelectLA, GoToSmartHome, SelectToSmartHome
	Pop-Location
}
