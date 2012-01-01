
<#
.Synopsis
	Test dialog controls.
	Author: Roman Kuzmin

.Description
	Just run it and press [F1] for description.
#>

param
(
	[int]$X = 2,
	[int]$Y = 2
)
Set-StrictMode -Version 2
$myFolder = Split-Path ($MyInvocation.MyCommand.Definition)

### Create a dialog
$dialog = $Far.CreateDialog($X, $Y, $X + 78, $Y + 19)
$dialog.HelpTopic = '<' + (Split-Path $MyInvocation.MyCommand.Path) + '\>TestDialog'

### User control
$uc = $dialog.AddUserControl(0, 0, 78, 19)
$uc.NoFocus = $true
# MouseClicked handler (emulates IDialog.MouseClicked)
$uc.add_MouseClicked({ Show-FarMessage "UserControl.MouseClicked: $($_.Mouse)" })
# Drawing; &{} is used to make variables local and avoid conflicts
$uc.add_Drawing({&{
	# absolute dialog rectangle
	$r1 = $this.Rect
	# relative control rectangle
	$r2 = $_.Control.Rect
	# coordinates for bottom text
	$x = $r1.Left + 2
	$y = $r1.Top + $r2.Bottom
	# write blue text on 'DialogBox' background
	$Far.UI.DrawColor($x + 4, $y, 'Blue', $Far.UI.GetPaletteBackground('DialogBox'), 'User control: for custom draw, for clicks on "dialog area", and etc.')
}})

### Box (double line)
$b1 = $dialog.AddBox(3, 1, 0, 0, 'Double Box')

### Text
$t1 = $dialog.AddText(5, -1, 0, '&Text')

### Edit (standard)
$e1 = $dialog.AddEdit(5, -1, 70, '')
$e1.History = 'PowerShellFarPrompt'
$e1.add_Coloring({ $_.Background1 = $_.Background3 = $Far.UI.GetPaletteBackground('PanelText') })

### Some disabled items
$dialog.AddText(5, -1, 0, 'Disabled Text').Disabled = $true
$ed = $dialog.AddEdit(5, -1, 70, 'Disabled Edit')
$ed.Disabled = $true

### Text (separator, double line)
$dialog.AddText(5, -1, 0, 'Separator2').Separator = 2

### CheckBox (standard)
$x1 = $dialog.AddCheckBox(5, -1, 'Check&Box')
$x1.Selected = 1

### CheckBox (three state)
$x2 = $dialog.AddCheckBox(19, 0, 'T&hreeState')
$x2.ThreeState = $true
$x2.Selected = 2

### Edit (fixed)
$e2 = $dialog.AddEditFixed(53, 0, 60, 'FixEdit')
$e2.Mask = 'AAAAAAA'

### Edit (password)
$e3 = $dialog.AddEditPassword(63, 0, 70, 'волга')

### Box (single line)
$bs = $dialog.AddBox(5, -1, 70, 2, '&Single Box')
$bs.Single = $true
$bs.add_Coloring({ $_.Foreground3 = 'Red' })

### RadioButton
$r1 = $dialog.AddRadioButton(7, -1, 'RadioButton&1')
$r1.Group = $true
$r2 = $dialog.AddRadioButton(25, 0, 'RadioButton&2')
$r2.Selected = $true

### Text (separator, single line)
$_ = $dialog.AddText(5, -2, 0, 'Separator1')
$_.BoxColor = $true
$_.Separator = 1

### ComboBox (edit)
$null = $dialog.AddText(5, -1, 34, 'ComboBox (Edit)')
$ce = $dialog.AddComboBox(5, -1, 34, 'Value')
$ce.Add('Value1').Checked = $true
$ce.Add('Value2').Disabled = $true
$ce.Add('').IsSeparator = $true
$null = $ce.Add('Value3')

### ComboBox (list)
$null = $dialog.AddText(5, -1, 34, 'ComboBox (List)')
$cl = $dialog.AddComboBox(5, -1, 34, '')
$cl.DropDownList = $true
$cl.Selected = 1
$null = $cl.Add('Понедельник')
$null = $cl.Add('Вторник')
$null = $cl.Add('Среда')

### ListBox
$lb = $dialog.AddListBox(37, $ce.Rect.Top - 1, 70, 16, 'ListBox.Title')
[Enum]::GetValues([ConsoleColor]) | .{process{ $null = $lb.Add($_) }}
$lb.Bottom = 'ListBox.Bottom'
$lb.NoClose = $true
$lb.Selected = 1

### Vertical texts (separators and standard)
$dialog.AddVerticalText(72, 2, 20, '').Separator = 1
$dialog.AddVerticalText(73, 2, 20, '').Separator = 2
$dialog.AddVerticalText(74, 2, 16, 'Vertical Text').Centered = $true

### Buttons
$done = $dialog.AddButton(0, $cl.Rect.Top + 2, 'Done')
$done.CenterGroup = $true
$fail = $dialog.AddButton(0, 0, '&Fail')
$fail.CenterGroup = $true
$test = $dialog.AddButton(0, 0, 'T&est')
$test.CenterGroup = $true
$list = $dialog.AddButton(0, 0, 'L&ist')
$list.CenterGroup = $true
$more = $dialog.AddButton(0, 0, 'More')
$more.CenterGroup = $true

### Special 'Cancel' button
$dialog.Cancel = $dialog.AddButton(0, 0, 'E&xit')
$dialog.Cancel.CenterGroup = $true

### Set default and focused items
$dialog.Default = $done
$dialog.Focused = $test

### Add some demo handlers
$log = @()

### Initialized: the dialog is already created but not yet shown
$dialog.add_Initialized({
	$e1.Text = '<Edit>'
})

