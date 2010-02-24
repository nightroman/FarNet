
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
# all editor lines string list
$Strings = $Lines.Strings
# selected lines
$Select = $Editor.Selection

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
$Editor.Insert('56')
Assert-Far ($Editor.GetText() -eq '5634')
$Editor.Overtype = $false
$Editor.Insert('78')
Assert-Far ($Editor.GetText() -eq '567834')
$Editor.Overtype = $Overtype

### Fun with removing the last line
$Editor.SetText("1`r2`r")
Assert-Far ($Lines.Count -eq 3)
$Lines.RemoveAt(2)
Assert-Far ($Editor.GetText() -eq "1`r`n2")
$Lines.RemoveAt(1)
Assert-Far ($Editor.GetText() -eq '1')
$Lines.RemoveAt(0)
Assert-Far ($Editor.GetText() -eq '')

### Line list and string list
# clear 1: using $Lines (note: at least one line always exists)
$Lines.Clear()
Assert-Far ($Editor.GetText() -eq '' -and $Strings.Count -eq 1 -and $Strings[0] -eq '')
# add lines when last line is empty (using $Lines and $Strings)
$Lines.Add('Строка1')
Assert-Far ($Editor.GetText() -eq "Строка1`r`n")
$Strings.Add('Line2')
Assert-Far ($Editor.GetText() -eq "Строка1`r`nLine2`r`n")
# get\set lines (using $Lines and $Strings)
$Lines[0].Text = $Lines[0].Text + '.'
$Strings[1] = $Strings[1] + '.'
$Strings[2] = 'End.'
Assert-Far ($Editor.GetText() -eq "Строка1.`r`nLine2.`r`nEnd.")
# clear 2: using $Strings (note: at least one line always exists)
$Strings.Clear()
Assert-Far ($Editor.GetText() -eq '' -and $Strings.Count -eq 1 -and $Strings[0] -eq '')
# add lines when last line is not empty (using $Lines and $Strings)
$Strings[0] = 'Строка1'
$Lines.Add('Line2')
Assert-Far ($Editor.GetText() -eq "Строка1`r`nLine2")
$Strings.Add('Line3')
Assert-Far ($Editor.GetText() -eq "Строка1`r`nLine2`r`nLine3")
# insert lines (using $Lines and $Strings)
$Lines.Insert(1, 'X')
Assert-Far ($Editor.GetText() -eq "Строка1`r`nX`r`nLine2`r`nLine3")
$Strings.Insert(3, 'Y')
Assert-Far ($Editor.GetText() -eq "Строка1`r`nX`r`nLine2`r`nY`r`nLine3")
# remove lines (using $Lines and $Strings)
$Strings.RemoveAt(3)
Assert-Far ($Editor.GetText() -eq "Строка1`r`nX`r`nLine2`r`nLine3")
$Lines.RemoveAt(1)
Assert-Far ($Editor.GetText() -eq "Строка1`r`nLine2`r`nLine3")
$null = $Lines.Remove($Lines[2])
Assert-Far ($Editor.GetText() -eq "Строка1`r`nLine2")

### EndOfLine
Assert-Far ($Lines[0].EndOfLine -eq "`r`n")
$Lines[0].EndOfLine = "`n"
Assert-Far ($Lines[0].EndOfLine -eq "`n")

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
	$Editor.Insert("Конец`r`nтеста`rTest-Editor`n")
	Assert-Far ($Editor.GetText() -eq ".`r`nКонец`r`nтеста`r`nTest-Editor`r`nEOF")
}

