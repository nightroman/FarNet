<#
.Synopsis
	Searches files and panel found results.
	Author: Roman Kuzmin

.Description
	See help (run this and [F1]) for more information about input, dialog
	controls and result panel keys.

.Parameter Regex
		Regular expression [string] or [regex]. If it is omitted then a dialog
		shows to define this and other options.

		Regex named groups produce additional named matches.
		Names cannot start with a number or contain punctuation.

.Parameter Options
		When Regex is [string], specifies regex options and extra options.
		See SearchRegexOptions in code for available values and aliases.

		The string is comma separated values or joined one-letter aliases.

		EXTRA OPTIONS

		SimpleMatch/t - Regex is the plain text, substring to match as is.

		WholeWord/w - Tells to test non-word boundaries before and after.

		AllText/a - Tells to process files as whole text, not lines.

		Note that Singleline/s implies AllText/a because without it inputs are
		lines and Singleline/s is not needed. They are not the same, AllText/a
		without Singleline/s means `\n` is not in `.` and `\n` may be used to
		control "number of lines between parts", e.g. `before.*\n.*after` =
		"before" with "after" at the next line.

.Parameter InputObject
		Strings (file paths) and FileInfo (from Get-*Item). If it is not
		defined then items come from the pipeline. If there are no items
		and Regex is not set then the dialog defines the input command.

		Built-in helpers:
		- Get-EditorHistory - files from history excluding network paths.
#>

[CmdletBinding()]
param(
	[object]$Regex
	,
	[string]$Options
	,
	[Parameter(ValueFromPipeline=1)]
	[object]$InputObject
)

#requires -Version 7.4

begin {

[Flags()]
enum SearchRegexOptions {
	None = 0
	i = 1; IgnoreCase = 1
	m = 2; Multiline = 2
	n = 4; ExplicitCapture = 4
	Compiled = 8
	s = 16; Singleline = 16
	x = 32; IgnorePatternWhitespace = 32
	RightToLeft = 64
	ECMAScript = 256
	CultureInvariant = 512

	t = 1024; SimpleMatch = 1024
	w = 2048; WholeWord = 2048
	a = 4096; AllText = 4096
}

$ErrorActionPreference=1; if ($Host.Name -ne 'FarHost') {Write-Error 'Requires FarHost.'}
$Items = [System.Collections.Generic.List[object]]::new()

}

process {

$Items.Add($InputObject)

}