### Idled: how to use custom frequency (show time in the console title every 2 seconds)
$dialog.add_Idled([FarNet.IdledHandler]::Create(2, {
	$Host.UI.RawUI.WindowTitle = [datetime]::Now
}))

### MouseClicked: how to get not processed mouse event and out-of-dialog mouse events
$dialog.add_MouseClicked({
	$log += "[Dialog: MouseClicked: $($_.Mouse)]"
	$ed.Text = $_.Mouse
	if ($_.Control -eq $null) {
		$_.Ignore = $true
		Show-FarMessage "Clicked outside of the dialog"
	}
})

### KeyPressed: how to get not processed by controls key events
$dialog.add_KeyPressed({
	$log += "[Dialog: KeyPressed: {0:x}]" -f $_.Code
	if ($_.Code -eq [FarNet.KeyCode]::F1) {
		Show-FarMessage @'
We catch F1 and do not set $_.Ignore to $true,
=> default action (help) will be still called.
'@
	}
})

### Closing: use ($_.Ignore = $true) to cancel closing
$dialog.add_Closing({
	$log += "[Closing: Selected = $($_.Control)]"
})

### GotFocus: edit box 1 has got focus
$e1.add_GotFocus({
	$log += "[Edit: GotFocus]"
})

### LosingFocus: edit box 1 is losing focus
$e1.add_LosingFocus({
	$log += "[Edit: LosingFocus]"
})

### TextChanged: edit box 1 text is changed
$e1.add_TextChanged({
	$log += "[Edit: TextChanged: Text = $($_.Text)]"
})

### ButtonClicked events for checkboxes and radiobuttons
$x1.add_ButtonClicked({
	$log += "[CheckBox: ButtonClicked: Selected = $($_.Selected)]"
})
$x2.add_ButtonClicked({
	$log += "[ThreeState: ButtonClicked: Selected = $($_.Selected)]"
})
$r1.add_ButtonClicked({
	$log += "[RadioButton1: ButtonClicked: Selected = $($_.Selected)]"
})
$r2.add_ButtonClicked({
	$log += "[RadioButton2: ButtonClicked: Selected = $($_.Selected)]"
})

### [Done] Adds log (and the dialog is closed)
$done.add_ButtonClicked({
	$log += "[Done: ButtonClicked]"
})

### [Fail] Just to test what happens on error
$fail.add_ButtonClicked({
	1 / $null
})

### [More] Run another dialog (also shows how to use $MyInvocation)
$more.add_ButtonClicked({
	& $MyInvocation.MyCommand.Definition ($X + 4) ($Y + 4)
	$_.Ignore = $true
})

### [Test] Test the dialog by stepper
$test.add_ButtonClicked({

	# don't close the dialog
	$_.Ignore = $true

	# create a stepper
	$stepper = New-Object PowerShellFar.Stepper
	$stepper.Ask = $true
	$stepper.Go((& "$myFolder\Test-Dialog+.ps1" -TestOpened))
})

### [List] Test list controls
function TestList($box, $fast)
{
	# disable redraw for better performance in 'slow' mode
	$dialog.DisableRedraw()

	# performance: use DetachItems ... AttachItems for large changes
	if ($fast) { $box.DetachItems() }

	# remove all items
	$box.Items.Clear()

	# create and add new items with some random properties
	$num = 1000
	$rnd = New-Object Random
	for($1 = 0; $1 -lt $num; ++$1) {
		$box.Items.Add((
			New-FarItem -Text $1 -Checked:($rnd.Next(2)) -Disabled:($rnd.Next(2))
		))
	}

	# remove some items randomly
	for($1 = $num; --$1 -ge 0;) {
		if ($rnd.Next(2)) {
			$box.Items.RemoveAt($1)
		}
	}

	# you should call AttachItems after DetachItems when changes are done
	if ($fast) { $box.AttachItems() }

	# set a random current item
	$box.Selected = $rnd.Next($num)

	# do not forget this!
	$dialog.EnableRedraw()
}

$list.add_ButtonClicked({

	# don't close the dialog
	$_.Ignore = $true

	# fast or slow?
	$fast = $lb.Title.StartsWith('Fast')
	$fast = !$fast

	# change list title
	if ($fast) { $lb.Title = 'Fast...' } else { $lb.Title = 'Slow...' }
	$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

	# test comboedit, combolist, listbox
	# Bug [_090208_040000] comboedit and combolist crash in 'slow' mode, Far problem
	TestList $ce $true # use $fast when Bug [_090208_040000] is fixed
	TestList $cl $true # use $fast when Bug [_090208_040000] is fixed
	TestList $lb $fast

	# change list title
	if ($fast) { $lb.Title = 'Fast ' } else { $lb.Title = 'Slow ' }
	$lb.Title += $stopwatch.ElapsedMilliseconds
	$lb.Bottom = 'WS {0:n}' -f ((Get-Process -Id $PID).WorkingSet / 1kb)
})

# Show the dialog, return on escape
if (!$dialog.Show()) {
	return
}

# Results: output control data and event log
@"
------ Dialog results ------
Edit1        : Text = '$($e1.Text)'
Edit2        : Text = '$($e2.Text)'
Edit3        : Text = '$($e3.Text)'
CheckBox     : Selected = $($x1.Selected)
ThreeState   : Selected = $($x2.Selected)
RadioButton1 : Selected = $($r1.Selected)
RadioButton2 : Selected = $($r2.Selected)
Combo(Edit)  : Selected = $($ce.Selected) Text = '$($ce.Text)'"
Combo(List)  : Selected = $($cl.Selected) Text = '$($cl.Text)'"
ListBox      : Selected = $($lb.Selected)
Selected     : $($dialog.Selected.Text)
-------------
Events:
"@
$log