### Selection operations
# rect selection
$Editor.SetText("1`nHELLO`nWORLD")
$Select.Select('Rect', 2, 1, 2, 2)
Assert-Far ($Select.Kind -eq 'Rect')
Assert-Far ($Select.Count -eq 2)
Assert-Far ($Select.GetText() -eq "L`r`nR")
Assert-Far ($Select.Strings[0] -eq "L")
Assert-Far ($Select.Strings[1] -eq "R")
$Select.Clear()
Assert-Far (!$Select.Exists)
Assert-Far ($Editor.GetText() -eq "1`r`nHELO`r`nWOLD")
# lines vs. true lines
$Editor.SetText("1`nHELLO`nWORLD`n")
Assert-Far ($Lines.Count -eq 4)
Assert-Far ($Editor.TrueLines.Count -eq 3)
# selection vs. true selection
$Select.Select('Stream', 0, 0, -1, 2)
Assert-Far ($Select.Count -eq 3)
Assert-Far ($Editor.TrueSelection.Count -eq 2)
# stream selection (line parts)
$Editor.SetText("1`nHELLO`nWORLD")
$Select.Select('Stream', 3, 1, 1, 2)
Assert-Far ($Select.Kind -eq 'Stream')
Assert-Far ($Select.Count -eq 2)
Assert-Far ($Select.GetText() -eq "LO`r`nWO")
Assert-Far ($Select.Strings[0] -eq "LO")
Assert-Far ($Select.Strings[1] -eq "WO")
# insert inside
$Select.Insert(1, "новый")
Assert-Far ($Select.GetText() -eq "LO`r`nновый`r`nWO")
# remove inside
$Select.RemoveAt(1)
Assert-Far ($Select.GetText() -eq "LO`r`nWO")
# insert first, remove first (when first is completely selected)
$Editor.SetText("1`nHELLO`nWORLD")
$Select.Select('Stream', 0, 1, 1, 2)
$Select.Insert(0, "новый")
Assert-Far ($Select.GetText() -eq "новый`r`nHELLO`r`nWO")
$Select.RemoveAt(0)
Assert-Far ($Select.GetText() -eq "HELLO`r`nWO")
# insert first (when first is not completely selected)
$Editor.SetText("1`nHELLO`nWORLD")
$Select.Select('Stream', 3, 1, 1, 2)
$Select.Insert(0, "-")
Assert-Far ($Select.GetText() -eq "-`r`nLO`r`nWO")
Assert-Far ($Editor.GetText() -eq "1`r`nHEL-`r`nLO`r`nWORLD")
# remove partially selected first
$Select.RemoveAt(0)
Assert-Far ($Select.GetText() -eq "LO`r`nWO")
Assert-Far ($Editor.GetText() -eq "1`r`nHEL`r`nLO`r`nWORLD")
# remove partially selected last
$Select.RemoveAt(1)
Assert-Far ($Select.GetText() -eq "LO")
Assert-Far ($Editor.GetText() -eq "1`r`nHEL`r`nLO`r`nRLD")
# add to selection (case: empty last line)
$Editor.SetText("11`n22`n33")
$Select.Select('Stream', 0, 1, -1, 2)
Assert-Far ($Select.GetText() -eq "22`r`n")
$Select.Add("44")
Assert-Far ($Select.GetText() -eq "22`r`n44`r`n")
Assert-Far ($Editor.GetText() -eq "11`r`n22`r`n44`r`n33")
# add to selection (case: not empty last line)
$Editor.SetText("11`n22`n33")
$Select.Select('Stream', 0, 1, 1, 1)
Assert-Far ($Select.GetText() -eq "22")
$Select.Add("44")
Assert-Far ($Select.GetText() -eq "22`r`n44")
Assert-Far ($Editor.GetText() -eq "11`r`n22`r`n4433")
# remove one line selection
$Editor.SetText("11`n22`n33")
$Select.Select('Stream', 0, 1, 1, 1)
Assert-Far ($Select.GetText() -eq "22")
$Select.RemoveAt(0)
Assert-Far (!$Select.Exists)
Assert-Far ($Editor.GetText() -eq "11`r`n`r`n33")
# set items text
$Editor.SetText("ФФ`nЫЫ`nЙЙ")
$Select.Select('Stream', 1, 0, 0, 2)
Assert-Far ($Select.GetText() -eq "Ф`r`nЫЫ`r`nЙ")
# via IStrings[i]
$Select.Strings[0] = "ШШ"
$Select.Strings[1] = ""
$Select.Strings[2] = "ЦЦ"
Assert-Far ($Select.GetText() -eq "ШШ`r`n`r`nЦЦ")
Assert-Far ($Editor.GetText() -eq "ФШШ`r`n`r`nЦЦЙ")
# via ILine.Text
$Select[0].Text = "Ш"
$Select[1].Text = "Ы"
$Select[2].Text = "Ц"
Assert-Far ($Select.GetText() -eq "Ш`r`nЫ`r`nЦ")
Assert-Far ($Editor.GetText() -eq "ФШ`r`nЫ`r`nЦЙ")
# test case: remove an empty line before ELL
$Editor.SetText("11`n`n22")
$Select.Select('Stream', 0, 0, -1, 2)
Remove-EmptyString- $Select
Assert-Far ($Select.GetText() -eq "11")
Assert-Far ($Editor.GetText() -eq "11`r`n22")

