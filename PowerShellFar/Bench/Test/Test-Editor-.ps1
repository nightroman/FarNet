
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
if (Test-Path 'Test.log') { Remove-Item 'Test.log' }

### Create an editor
$Editor = New-FarEditor $Test -Title 'TEST' -DeleteSource 'File'

### Add editor events
$Editor.add_Opened({ 'Editor:Opened' >> Test.log })
$Editor.add_Saving({ 'Editor:Saving' >> Test.log })

### Open the editor
$Editor.Open()

# Id tested later
$id = $Editor.Id

### Useful objects
# current editor
$Editor = $Far.Editor
# all editor lines
$Lines = $Editor.Lines
# selected lines
$Parts = $Editor.SelectedLines

### Check Id and editor collection
Assert-Far ($Editor.Id -eq $id)
$found = $false
foreach($e in $Far.Editors()) {
	if ($e.Id -eq $id) {
		$found = $true
		break
	}
}
Assert-Far $found

### Overtype
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
$Editor.AddText('Строка1')
Assert-Far ($Editor.GetText() -eq "Строка1`r`n")
$Editor.AddText('Line2')
Assert-Far ($Editor.GetText() -eq "Строка1`r`nLine2`r`n")
# get\set lines
$Editor[0].Text += '.'
$Editor[1].Text += '.'
$Editor[2].Text = 'End.'
Assert-Far ($Editor.GetText() -eq "Строка1.`r`nLine2.`r`nEnd.")
# add lines when last line is not empty
$Editor.Clear()
$Editor[0].Text = 'Строка1'
$Editor.AddText('Line2')
Assert-Far ($Editor.GetText() -eq "Строка1`r`nLine2")
$Editor.AddText('Line3')
Assert-Far ($Editor.GetText() -eq "Строка1`r`nLine2`r`nLine3")
# insert lines
$Editor.InsertText(1, 'X')
Assert-Far ($Editor.GetText() -eq "Строка1`r`nX`r`nLine2`r`nLine3")
$Editor.InsertText(3, 'Y')
Assert-Far ($Editor.GetText() -eq "Строка1`r`nX`r`nLine2`r`nY`r`nLine3")
# remove lines
$Editor.RemoveAt(3)
Assert-Far ($Editor.GetText() -eq "Строка1`r`nX`r`nLine2`r`nLine3")
$Editor.RemoveAt(1)
Assert-Far ($Editor.GetText() -eq "Строка1`r`nLine2`r`nLine3")
$Editor.RemoveAt(2)
Assert-Far ($Editor.GetText() -eq "Строка1`r`nLine2")

### EndOfLine
Assert-Far ($Editor[0].EndOfLine -eq "`r`n")
$Editor[0].EndOfLine = "`n"
Assert-Far ($Editor[0].EndOfLine -eq "`n")

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

