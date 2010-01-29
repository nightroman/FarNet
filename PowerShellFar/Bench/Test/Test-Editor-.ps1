
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
if ($Editor.Id -ne $id) { throw }
$found = $false
foreach($e in $Far.Editors()) {
	if ($e.Id -eq $id) {
		$found = $true
		break
	}
}
if (!$found) { throw }

### Overtype
$Editor.Overtype = $true
$Editor.SetText('1234')
$Editor.GoTo(0, 0)
$Editor.Insert('56')
if ($Editor.GetText() -ne '5634') { throw }
$Editor.Overtype = $false
$Editor.Insert('78')
if ($Editor.GetText() -ne '567834') { throw }
$Editor.Overtype = $Overtype

### Fun with removing the last line
$Editor.SetText("1`r2`r")
if ($Lines.Count -ne 3) { throw }
$Lines.RemoveAt(2)
if ($Editor.GetText() -ne "1`r`n2") { throw }
$Lines.RemoveAt(1)
if ($Editor.GetText() -ne '1') { throw }
$Lines.RemoveAt(0)
if ($Editor.GetText() -ne '') { throw }

### Line list and string list
# clear 1: using $Lines (note: at least one line always exists)
$Lines.Clear()
if ($Editor.GetText() -ne '' -or $Strings.Count -ne 1 -or $Strings[0] -ne '') { throw }
# add lines when last line is empty (using $Lines and $Strings)
$Lines.Add('Строка1')
if ($Editor.GetText() -ne "Строка1`r`n") { throw }
$Strings.Add('Line2')
if ($Editor.GetText() -ne "Строка1`r`nLine2`r`n") { throw }
# get\set lines (using $Lines and $Strings)
$Lines[0].Text = $Lines[0].Text + '.'
$Strings[1] = $Strings[1] + '.'
$Strings[2] = 'End.'
if ($Editor.GetText() -ne "Строка1.`r`nLine2.`r`nEnd.") { throw }
# clear 2: using $Strings (note: at least one line always exists)
$Strings.Clear()
if ($Editor.GetText() -ne '' -or $Strings.Count -ne 1 -or $Strings[0] -ne '') { throw }
# add lines when last line is not empty (using $Lines and $Strings)
$Strings[0] = 'Строка1'
$Lines.Add('Line2')
if ($Editor.GetText() -ne "Строка1`r`nLine2") { throw }
$Strings.Add('Line3')
if ($Editor.GetText() -ne "Строка1`r`nLine2`r`nLine3") { throw }
# insert lines (using $Lines and $Strings)
$Lines.Insert(1, 'X')
if ($Editor.GetText() -ne "Строка1`r`nX`r`nLine2`r`nLine3") { throw }
$Strings.Insert(3, 'Y')
if ($Editor.GetText() -ne "Строка1`r`nX`r`nLine2`r`nY`r`nLine3") { throw }
# remove lines (using $Lines and $Strings)
$Strings.RemoveAt(3)
if ($Editor.GetText() -ne "Строка1`r`nX`r`nLine2`r`nLine3") { throw }
$Lines.RemoveAt(1)
if ($Editor.GetText() -ne "Строка1`r`nLine2`r`nLine3") { throw }
$null = $Lines.Remove($Lines[2])
if ($Editor.GetText() -ne "Строка1`r`nLine2") { throw }

### EndOfLine
if ($Lines[0].EndOfLine -ne "`r`n") { throw }
$Lines[0].EndOfLine = "`n"
if ($Lines[0].EndOfLine -ne "`n") { throw }

### Set all text (note preserved EOF states)
$Editor.SetText('')
if ($Editor.GetText() -ne '') { throw }
$Editor.SetText("1`r`n2`n3`r")
if ($Editor.GetText() -ne "1`r`n2`r`n3`r`n") { throw }
$Editor.SetText(".`r`n2`n3`rEOF")
if ($Editor.GetText() -ne ".`r`n2`r`n3`r`nEOF") { throw }

### Editor and cursor methods
$Editor.GoTo(0, 1)
$Editor.DeleteLine()
if ($Editor.GetText() -ne ".`r`n3`r`nEOF") { throw }
$Editor.DeleteChar()
if ($Editor.GetText() -ne ".`r`n`r`nEOF") { throw }
$Editor.DeleteChar()
if ($Editor.GetText() -ne ".`r`nEOF") { throw }
if (!$Overtype) {
	$Editor.Insert("Конец`r`nтеста`rTest-Editor`n")
	if ($Editor.GetText() -ne ".`r`nКонец`r`nтеста`r`nTest-Editor`r`nEOF") { throw }
}

