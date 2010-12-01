
<#
.SYNOPSIS
	Test editor and related scripts.
	Author: Roman Kuzmin

.DESCRIPTION
	It tests, demonstrates and covers almost all important functionality of
	FarNet editor. Thus, if you look at it and understand it then you know
 	almost all you need for editor scripting. Note that it also includes
 	technical code for testing FarNet and PowerShellFar.

	It uses and removes in the current folder:
	- Test1.tmp
	- Test2.tmp
	- Test.log
#>

param
(
	$Test = 'Test1.tmp',
	[switch]$Overtype
)

# setup
Set-Location (Get-Location -PSProvider FileSystem)
if (Test-Path $Test) { Remove-Item $Test }
$script:log = @()

### Create an editor
$Editor = New-FarEditor $Test -Title 'TEST' -DeleteSource 'File'

### Add editor events
$Editor.add_Opened({ $script:log += 'Editor:Opened' })
$Editor.add_Saving({ $script:log += 'Editor:Saving' })

### Open the editor
$Editor.Open()
$id = $Editor.Id
$Editor = $Far.Editor
Assert-Far ($Editor.Id -eq $id)

### Find Id in the editor list
$found = $false
foreach($e in $Far.Editors()) {
	if ($e.Id -eq $id) {
		$found = $true
		break
	}
}
Assert-Far $found

### Overtype: test and set to the given
$Editor.Overtype = $true
$Editor.SetText('1234')
$Editor.GoTo(0, 0)
$Editor.InsertText('56')
Assert-Far ($Editor.GetText() -eq '5634')
$Editor.Overtype = $false
$Editor.InsertText('78')
Assert-Far ($Editor.GetText() -eq '567834')
$Editor.Overtype = $Overtype

### Fun with removing the last line
$Editor.SetText("1`r2`r")
Assert-Far ($Editor.Count -eq 3)
$Editor.RemoveAt(2)
Assert-Far ($Editor.GetText() -eq "1`r`n2")
$Editor.RemoveAt(1)
Assert-Far ($Editor.GetText() -eq '1')
$Editor.RemoveAt(0)
Assert-Far ($Editor.GetText() -eq '')

### Line list and string list
# clear 1: note: at least one line always exists
$Editor.Clear()
Assert-Far ($Editor.GetText() -eq '' -and $Editor.Count -eq 1 -and $Editor[0].Text -eq '')
# add lines when last line is empty
$Editor.Add('Строка1')
Assert-Far ($Editor.GetText() -eq "Строка1`r`n")
$Editor.Add('Line2')
Assert-Far ($Editor.GetText() -eq "Строка1`r`nLine2`r`n")
# get\set lines
$Editor[0].Text += '.'
$Editor[1].Text += '.'
$Editor[2].Text = 'End.'
Assert-Far ($Editor.GetText() -eq "Строка1.`r`nLine2.`r`nEnd.")
# add lines when last line is not empty
$Editor.Clear()
$Editor[0].Text = 'Строка1'
$Editor.Add('Line2')
Assert-Far ($Editor.GetText() -eq "Строка1`r`nLine2")
$Editor.Add('Line3')
Assert-Far ($Editor.GetText() -eq "Строка1`r`nLine2`r`nLine3")
# insert lines
$Editor.Insert(1, 'X')
Assert-Far ($Editor.GetText() -eq "Строка1`r`nX`r`nLine2`r`nLine3")
$Editor.Insert(3, 'Y')
Assert-Far ($Editor.GetText() -eq "Строка1`r`nX`r`nLine2`r`nY`r`nLine3")
# remove lines
$Editor.RemoveAt(3)
Assert-Far ($Editor.GetText() -eq "Строка1`r`nX`r`nLine2`r`nLine3")
$Editor.RemoveAt(1)
Assert-Far ($Editor.GetText() -eq "Строка1`r`nLine2`r`nLine3")
$Editor.RemoveAt(2)
Assert-Far ($Editor.GetText() -eq "Строка1`r`nLine2")

### Set all text (note preserved EOF states)
$Editor.SetText('')
Assert-Far ($Editor.GetText() -eq '')
$Editor.SetText("1`r`n2`n3`r")
Assert-Far ($Editor.GetText() -eq "1`r`n2`r`n3`r`n")
$Editor.SetText(".`r`n2`n3`rEOF")
Assert-Far ($Editor.GetText() -eq ".`r`n2`r`n3`r`nEOF")

### Editor and cursor methods
$Editor.GoTo(0, 1)
$Editor.DeleteLine()
Assert-Far ($Editor.GetText() -eq ".`r`n3`r`nEOF")
$Editor.DeleteChar()
Assert-Far ($Editor.GetText() -eq ".`r`n`r`nEOF")
$Editor.DeleteChar()
Assert-Far ($Editor.GetText() -eq ".`r`nEOF")
if (!$Overtype) {
	$Editor.InsertText("Конец`r`nтеста`rTest-Editor`n")
	Assert-Far ($Editor.GetText() -eq ".`r`nКонец`r`nтеста`r`nTest-Editor`r`nEOF")
}

### Column selection

$Editor.SetText(@'
1
HELLO
WORLD
'@)
$Editor.SelectText(2, 1, 2, 2, 'Column')
Assert-Far @(
	$Editor.SelectionKind -eq 'Column'
	$Editor.GetSelectedText("`n") -eq "L`nR"
)
$Select = $Editor.SelectedLines
Assert-Far ($Select.Count -eq 2)
Assert-Far @(
	$Select[0].Text -eq 'HELLO'
	$Select[0].ActiveText -eq 'L'
	$Select[0].SelectedText -eq 'L'
	$Select[1].Text -eq 'WORLD'
	$Select[1].ActiveText -eq 'R'
	$Select[1].SelectedText -eq 'R'
)
$Editor.DeleteText()
Assert-Far @(
	!$Editor.SelectionExists
	$Editor.GetText("`n") -eq "1`nHELO`nWOLD"
)

### State, Save, Redraw, Redrawing, Title
Assert-Far ($Editor.IsModified -and !$Editor.IsSaved)
$Editor.Title = "EDITOR TEST SUCCEEDED"
$Editor.SetText("EDITOR TEST SUCCEEDED") #! $Editor.Title issue
$Editor.Save($true)
Assert-Far ($Editor.IsModified -and $Editor.IsSaved)
$Editor.add_Redrawing({ Start-Sleep -m 25 })
for($Editor.GoTo(0, 0); $Editor.Caret.X -lt 21; $Editor.GoToColumn($Editor.Caret.X + 1)) { $Editor.Redraw() }

### Close
#! don't check file is removed
$Editor.Close()

### Check logged events
#! don't check Closed event
$logged = $script:log -join "`r`n"
Assert-Far ($log -eq @'
Editor:Opened
Editor:Saving
'@)

### Repeat the test with changed parameters
if (!$Overtype) {
	& $MyInvocation.MyCommand.Definition Test2.tmp -Overtype
}