### Test editor scripts

# Escape\Unescape\ToUpper\ToLower selected
$text = @"
`t`r`n`r`n"йцу\кен"`t `r`n`r`n`r`n"!№;%:?*" `r`n
"@
$Editor.SetText($text)
$Select.Select('Stream', 0, 2, -1, 3)
# Escape
Set-Selection- -Replace '([\\"])', '\$1'
Assert-Far ($Select.GetText() -eq @"
\"йцу\\кен\"`t `r`n
"@)
# Unescape
Set-Selection- -Replace '\\([\\"])', '$1'
Assert-Far ($Editor.GetText() -eq $text)
# ToUpper
Set-Selection- -ToUpper
Assert-Far ($Select.GetText() -eq @"
"ЙЦУ\КЕН"`t `r`n
"@)
# ToLower
Set-Selection- -ToLower
Assert-Far ($Editor.GetText() -eq $text)

# remove end spaces from selected
$Select | Remove-EndSpace-
Assert-Far ($Select.GetText() -eq @"
"йцу\кен"`r`n
"@)

# remove end spaces from all text
$Lines | Remove-EndSpace-
Assert-Far ($Editor.GetText() -eq @"
`r`n`r`n"йцу\кен"`r`n`r`n`r`n"!№;%:?*"`r`n
"@)

# remove double empty lines from selected
$Select.Select('Stream', 0, 2, -1, 6)
Remove-EmptyString- $Select 2
Assert-Far ($Select.GetText() -eq @"
"йцу\кен"`r`n`r`n"!№;%:?*"`r`n
"@)

# remove empty lines from all text
$Select.Unselect()
Remove-EmptyString- $Lines
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
	$Select.Select('Stream', 0, 0, 0, 3)
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
$Editor.GoToPos(2)
Go-Home-
Assert-Far ($Editor.Cursor.X -eq 1)
Go-Home-
Assert-Far ($Editor.Cursor.X -eq 0)
Go-Home- -Select
Assert-Far ($Select.GetText() -eq "`t")
$Select.Unselect()
$Editor.GoToPos(2)
Go-Home- -Select
Assert-Far ($Select.GetText() -eq "1")
Go-Home- -Select
Assert-Far ($Select.GetText() -eq "`t1")
Assert-Far ($Editor.Cursor.X -eq 2 )
Go-Selection-
Assert-Far ($Editor.Cursor.X -eq 0)

### State, Save, Redraw, event OnRedraw, Title
Assert-Far $Editor.IsModified
$Editor.Title = "EDITOR TEST SUCCEEDED"
$Editor.SetText("EDITOR TEST SUCCEEDED") #! $Editor.Title issue
$Editor.Save()
Assert-Far ($Editor.IsModified -and $Editor.IsSaved)
$Editor.add_OnRedraw({ Start-Sleep -m 25 })
for($Editor.GoTo(0, 0); $Editor.Cursor.X -lt 21; $Editor.GoToPos($Editor.Cursor.X + 1)) { $Editor.Redraw() }

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