end { try {

function Get-EditorHistory {
	$array = $Far.History.Editor()
	[System.Array]::Reverse($array)
	$array | .{process{ if (!$_.Name.StartsWith('\\')) {$_.Name} }}
}

function whole_word_regex($_) {
	"(?(?=\w)\b)$_(?(?<=\w)\b)"
}

function to_options_string([string]$Options) {
	$r = ''
	$a = $Options.Split(',', [System.StringSplitOptions]'RemoveEmptyEntries, TrimEntries')
	for($$ = 0; $$ -lt $a.Length; ++$$) {
		$s = $a[$$]
		$o = ''
		if (![Enum]::TryParse([SearchRegexOptions], $s, $true, [ref]$o)) {
			if ($s.Length -eq 1) {
				try { [SearchRegexOptions]$s }
				catch { throw "'$Options': $_"}
			}
			$o = to_options_string ($s.ToCharArray() -join ', ')
		}
		$r += $$ ? ", $o" : $o
	}
	$r
}

$InPattern = ''
$InOptions = ''
function regex_from_pattern_and_options($Pattern, [SearchRegexOptions]$SearchOptions) {
	$Script:InPattern = $Pattern
	$Script:InOptions = $SearchOptions

	if ([int]$SearchOptions -band [SearchRegexOptions]::SimpleMatch) {
		$Pattern = [regex]::Escape($Pattern)
	}
	if ([int]$SearchOptions -band [SearchRegexOptions]::WholeWord) {
		$Pattern = whole_word_regex $Pattern
	}
	if ([int]$SearchOptions -band [SearchRegexOptions]::AllText) {
		$Script:AllText = $true
	}
	$RegexOptions = [Text.RegularExpressions.RegexOptions](([int]$SearchOptions) -band (-bnot (1024 + 2048 + 4096)))
	[regex]::new($Pattern, $RegexOptions)
}

if ($MyInvocation.InvocationName -eq '.') {
	return
}

$AllText = $false

### Regex dialog
if (!$Regex) {
	$dialog = $Far.CreateDialog(-1, -1, -1, 9)
	[int]$w1 = $dialog.Rect.Width
	[int]$w2 = $w1 - 6
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

	$dialog.AddText(-1, -1, 0, '').Separator = $true

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
			try {
				$SearchOptions = [SearchRegexOptions](to_options_string $eOptions.Text)
			}
			catch {
				Show-FarMessage $_ 'Invalid options'
				$dialog.Focused = $eOptions
				continue
			}
		}
		else {
			$SearchOptions = [SearchRegexOptions]::None
		}

		# pattern after options
		$pattern = $eRegex.Text
		if (!$pattern) {
			Show-FarMessage 'Pattern must not be empty.' 'Invalid pattern'
			$dialog.Focused = $eRegex
			continue
		}

		# regex after options and pattern
		$Regex = regex_from_pattern_and_options $pattern $SearchOptions

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
				$text = "(Get-ChildItem . -File -Force -Recurse -Include $text).Where({`$_.FullName -notlike '*\obj\*'})"
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
}

### Validate input and set job data
if ($Regex -isnot [regex]) {
	$Regex = "$Regex"
	$SearchOptions = $Options ? [SearchRegexOptions]$Options : 0
	$Regex = regex_from_pattern_and_options $pattern $SearchOptions
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
	Regex = "* $InPattern * $InOptions"
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

	$title = '{0}: {1} in {2} files {3}' -f $ExplorerData.State, $this.Explorer.Cache.Count, $ExplorerData.CountItems, $ExplorerData.Regex
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
		if (!$file) {
			return
		}
		$_.Ignore = $true
		$x1, $y1, $x2, $y2 = $file.Data
		$editor = New-FarEditor $file.Name ($y1 + 1) -DisableHistory
		$frame = $editor.Frame
		$editor.Open()
		$frame.CaretColumn = $x1
		$frame.VisibleLine = $frame.CaretLine - $Host.UI.RawUI.WindowSize.Height / 3
		$editor.Frame = $frame
		$editor.SelectText($x1, $y1, $x2, $y2)
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

	function __fileLineMatch($item, $i, $m) {
		$file = [FarNet.SetFile]::new($item, $true)
		$file.Data = @($m.Index, $i, ($m.Index + $m.Length - 1), $i)
		$file.Description = $m.Name -eq '0' ? "$($i + 1): $($line.Trim())" : "$($m.Name): $($m.Value)"
		$ExplorerData.Output.Add($file)
	}

	function __fileTextMatch($item, $text, $m) {
		$file = [FarNet.SetFile]::new($item, $true)
		$1 = [FarNet.Works.Kit]::IndexToColumnLine($text, $m.Index)
		$2 = [FarNet.Works.Kit]::IndexToColumnLine($text, $m.Index + $m.Length - 1)
		$file.Data = @($1.Item1, $1.Item2, $2.Item1, $2.Item2)
		$file.Description = $m.Name -eq '0' ? "$($1.Item2 + 1): $($m.Value)" : "$($m.Name): $($m.Value)"
		$ExplorerData.Output.Add($file)
	}

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
			$text = try {[System.IO.File]::ReadAllText($item)} catch {continue}
			for($m = $Regex.Match($text); $m.Success; $m = $m.NextMatch()) {
				foreach($g in $m.Groups) {
					if (($_=$g.Name) -eq '0' -or ($_ -and ![char]::IsDigit($_))) {
						__fileTextMatch $item $text $g
					}
				}
			}
			continue
		}

		$$ = -1
		$lines = try {[System.IO.File]::ReadAllLines($item)} catch {continue}
		foreach($line in $lines) {
			++$$
			for($m = $Regex.Match($line); $m.Success; $m = $m.NextMatch()) {
				foreach($g in $m.Groups) {
					if (($_=$g.Name) -eq '0' -or ($_ -and ![char]::IsDigit($_))) {
						__fileLineMatch $item $$ $g
					}
				}
			}
		}
	}

	$ExplorerData.State = 'Completed'
}}
catch {
	$PSCmdlet.ThrowTerminatingError($_)
}}