### Selection operations
# rect selection
$Editor.SetText("1`nHELLO`nWORLD")
$Editor.SelectText('Rect', 2, 1, 2, 2)
Assert-Far ($Editor.SelectionKind -eq 'Rect')
Assert-Far ($Parts.Count -eq 2)
Assert-Far ($Editor.GetSelectedText() -eq "L`r`nR")
Assert-Far ($Parts[0].Text -eq "L")
Assert-Far ($Parts[1].Text -eq "R")
$Editor.DeleteText()
Assert-Far (!$Editor.SelectionExists)
Assert-Far ($Editor.GetText() -eq "1`r`nHELO`r`nWOLD")
# lines vs. true lines
$Editor.SetText("1`nHELLO`nWORLD`n")
Assert-Far ($Editor.Count -eq 4)
# selection vs. true selection
$Editor.SelectText('Stream', 0, 0, -1, 2)
Assert-Far ($Parts.Count -eq 3)
# stream selection (line parts)
$Editor.SetText("1`nHELLO`nWORLD")
$Editor.SelectText('Stream', 3, 1, 1, 2)
Assert-Far ($Editor.SelectionKind -eq 'Stream')
Assert-Far ($Parts.Count -eq 2)
Assert-Far ($Editor.GetSelectedText() -eq "LO`r`nWO")
Assert-Far ($Parts[0].Text -eq "LO")
Assert-Far ($Parts[1].Text -eq "WO")
# insert inside
$Parts.InsertText(1, "новый")
Assert-Far ($Editor.GetSelectedText() -eq "LO`r`nновый`r`nWO")
# remove inside
$Parts.RemoveAt(1)
Assert-Far ($Editor.GetSelectedText() -eq "LO`r`nWO")
# insert first, remove first (when first is completely selected)
$Editor.SetText("1`nHELLO`nWORLD")
$Editor.SelectText('Stream', 0, 1, 1, 2)
$Parts.InsertText(0, "новый")
Assert-Far ($Editor.GetSelectedText() -eq "новый`r`nHELLO`r`nWO")
$Parts.RemoveAt(0)
Assert-Far ($Editor.GetSelectedText() -eq "HELLO`r`nWO")
# insert first (when first is not completely selected)
$Editor.SetText("1`nHELLO`nWORLD")
$Editor.SelectText('Stream', 3, 1, 1, 2)
$Parts.InsertText(0, "-")
Assert-Far ($Editor.GetSelectedText() -eq "-`r`nLO`r`nWO")
Assert-Far ($Editor.GetText() -eq "1`r`nHEL-`r`nLO`r`nWORLD")
# remove partially selected first
$Parts.RemoveAt(0)
Assert-Far ($Editor.GetSelectedText() -eq "LO`r`nWO")
Assert-Far ($Editor.GetText() -eq "1`r`nHEL`r`nLO`r`nWORLD")
# remove partially selected last
$Parts.RemoveAt(1)
Assert-Far ($Editor.GetSelectedText() -eq "LO")
Assert-Far ($Editor.GetText() -eq "1`r`nHEL`r`nLO`r`nRLD")
# add to selection (case: empty last line)
$Editor.SetText("11`n22`n33")
$Editor.SelectText('Stream', 0, 1, -1, 2)
Assert-Far ($Editor.GetSelectedText() -eq "22`r`n")
$Parts.AddText("44")
Assert-Far ($Editor.GetSelectedText() -eq "22`r`n44`r`n")
Assert-Far ($Editor.GetText() -eq "11`r`n22`r`n44`r`n33")
# add to selection (case: not empty last line)
$Editor.SetText("11`n22`n33")
$Editor.SelectText('Stream', 0, 1, 1, 1)
Assert-Far ($Editor.GetSelectedText() -eq "22")
$Parts.AddText("44")
Assert-Far ($Editor.GetSelectedText() -eq "22`r`n44")
Assert-Far ($Editor.GetText() -eq "11`r`n22`r`n4433")
# remove one line selection
$Editor.SetText("11`n22`n33")
$Editor.SelectText('Stream', 0, 1, 1, 1)
Assert-Far ($Editor.GetSelectedText() -eq "22")
$Parts.RemoveAt(0)
Assert-Far (!$Editor.SelectionExists)
Assert-Far ($Editor.GetText() -eq "11`r`n`r`n33")
# set items text
$Editor.SetText("ФФ`nЫЫ`nЙЙ")
$Editor.SelectText('Stream', 1, 0, 0, 2)
Assert-Far ($Editor.GetSelectedText() -eq "Ф`r`nЫЫ`r`nЙ")
$Parts[0].Text = "ШШ"
$Parts[1].Text = ""
$Parts[2].Text = "ЦЦ"
Assert-Far ($Editor.GetSelectedText() -eq "ШШ`r`n`r`nЦЦ")
Assert-Far ($Editor.GetText() -eq "ФШШ`r`n`r`nЦЦЙ")
$Parts[0].Text = "Ш"
$Parts[1].Text = "Ы"
$Parts[2].Text = "Ц"
Assert-Far ($Editor.GetSelectedText() -eq "Ш`r`nЫ`r`nЦ")
Assert-Far ($Editor.GetText() -eq "ФШ`r`nЫ`r`nЦЙ")
# test case: remove an empty line before ELL
$Editor.SetText("11`n`n22")
$Editor.SelectText('Stream', 0, 0, -1, 2)
Remove-EmptyString- $Parts
Assert-Far ($Editor.GetSelectedText() -eq "11")
Assert-Far ($Editor.GetText() -eq "11`r`n22")

### Test editor scripts

