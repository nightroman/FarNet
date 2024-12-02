<#
.Synopsis
	Searches for a regex in files and opens the result panel.
	Author: Roman Kuzmin

.Description
	See help (run this and press [F1]) for more information about input, dialog
	controls and result panel keys.

.Parameter Regex
		Regular expression pattern or instance. If it is omitted then a
		dialog is opened where you can define this and other parameters.

		If the regex defines capturing groups then each group is treated as a
		match and gets selected on opening.

		With AllText editor selection is not used and groups are ignored.

.Parameter Options
		Regular expression options and extra options. It is used when Regex is
		not a regex instance. See SearchRegexOptions here for available values.

		The option Singleline/s implies AllText true.

.Parameter InputObject
		Strings (file paths) and IO.FileInfo (from Get-*Item) as ready input or
		a script block for background input. If it is not defined then objects
		come from the pipeline. If there are no objects and Regex is not set
		then an input command is specified in the dialog.

		Built-in utility commands:
		* Get-EditorHistory - files from editor history excluding network paths

.Parameter AllText
		Tells to search in all text, not in separate lines. The editor opens
		without selection with the caret at the found match.

		The option Singleline/s implies AllText true.

.Example
	> ps: Start-Far.ps1 ps:Search-Regex -Hidden
#>

param(
	$Regex,
	$Options,
	$InputObject,
	[switch]$AllText
)

$ErrorActionPreference = 1
if ($args) {Write-Error -ErrorAction Stop "Invalid arguments: $args"}

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

# Pattern to whole word
function WholeWord($_) {
	"(?(?=\w)\b)$_(?(?<=\w)\b)"
}

# Utility
function Get-EditorHistory {
	$array = $Far.History.Editor()
	[System.Array]::Reverse($array)
	$array | .{process{ if (!$_.Name.StartsWith('\\')) {$_.Name} }}
}

# Collect input if any
if (!$InputObject) {
	$Host.UI.RawUI.WindowTitle = 'Collecting input...'
	$InputObject = @($input)
}

# Job parameters
$parameters = @{}

