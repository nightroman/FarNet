<#
.Synopsis
	Searches files and panel found results.
	Author: Roman Kuzmin

.Description
	See help (run this and [F1]) for more information about input, dialog
	controls and result panel keys.

.Parameter Regex
		Regular expression [string] or [regex]. If it is omitted then a
		dialog is opened where you can define this and other parameters.

		Regex capturing groups are treated as separate found matches.

		With AllText editor selection is not used and groups are ignored.

.Parameter Options
		Regular expression options and extra options, when Regex is [string].
		See SearchRegexOptions here for available values.

		The option Singleline/s implies AllText.

.Parameter InputObject
		Strings (file paths) and FileInfo (from Get-*Item). If it is not
		defined then items come from the pipeline. If there are no items
		and Regex is not set then an input command is specified in the
		dialog.

		Built-in helpers:
		* Get-EditorHistory - files from history excluding network paths

.Parameter AllText
		Tells to read and process files as whole strings, not lines.
		Found matches are not selected in the editor.

		The option Singleline/s implies AllText.
#>

[CmdletBinding()]
param(
	$Regex
	,
	$Options
	,
	[Parameter(ValueFromPipeline=1)]
	$InputObject
	,
	[switch]$AllText
)

#requires -Version 7.4
begin {
	$ErrorActionPreference = 1; if ($Host.Name -ne 'FarHost') {Write-Error 'Requires FarHost.'}
	$Items = [System.Collections.Generic.List[object]]::new()
}
process {
	$Items.Add($InputObject)
}
end { try {

[Flags()]
enum SearchRegexOptions {
	None = 0
	IgnoreCase = 1; ic = 1
	Multiline = 2; m = 2
	ExplicitCapture = 4; ec = 4
	Compiled = 8
	Singleline = 16; s = 16
	IgnorePatternWhitespace = 32; ipw = 32
	RightToLeft = 64
	ECMAScript = 256
	CultureInvariant = 512
	SimpleMatch = 1024; sm = 1024
	WholeWord = 2048; ww = 2048
}

function Get-EditorHistory {
	$array = $Far.History.Editor()
	[System.Array]::Reverse($array)
	$array | .{process{ if (!$_.Name.StartsWith('\\')) {$_.Name} }}
}

function whole_word_regex($_) {
	"(?(?=\w)\b)$_(?(?<=\w)\b)"
}

### Regex dialog
if (!$Regex) {
	[int]$w1 = $Far.UI.WindowSize.X * 0.8
	[int]$w2 = $w1 - 6
	$dialog = $Far.CreateDialog(-1, -1, $w1, 9)
	$dialog.TypeId = 'DA462DD5-7767-471E-9FC8-64A227BEE2B1'
	$dialog.HelpTopic = "<$($Psf.AppHome)\\>search-regexps1"
	[void]$dialog.AddBox(3, 1, 0, 0, 'Search-Regex')
	$x = 13

	[void]$dialog.AddText(5, -1, 0, '&Pattern')
	$eRegex = $dialog.AddEdit($x, 0, $w2, '')
	$eRegex.History = 'SearchText'
	$eRegex.UseLastHistory = $true

	[void]$dialog.AddText(5, -1, 0, '&Options')
	$eOptions = $dialog.AddEdit($x, 0, $w2, $Options)
	$eOptions.History = 'RegexOptions'
	$eOptions.UseLastHistory = $true

	[void]$dialog.AddText(5, -1, 0, '&Input')
	$eInput = $dialog.AddEdit($x, 0, $w2, '')
	if ($Items) {
		$eInput.Disabled = $true
		$eInput.Text = "$($Items.Count) items"
	}
	else {
		$eInput.History = 'PowerShellItems'
		$eInput.UseLastHistory = $true
	}

	$xAllText = $dialog.AddCheckBox($x, -1, '&All text')
	$xAllText.Selected = [FarNet.User]::Data['Search-Regex.AllText'] ?? [bool]$AllText

	$dialog.Default = $dialog.AddButton(0, -1, 'Ok')
	$dialog.Default.CenterGroup = $true

	$dialog.Cancel = $dialog.AddButton(0, 0, 'Cancel')
	$dialog.Cancel.CenterGroup = $true

	### Dialog loop
	for() {
		if (!$dialog.Show()) {
			return
		}

		# options
		if ($eOptions.Text) {
			try { $Options = [SearchRegexOptions]($eOptions.Text.Trim() -split '\W+' -join ',') }
			catch {
				Show-FarMessage $_ 'Invalid options'
				$dialog.Focused = $eOptions
				continue
			}
		}
		else {
			$Options = [SearchRegexOptions]::None
		}

		# pattern after options
		$pattern = $eRegex.Text
		if (!$pattern) {
			Show-FarMessage 'Pattern must not be empty.' 'Invalid pattern'
			$dialog.Focused = $eRegex
			continue
		}
		if ([int]$Options -band [SearchRegexOptions]::SimpleMatch) {
			$pattern = [regex]::Escape($pattern)
		}
		if ([int]$Options -band [SearchRegexOptions]::WholeWord) {
			$pattern = whole_word_regex $pattern
		}

		# regex after options and pattern
		try {
			$RegexOptions = [Text.RegularExpressions.RegexOptions](([int]$Options) -band (-bnot (1024 + 2048)))
			$Regex = [regex]::new($pattern, $RegexOptions)
		}
		catch {
			Show-FarMessage $_ 'Invalid pattern'
			$dialog.Focused = $eRegex
			continue
		}

		# save
		[FarNet.User]::Data['Search-Regex.AllText'] = $xAllText.Selected

		# ready input
		if ($Items) {
			break
		}

		try {
			# parse input
			$text = $eInput.Text.Trim()
			if (!$text) {
				$text = 'Get-ChildItem -File -Force'
			}
			elseif ($text.StartsWith('*')) {
				$text = "Get-ChildItem . -File -Force -Recurse -Include $text"
			}
			$sb = [scriptblock]::Create($text)

			# invoke input
			$Host.UI.RawUI.WindowTitle = 'Collecting input...'
			$Items = & $sb
			if ($Items) {
				break
			}

			# no input
			$dialog.Focused = $eInput
			Show-FarMessage 'There are no input items.' 'Input'
		}
		catch {
			Show-FarMessage $_ 'Invalid Input'
			$dialog.Focused = $eInput
			$Items = $null
		}
	}

	# other options
	$AllText = [bool]$xAllText.Selected
}

### Validate input and set job data
if ($Regex -isnot [regex]) {
	$Regex = "$Regex"
	$Options = $Options ? [SearchRegexOptions]$Options : 0
	if ([int]$Options -band [SearchRegexOptions]::SimpleMatch) {
		$Regex = [regex]::Escape($Regex)
	}
	if ([int]$Options -band [SearchRegexOptions]::WholeWord) {
		$Regex = whole_word_regex $Regex
	}
	$RegexOptions = [Text.RegularExpressions.RegexOptions](([int]$Options) -band (-bnot (1024 + 2048)))
	$Regex = [regex]::new($Regex, $RegexOptions)
}
if (!$Items) {
	throw "There is no input."
}
if ($Regex.Options -band [System.Text.RegularExpressions.RegexOptions]::Singleline) {
	$AllText = $true
}

### Explorer
$Explorer = [PowerShellFar.PowerExplorer]::new('7ef0bbec-9509-4223-a452-ea928ac9846c')
$Explorer.Data = @{
	Output = [System.Management.Automation.PSDataCollection[FarNet.SetFile]]::new()
	Done = $false
	CountItems = 0
	State = 'Running'
}
$Explorer.AsGetFiles = {
	param($Explorer)
	foreach($file in $Explorer.Data.Output.ReadAll()) {
		$Explorer.Cache.Add($file)
	}
}

### Panel
$Panel = [FarNet.Panel]::new($Explorer)
$Panel.Highlighting = 'Full'
$Panel.RealNames = $true
$Panel.RightAligned = $true
$Panel.SortMode = 'Unsorted'
$Panel.Title = 'Searching...'
$Panel.ViewMode = 'LongDescriptions'

### Plan
# 'Descriptions'
$col1 = [FarNet.SetColumn]@{ Kind = 'NR'; Name = 'File' }
$col2 = [FarNet.SetColumn]@{ Kind = 'Z'; Name = 'Match' }
$plan = [FarNet.PanelPlan]@{ Columns = $col1, $col2; StatusColumns = $col2 }
$Panel.SetPlan('Descriptions', $plan)
$Panel.SetPlan(0, $plan)
# 'LongDescriptions'
$plan = $plan.Clone()
$plan.IsFullScreen = $true
$Panel.SetPlan('LongDescriptions', $plan)
$Panel.SetPlan(9, $plan)

### Timer: check new data and update
$Panel.TimerInterval = 2000
$Panel.add_Timer({
	$ExplorerData = $this.Explorer.Data
	if ($ExplorerData.Done) {
		return
	}

	if ($ExplorerData.Output.Count) {
		$this.Update($false)
	}

	$title = '{0}: {1} lines in {2} files' -f $ExplorerData.State, $this.Explorer.Cache.Count, $ExplorerData.CountItems
	if ($this.Title -ne $title) {
		$this.Title = $title
		$this.Redraw()
	}

	if ($ExplorerData.State -ne 'Running') {
		$ExplorerData.Done = $true
		$Far.UI.SetProgressFlash()
		$Far.UI.WindowTitle = $title
	}
})

### KeyPressed
$Panel.add_KeyPressed({
	### [Enter] opens an editor at the selected match
	if ($_.Key.Is([FarNet.KeyCode]::Enter)) {
		$file = $this.CurrentFile
		if (!$file -or $file.Description -notmatch '^\s*(\d+):') {
			return
		}
		$_.Ignore = $true
		$editor = New-FarEditor $file.Name ($Matches[1]) -DisableHistory
		$frame = $editor.Frame
		$editor.Open()
		$index, $length = $file.Data
		$frame.CaretColumn = $index + $length
		$frame.VisibleLine = $frame.CaretLine - $Host.UI.RawUI.WindowSize.Height / 3
		$editor.Frame = $frame
		if ($length) {
			# null if a file is already opened
			if ($line = $editor.Line) {
				$line.SelectText($index, $frame.CaretColumn)
			}
		}
		$editor.Redraw()
		return
	}
	### [F1] opens Search-Regex help topic
	if ($_.Key.Is([FarNet.KeyCode]::F1)) {
		$Far.ShowHelp($Psf.AppHome, 'search-regexps1', 'Path')
		$_.Ignore = $true
		return
	}
})

### Escaping
$Panel.add_Escaping({
	$_.Ignore = $true

	# close empty
	if (!$this.Explorer.Cache.Count) {
		$this.CloseChild()
		return
	}

	# ask not empty
	switch(Show-FarMessage "How would you like to continue?" -Caption $this.Title -Choices '&Close', '&Push', 'Cancel') {
		0 {
			$this.CloseChild()
		}
		1 {
			$this.Push()
		}
	}
})

### Closed
$Panel.add_Closed({
	# let the task know
	$this.Explorer.Data.Done = $true
})

### Start search task
Start-FarTask -Panel $Panel -Items $Items -Regex $Regex -AllText $AllText {
	param($Panel, $Items, $Regex, $AllText)

	$ExplorerData = $Panel.Explorer.Data

	job {
		[FarNet.Tasks]::OpenPanel({ $Data.Panel.OpenChild($null) })
	}

	### Process input items
	foreach($item in $Items) {
		if ($ExplorerData.Done) {
			return
		}

		if ($item -isnot [System.IO.FileInfo]) {
			$item = Get-Item -LiteralPath $item -Force -ErrorAction Ignore
			if ($item -isnot [System.IO.FileInfo]) {
				continue
			}
		}

		++$ExplorerData.CountItems

		if ($AllText) {
			$text = try {[System.IO.File]::ReadAllText($item.FullName)} catch {continue}
			if ($text -match $Regex) {
				for($m = $Regex.Match($text); $m.Success; $m = $m.NextMatch()) {
					$s = $text.Substring(0, $m.Index)
					$n = [regex]::Count($s, '\n')
					$null = $s -match '\n?([^\n]*)$'
					$file = [FarNet.SetFile]::new($item, $true)
					$file.Data = @($Matches[1].Length, 0)
					$file.Description = '{0,4:d}: {1}' -f ($n + 1), $m.Value
					$ExplorerData.Output.Add($file)
				}
			}
			continue
		}

		$no = 0
		$lines = try {[System.IO.File]::ReadAllLines($item.FullName)} catch {continue}
		foreach($line in $lines) {
			++$no
			if ($line -match $Regex) {
				for($m = $Regex.Match($line); $m.Success; $m = $m.NextMatch()) {
					if ($m.Groups.Count -gt 1) {
						for($gi = 1; $gi -lt $m.Groups.Count; ++$gi) {
							$g = $m.Groups[$gi]
							$file = [FarNet.SetFile]::new($item, $true)
							$file.Data = @($g.Index, $g.Length)
							$file.Description = '{0,4:d}: {1}' -f $no, $line.Trim()
							$ExplorerData.Output.Add($file)
						}
					}
					else {
						$file = [FarNet.SetFile]::new($item, $true)
						$file.Data = @($m.Index, $m.Length)
						$file.Description = '{0,4:d}: {1}' -f $no, $line.Trim()
						$ExplorerData.Output.Add($file)
					}
				}
			}
		}
	}

	$ExplorerData.State = 'Completed'
}
} catch {
	$PSCmdlet.ThrowTerminatingError($_)
}}