### Selection operations
# rect selection
$Editor.SetText("1`nHELLO`nWORLD")
$Select.Select('Rect', 2, 1, 2, 2)
if ($Select.Type -ne 'Rect') { throw }
if ($Select.Count -ne 2) { throw }
if ($Select.GetText() -ne "L`r`nR") { throw }
if ($Select.Strings[0] -ne "L") { throw }
if ($Select.Strings[1] -ne "R") { throw }
$Select.Clear()
if ($Select.Exists) { throw }
if ($Editor.GetText() -ne "1`r`nHELO`r`nWOLD") { throw }
# lines vs. true lines
$Editor.SetText("1`nHELLO`nWORLD`n")
if ($Lines.Count -ne 4) { throw }
if ($Editor.TrueLines.Count -ne 3) { throw }
# selection vs. true selection
$Select.Select('Stream', 0, 0, -1, 2)
if ($Select.Count -ne 3) { throw }
if ($Editor.TrueSelection.Count -ne 2) { throw }
# stream selection (line parts)
$Editor.SetText("1`nHELLO`nWORLD")
$Select.Select('Stream', 3, 1, 1, 2)
if ($Select.Type -ne 'Stream') { throw }
if ($Select.Count -ne 2) { throw }
if ($Select.GetText() -ne "LO`r`nWO") { throw }
if ($Select.Strings[0] -ne "LO") { throw }
if ($Select.Strings[1] -ne "WO") { throw }
# insert inside
$Select.Insert(1, "новый")
if ($Select.GetText() -ne "LO`r`nновый`r`nWO") { throw }
# remove inside
$Select.RemoveAt(1)
if ($Select.GetText() -ne "LO`r`nWO") { throw }
# insert first, remove first (when first is completely selected)
$Editor.SetText("1`nHELLO`nWORLD")
$Select.Select('Stream', 0, 1, 1, 2)
$Select.Insert(0, "новый")
if ($Select.GetText() -ne "новый`r`nHELLO`r`nWO") { throw }
$Select.RemoveAt(0)
if ($Select.GetText() -ne "HELLO`r`nWO") { throw }
# insert first (when first is not completely selected)
$Editor.SetText("1`nHELLO`nWORLD")
$Select.Select('Stream', 3, 1, 1, 2)
$Select.Insert(0, "-")
if ($Select.GetText() -ne "-`r`nLO`r`nWO") { throw }
if ($Editor.GetText() -ne "1`r`nHEL-`r`nLO`r`nWORLD") { throw }
# remove partially selected first
$Select.RemoveAt(0)
if ($Select.GetText() -ne "LO`r`nWO") { throw }
if ($Editor.GetText() -ne "1`r`nHEL`r`nLO`r`nWORLD") { throw }
# remove partially selected last
$Select.RemoveAt(1)
if ($Select.GetText() -ne "LO") { throw }
if ($Editor.GetText() -ne "1`r`nHEL`r`nLO`r`nRLD") { throw }
# add to selection (case: empty last line)
$Editor.SetText("11`n22`n33")
$Select.Select('Stream', 0, 1, -1, 2)
if ($Select.GetText() -ne "22`r`n") { throw }
$Select.Add("44")
if ($Select.GetText() -ne "22`r`n44`r`n") { throw }
if ($Editor.GetText() -ne "11`r`n22`r`n44`r`n33") { throw }
# add to selection (case: not empty last line)
$Editor.SetText("11`n22`n33")
$Select.Select('Stream', 0, 1, 1, 1)
if ($Select.GetText() -ne "22") { throw }
$Select.Add("44")
if ($Select.GetText() -ne "22`r`n44") { throw }
if ($Editor.GetText() -ne "11`r`n22`r`n4433") { throw }
# remove one line selection
$Editor.SetText("11`n22`n33")
$Select.Select('Stream', 0, 1, 1, 1)
if ($Select.GetText() -ne "22") { throw }
$Select.RemoveAt(0)
if ($Select.Exists) { throw }
if ($Editor.GetText() -ne "11`r`n`r`n33") { throw }
# set items text
$Editor.SetText("ФФ`nЫЫ`nЙЙ")
$Select.Select('Stream', 1, 0, 0, 2)
if ($Select.GetText() -ne "Ф`r`nЫЫ`r`nЙ") { throw }
# via IStrings[i]
$Select.Strings[0] = "ШШ"
$Select.Strings[1] = ""
$Select.Strings[2] = "ЦЦ"
if ($Select.GetText() -ne "ШШ`r`n`r`nЦЦ") { throw }
if ($Editor.GetText() -ne "ФШШ`r`n`r`nЦЦЙ") { throw }
# via ILine.Text
$Select[0].Text = "Ш"
$Select[1].Text = "Ы"
$Select[2].Text = "Ц"
if ($Select.GetText() -ne "Ш`r`nЫ`r`nЦ") { throw }
if ($Editor.GetText() -ne "ФШ`r`nЫ`r`nЦЙ") { throw }
# test case: remove an empty line before ELL
$Editor.SetText("11`n`n22")
$Select.Select('Stream', 0, 0, -1, 2)
Remove-EmptyString- $Select
if ($Select.GetText() -ne "11") { throw }
if ($Editor.GetText() -ne "11`r`n22") { throw }