# Escape\Unescape\ToUpper\ToLower selected
$text = @"
`t`r`n`r`n"йцу\кен"`t `r`n`r`n`r`n"!№;%:?*" `r`n
"@
$Editor.SetText($text)
$Editor.SelectText('Stream', 0, 2, -1, 3)
# Escape
Set-Selection- -Replace '([\\"])', '\$1'
Assert-Far ($Editor.GetSelectedText() -eq @"
\"йцу\\кен\"`t `r`n
"@)
# Unescape
Set-Selection- -Replace '\\([\\"])', '$1'
Assert-Far ($Editor.GetText() -eq $text)
# ToUpper
Set-Selection- -ToUpper
Assert-Far ($Editor.GetSelectedText() -eq @"
"ЙЦУ\КЕН"`t `r`n
"@)
# ToLower
Set-Selection- -ToLower
Assert-Far ($Editor.GetText() -eq $text)

# remove end spaces from selected
$Editor.SelectedLines | Remove-EndSpace-
Assert-Far ($Editor.GetSelectedText() -eq @"
"йцу\кен"`r`n
"@)

# remove end spaces from all text
$Editor.Lines | Remove-EndSpace-
Assert-Far ($Editor.GetText() -eq @"
`r`n`r`n"йцу\кен"`r`n`r`n`r`n"!№;%:?*"`r`n
"@)

# remove double empty lines from selected
$Editor.SelectText('Stream', 0, 2, -1, 6)
Remove-EmptyString- $Parts 2 #?????
Assert-Far ($Editor.GetSelectedText() -eq @"
"йцу\кен"`r`n`r`n"!№;%:?*"`r`n
"@)

# remove empty lines from all text
$Editor.UnselectText()
Remove-EmptyString- $Lines #?????
Assert-Far ($Editor.GetText() -eq @"
"йцу\кен"`r`n"!№;%:?*"
"@)

### Indent, outdent, reindent
if ($Editor.ExpandTabs -eq 'None' -and $Editor.TabSize -eq 4) {
	$Editor.SetText(@'
{
 2
  3
   }
'@)
	$Editor.SelectText('Stream', 0, 0, 0, 3)
	Indent-Selection-
	Assert-Far ($Editor.GetText() -eq @'
	{
	 2
	  3
	   }
'@)
	Indent-Selection- -Outdent
	Assert-Far ($Editor.GetText() -eq @'
{
 2
  3
   }
'@)
	Indent-Selection- -Outdent
	Assert-Far ($Editor.GetText() -eq @'
{
2
3
}
'@)
	Reindent-Selection-
	Assert-Far ($Editor.GetText() -eq @'
{
	2
	3
}
'@)
}

### Go-Home-, Go-Selection-
$Editor.SetText("`t123")
$Editor.GoToColumn(2)
Go-Home-
Assert-Far ($Editor.Caret.X -eq 1)
Go-Home-
Assert-Far ($Editor.Caret.X -eq 0)
Go-Home- -Select
Assert-Far ($Editor.GetSelectedText() -eq "`t")
$Editor.UnselectText()
$Editor.GoToColumn(2)
Go-Home- -Select
Assert-Far ($Editor.GetSelectedText() -eq "1")
Go-Home- -Select
Assert-Far ($Editor.GetSelectedText() -eq "`t1")
Assert-Far ($Editor.Caret.X -eq 2 )
Go-Selection-
Assert-Far ($Editor.Caret.X -eq 0)

### State, Save, Redraw, event OnRedraw, Title
Assert-Far $Editor.IsModified
$Editor.Title = "EDITOR TEST SUCCEEDED"
$Editor.SetText("EDITOR TEST SUCCEEDED") #! $Editor.Title issue
$Editor.Save()
Assert-Far ($Editor.IsModified -and $Editor.IsSaved)
$Editor.add_OnRedraw({ Start-Sleep -m 25 })
for($Editor.GoTo(0, 0); $Editor.Caret.X -lt 21; $Editor.GoToColumn($Editor.Caret.X + 1)) { $Editor.Redraw() }

### Close
#! don't check file is removed
$Editor.Close()

### Check logged events, remove log
#! don't check Closed event
$log = [string](Get-Content Test.log)
Assert-Far ($log -eq "Editor:Opened Editor:Saving")
Remove-Item Test.log -ErrorAction 0

### Repeat the test with changed parameters
if (!$Overtype) {
	& $MyInvocation.MyCommand.Definition Test2.tmp -Overtype
}