### Create and show a dialog
if (!$Regex) {
	$dialog = $Far.CreateDialog(-1, -1, 77, $(if ($InputObject) { 10 } else { 12 }))
	$dialog.TypeId = 'DA462DD5-7767-471E-9FC8-64A227BEE2B1'
	$dialog.HelpTopic = "<$($Psf.AppHome)\\>search-regexps1"
	[void]$dialog.AddBox(3, 1, 0, 0, 'Search-Regex')
	$x = 13

	[void]$dialog.AddText(5, -1, 0, '&Pattern')
	$eRegex = $dialog.AddEdit($x, 0, 71, '')
	$eRegex.History = 'SearchText'
	$eRegex.UseLastHistory = $true

	[void]$dialog.AddText(5, -1, 0, '&Options')
	$eOptions = $dialog.AddEdit($x, 0, 71, $Options)
	$eOptions.History = 'RegexOptions'
	$eOptions.UseLastHistory = $true

	if (!$InputObject) {
		[void]$dialog.AddText(5, -1, 0, '&Input')
		$eInput = $dialog.AddEdit($x, 0, 71, '')
		$eInput.History = 'PowerShellItems'
		$eInput.UseLastHistory = $true
	}

	$dialog.AddText(5, -1, 0, '').Separator = 1

	$xAllText = $dialog.AddCheckBox($x, -1, '&All text')
	$xAllText.Selected = [bool]$AllText

	if (!$InputObject) {
		$xScript = $dialog.AddCheckBox($x, -1, '&Background input')
	}

	$dialog.AddText(5, -1, 0, '').Separator = 1

	$dialog.Default = $dialog.AddButton(0, -1, 'Ok')
	$dialog.Default.CenterGroup = $true

	$dialog.Cancel = $dialog.AddButton(0, 0, 'Cancel')
	$dialog.Cancel.CenterGroup = $true

	# show dialog
	for() {
		if (!$dialog.Show()) {return}

		# options
		if ($eOptions.Text) {
			try { $Options = [SearchRegexOptions]($eOptions.Text.Trim() -split '\W+' -join ',') }
			catch {
				$Far.Message($_, 'Invalid options')
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
			$Far.Message('Pattern must not be empty.', 'Invalid pattern')
			$dialog.Focused = $eRegex
			continue
		}
		if ([int]$Options -band [SearchRegexOptions]::SimpleMatch) {
			$pattern = [regex]::Escape($pattern)
		}
		if ([int]$Options -band [SearchRegexOptions]::WholeWord) {
			$pattern = WholeWord $pattern
		}

		# regex after options and pattern
		try {
			$RegexOptions = [Text.RegularExpressions.RegexOptions](([int]$Options) -band (-bnot (1024 + 2048)))
			$Regex = [regex]::new($pattern, $RegexOptions)
		}
		catch {
			$Far.Message($_, 'Invalid pattern')
			$dialog.Focused = $eRegex
			continue
		}

		# ready input
		if ($InputObject) {
			break
		}

		try {
			# parse command
			if (!($script = $eInput.Text)) { $script = 'Get-ChildItem' }
			$script = [scriptblock]::Create($script)

			# background
			if ($xScript.Selected) {
				$parameters.Script = $script
				break
			}

			# invoke
			$Host.UI.RawUI.WindowTitle = 'Evaluating input...'
			$InputObject = & $script
			if ($InputObject) {
				break
			}

			# no input
			$dialog.Focused = $eInput
			$Far.Message('There are no input files.', 'Input')
		}
		catch {
			$Far.Message($_, 'Invalid Input', 'LeftAligned')
			$dialog.Focused = $eInput
			$InputObject = $null
		}
	}

	# other options
	$AllText = [bool]$xAllText.Selected
}
elseif ($InputObject -is [scriptblock]) {
	$parameters.Script = [scriptblock]::Create($InputObject) #!
}

### Validate input and set job data
try {
	if ($Regex -isnot [regex]) {
		$Regex = "$Regex"
		$Options = if ($Options) {[SearchRegexOptions]$Options} else {0}
		if ([int]$Options -band [SearchRegexOptions]::SimpleMatch) {
			$Regex = [regex]::Escape($Regex)
		}
		if ([int]$Options -band [SearchRegexOptions]::WholeWord) {
			$Regex = WholeWord $Regex
		}
		$RegexOptions = [Text.RegularExpressions.RegexOptions](([int]$Options) -band (-bnot (1024 + 2048)))
		$Regex = [regex]::new($Regex, $RegexOptions)
	}

	if (!$InputObject -and !$parameters.Script) {
		throw "There is no input to search in."
	}

	if ($Regex.Options -band [System.Text.RegularExpressions.RegexOptions]::Singleline) {
		$AllText = $true
	}
}
catch {
	Write-Error -ErrorAction Stop $_
}

# Other parameters
$parameters.Items = $InputObject
$parameters.Regex = $Regex
$parameters.AllText = $AllText
$parameters.Out = $parameters
if ($parameters.Script) {
	$parameters.Path = (Get-Location -PSProvider FileSystem).Path
}

### Start search as a background job
$job = Start-FarJob -Output -Parameters:$parameters {
	param(
		$Script,
		$Items,
		$Regex,
		$Path,
		[switch]$AllText,
		$Out
	)
	$Out.Total = 0
	$re = $Regex
	$(
		if ($Script) {
			Set-Location -LiteralPath $Path
			& $Script
		}
		else {
			$Items
		}
	) | .{process{
		$item = $_
		trap {continue}
		$(
			if ($item -is [string]) {
				$item = Get-Item -LiteralPath $item -Force
			}
			if ($item -and !$item.PSIsContainer) {
				++$Out.Total
				if ($AllText) {
					$text = [System.IO.File]::ReadAllText($item.FullName)
					if ($text -match $re) {
						for($m = $re.Match($text); $m.Success; $m = $m.NextMatch()) {
							$s = $text.Substring(0, $m.Index)
							$n = [regex]::Count($s, '\n')
							$null = $s -match '\n?([^\n]*)$'
							$file = [FarNet.SetFile]::new($item, $true)
							$file.Data = @($Matches[1].Length, 0)
							$file.Description = '{0,4:d}: {1}' -f ($n + 1), $m.Value
							$file
						}
					}
				}
				else {
					$no = 0
					foreach($line in [System.IO.File]::ReadAllLines($item.FullName)) {
						++$no
						if ($line -match $re) {
							for($m = $re.Match($line); $m.Success; $m = $m.NextMatch()) {
								if ($m.Groups.Count -gt 1) {
									for($gi = 1; $gi -lt $m.Groups.Count; ++$gi) {
										$g = $m.Groups[$gi]
										$file = [FarNet.SetFile]::new($item, $true)
										$file.Data = @($g.Index, $g.Length)
										$file.Description = '{0,4:d}: {1}' -f $no, $line.Trim()
										$file
									}
								}
								else {
									$file = [FarNet.SetFile]::new($item, $true)
									$file.Data = @($m.Index, $m.Length)
									$file.Description = '{0,4:d}: {1}' -f $no, $line.Trim()
									$file
								}
							}
						}
					}
				}
			}
		)
	}}
}

### Explorer with the job for search results
$Explorer = [PowerShellFar.PowerExplorer]::new('7ef0bbec-9509-4223-a452-ea928ac9846c')
$Explorer.Data = $job
$Explorer.AsGetFiles = {param($this, $_)
	$job = $this.Data
	foreach($e in $job.Output.ReadAll()) {
		$this.Cache.Add($e)
	}
}

### Panel with the explorer
$Panel = [FarNet.Panel]::new($Explorer)
$Panel.Highlighting = 'Full'
$Panel.RealNames = $true
$Panel.RightAligned = $true
$Panel.SortMode = 'Unsorted'
$Panel.Title = 'Searching...'
$Panel.ViewMode = if ($Far.Panel.IsVisible) {'Descriptions'} else {'LongDescriptions'}
$Panel.Garbage.Add($job)

### Plan

# 'Descriptions'
$col1 = [FarNet.SetColumn]@{ Kind = 'NR'; Name = 'File' }
$col2 = [FarNet.SetColumn]@{ Kind = 'Z'; Name = 'Match' }
$plan = [FarNet.PanelPlan]@{ Columns = $col1, $col2; StatusColumns = $col2 }
$Panel.SetPlan('Descriptions', $plan)

# 'LongDescriptions'
$plan = $plan.Clone()
$plan.IsFullScreen = $true
$Panel.SetPlan('LongDescriptions', $plan)

### Timer: check new data and updates
$Panel.TimerInterval = 2000
$Panel.add_Timer({
	$job = $this.Explorer.Data
	if (!$job.Parameters.Done) {
		if ($job.Output.Count) {
			$this.Update($false)
		}
		$title = '{0}: {1} lines in {2} files' -f $job.JobStateInfo.State, $this.Explorer.Cache.Count, $job.Parameters.Total
		if ($this.Title -ne $title) {
			$this.Title = $title
			$this.Redraw()
		}
		if ($job.JobStateInfo.State -ne 'Running') {
			$job.Parameters.Done = $true
			$Far.UI.SetProgressFlash()
			$Far.UI.WindowTitle = $title
		}
	}
})

### KeyPressed: handles keys
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

### Escaping: ask for exit
$Panel.add_Escaping({
	# processed
	$_.Ignore = $true
	# close if empty:
	if (!$this.Explorer.Cache.Count) {
		$this.CloseChild()
		return
	}
	# not empty; ask
	$r = Show-FarMessage "How would you like to continue?" -Caption $this.Title -Choices '&Close', '&Push', 'Cancel'
	# close
	if ($r -eq 0) {
		$this.CloseChild()
	}
	# push
	elseif ($r -eq 1) {
		$this.Push()
	}
})

### Go!
$Panel.OpenChild($null)