### Test editor scripts

# Escape\Unescape\ToUpper\ToLower selected
$text = @"
`t`r`n`r`n"йцу\кен"`t `r`n`r`n`r`n"!№;%:?*" `r`n
"@
$Editor.SetText($text)
$Select.Select('Stream', 0, 2, -1, 3)
# Escape
Set-Selection- -Replace '([\\"])', '\$1'
if ($Select.GetText() -ne @"
\"йцу\\кен\"`t `r`n
"@) { throw }
# Unescape
Set-Selection- -Replace '\\([\\"])', '$1'
if ($Editor.GetText() -ne $text) { throw }
# ToUpper
Set-Selection- -ToUpper
if ($Select.GetText() -ne @"
"ЙЦУ\КЕН"`t `r`n
"@) { throw }
# ToLower
Set-Selection- -ToLower
if ($Editor.GetText() -ne $text) { throw }

# remove end spaces from selected
$Select | Remove-EndSpace-
if ($Select.GetText() -ne @"
"йцу\кен"`r`n
"@) { throw }

# remove end spaces from all text
$Lines | Remove-EndSpace-
if ($Editor.GetText() -ne @"
`r`n`r`n"йцу\кен"`r`n`r`n`r`n"!№;%:?*"`r`n
"@) { throw }

# remove double empty lines from selected
$Select.Select('Stream', 0, 2, -1, 6)
Remove-EmptyString- $Select 2
if ($Select.GetText() -ne @"
"йцу\кен"`r`n`r`n"!№;%:?*"`r`n
"@) { throw }

# remove empty lines from all text
$Select.Unselect()
Remove-EmptyString- $Lines
if ($Editor.GetText() -ne @"
"йцу\кен"`r`n"!№;%:?*"
"@) { throw }

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
	if ($Editor.GetText() -ne @'
	{
	 2
	  3
	   }
'@) { throw }
	Indent-Selection- -Outdent
	if ($Editor.GetText() -ne @'
{
 2
  3
   }
'@) { throw }
	Indent-Selection- -Outdent
	if ($Editor.GetText() -ne @'
{
2
3
}
'@) { throw }
	Reindent-Selection-
	if ($Editor.GetText() -ne @'
{
	2
	3
}
'@) { throw }
}

### Go-Home-, Go-Selection-
$Editor.SetText("`t123")
$Editor.GoToPos(2)
Go-Home-
if ($Editor.Cursor.X -ne 1) { throw }
Go-Home-
if ($Editor.Cursor.X -ne 0) { throw }
Go-Home- -Select
if ($Select.GetText() -ne "`t") { throw }
$Select.Unselect()
$Editor.GoToPos(2)
Go-Home- -Select
if ($Select.GetText() -ne "1") { throw }
Go-Home- -Select
if ($Select.GetText() -ne "`t1") { throw }
if ($Editor.Cursor.X -ne 2 ) { throw }
Go-Selection-
if ($Editor.Cursor.X -ne 0 ) { throw }

### State, Save, Redraw, event OnRedraw, Title
if (!$Editor.IsModified) { throw }
$Editor.Title = "EDITOR TEST SUCCEEDED"
$Editor.SetText("EDITOR TEST SUCCEEDED") #! $Editor.Title issue
$Editor.Save()
if (!$Editor.IsModified -or !$Editor.IsSaved) { throw }
$Editor.add_OnRedraw({ Start-Sleep -m 25 })
for($Editor.GoTo(0, 0); $Editor.Cursor.X -lt 21; $Editor.GoToPos($Editor.Cursor.X + 1)) { $Editor.Redraw() }

### Close
#! don't check file is removed
$Editor.Close()

### Check logged events, remove log
#! don't check Closed event
$log = [string](Get-Content Test.log)
if ($log -ne "Editor:Opened Editor:Saving") { throw }
Remove-Item Test.log -ErrorAction 0

### Repeat the test with changed parameters
if (!$Overtype) {
	& $MyInvocation.MyCommand.Definition Test2.tmp -Overtype
}
