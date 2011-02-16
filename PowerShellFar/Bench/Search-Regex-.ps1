
<#
.SYNOPSIS
	Searches for a regex in files and work on results in a panel
	Author: Roman Kuzmin

.DESCRIPTION
	See help (e.g. run this and press F1) for more information about input,
	dialog controls and result panel keys.

.PARAMETER Regex
		Regular expression or *substring to search for a literal substring. If
		it is not defined then a dialog is opened where you can define it and
		the other intput data.
.PARAMETER Options
		.NET regular expression options. Used if -Regex is defined as a string,
		i.e. not a ready to use Regex object.
.PARAMETER InputObject
		Strings (file paths) or IO.FileInfo based objects (e.g. from
		Get-*Item). If it is not defined then objects are taken from the input
		pipeline. If there are no objects and -Regex is not set then in a
		dialog you have to define a command which output is used as input
		objects.
.PARAMETER Groups
		To put to a panel found regex groups instead of full matches. Ignored
		if -AllText is set.
.PARAMETER AllText
		To search in a file text read as one string.
#>

[CmdletBinding()]
param
(
	$Regex,
	$Options,
	$InputObject,
	[switch]$Groups,
	[switch]$AllText
)

Assert-Far -Panels -Message "Run this script from panels." -Title "Search-Regex"

# Collect input if any
if (!$InputObject) {
	$Host.UI.RawUI.WindowTitle = 'Collecting input...'
	$InputObject = @($input)
}

# Job parameters
$parameters = @{}

### Create and show a dialog
if (!$Regex) {

	$dialog = $Far.CreateDialog(-1, -1, 77, $(if ($InputObject) { 11 } else { 13 }))
	$dialog.TypeId = 'DA462DD5-7767-471E-9FC8-64A227BEE2B1'
	$dialog.HelpTopic = $Psf.HelpTopic + 'SearchRegex'
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

	$xGroups = $dialog.AddCheckBox($x, -1, '&Groups')
	$xGroups.Selected = [bool]$Groups

	$xAllText = $dialog.AddCheckBox($x, -1, '&All text')
	$xAllText.Selected = [bool]$AllText

	if (!$InputObject) {
		$xCommand = $dialog.AddCheckBox($x, -1, '&Background input')
	}

	$dialog.AddText(5, -1, 0, '').Separator = 1

	$dialog.Default = $dialog.AddButton(0, -1, 'Ok')
	$dialog.Default.CenterGroup = $true

	$dialog.Cancel = $dialog.AddButton(0, 0, 'Cancel')
	$dialog.Cancel.CenterGroup = $true

	# show dialog
	for(;;) {

		if (!$dialog.Show()) {
			return
		}

		# pattern
		$pattern = $eRegex.Text
		if (!$pattern) {
			$Far.Message("Expression must not be empty.", "Invalid Expression")
			$dialog.Focused = $eRegex
			continue
		}
		if ($pattern.StartsWith('*')) {
			$pattern = [regex]::Escape($pattern.Substring(1))
		}

		# options before regex
		if ($eOptions.Text) {
			try { $Options = [Text.RegularExpressions.RegexOptions]$eOptions.Text }
			catch {
				$Far.Message($_, "Invalid Options")
				$dialog.Focused = $eOptions
				continue
			}
		}
		else {
			$Options = [Text.RegularExpressions.RegexOptions]::None
		}

		# regex after options
		try {
			$Regex = New-Object Regex $pattern, $Options
		}
		catch {
			$Far.Message($_, "Invalid Expression")
			$dialog.Focused = $eRegex
			continue
		}

		if ($InputObject) {
			break
		}

		if ($xCommand.Selected) {
			$parameters.Command = $eInput.Text
			break
		}

		try {
			$Host.UI.RawUI.WindowTitle = 'Evaluating input...'
			$InputObject = Invoke-Expression $eInput.Text
			if ($InputObject) {
				break
			}

			$dialog.Focused = $eInput
			$Far.Message("There are no input files.", "Check the input")
		}
		catch {
			$Far.Message($_, "Invalid Input")
			$dialog.Focused = $eInput
			$InputObject = $null
		}
	}

	# other options
	$Groups = [bool]$xGroups.Selected
	$AllText = [bool]$xAllText.Selected
}

### Validate input and set job data
if (!$InputObject -and !$parameters.Command) {
	throw "There is no input to search in."
}
if (!$Regex) {
	throw "Parameter -Regex is empty."
}
if ($Regex -is [string]) {
	if ($Regex.StartsWith('*')) {
		$Regex = [regex]::Escape($Regex.Substring(1))
	}
	if (!$Options) { $Options = 'IgnoreCase' }
	$Regex = New-Object Regex $Regex, $Options
}
elseif ($Regex -isnot [regex]) {
	throw "Unknown type of parameter -Regex."
}

# Other parameters
$parameters.Items = $InputObject
$parameters.Regex = $Regex
$parameters.Groups = $Groups
$parameters.AllText = $AllText
$parameters.Out = $parameters
if ($parameters.Command) {
	$parameters.Path = (Get-Location -PSProvider FileSystem).Path
}

### Start search as a background job
$job = Start-FarJob -Output -Parameters:$parameters {
	param
	(
		$Command,
		$Items,
		$Regex,
		$Path,
		[switch]$Groups,
		[switch]$AllText,
		$Out
	)
	$Out.Total = 0
	$re = $Regex
	.{
		if ($Command) {
			Set-Location -LiteralPath $Path
			Invoke-Expression $Command
		}
		else {
			$Items
		}
	} | .{process{
		$item = $_
		trap { continue }
		.{
			if ($item -is [string]) {
				$item = Get-Item -LiteralPath $item -Force
			}
			if ($item -and !$item.PSIsContainer) {
				++$Out.Total
				if ($AllText) {
					$text = [System.IO.File]::ReadAllText($item.FullName, [System.Text.Encoding]::Default)
					if ($text -match $re) {
						for($m = $re.Match($text); $m.Success; $m = $m.NextMatch()) {
							$s = $text.Substring(0, $m.Index)
							$n = [regex]::Split($s, '[^\n]+')
							$null = $s -match '\n?([^\n]*)$'
							New-Object FarNet.SetFile $item, $true -Property @{
								Data = @($matches[1].Length, 0)
								Description = '{0,4:d}: {1}' -f ($n.Count - 1), $m.Value
							}
						}
					}
				}
				else {
					$no = 0
					foreach($line in [System.IO.File]::ReadAllLines($item.FullName, [System.Text.Encoding]::Default)) {
						++$no
						if ($line -match $re) {
							for($m = $re.Match($line); $m.Success; $m = $m.NextMatch()) {
								if ($Groups -and $m.Groups.Count -gt 1) {
									for($gi = 1; $gi -lt $m.Groups.Count; ++$gi) {
										$g = $m.Groups[$gi]
										New-Object FarNet.SetFile $item, $true -Property @{
											Data = @($g.Index, $g.Length)
											Description = '{0,4:d}: {1}' -f $no, $line.Trim()
										}
									}
								}
								else {
									New-Object FarNet.SetFile $item, $true -Property @{
										Data = @($m.Index, $m.Length)
										Description = '{0,4:d}: {1}' -f $no, $line.Trim()
									}
								}
							}
						}
					}
				}
			}
		}
	}}
}

### Create a panel with the job object for search results
$Panel = New-Object FarNet.Panel
$Panel.Highlighting = 'Full'
$Panel.RealNames = $true
$Panel.RightAligned = $true
$Panel.SortMode = 'Unsorted'
$Panel.Title = 'Searching...'
$Panel.ViewMode = 'Descriptions'
$Panel.Data.Host = $job
$Panel.Garbage.Add($job)

### Modes
# 'Descriptions'
$m0 = New-Object FarNet.PanelPlan
$c1 = New-Object FarNet.SetColumn -Property @{ Kind = 'NR'; Name = 'File' }
$c2 = New-Object FarNet.SetColumn -Property @{ Kind = 'Z'; Name = 'Match' }
$m0.Columns = $c1, $c2
$m0.StatusColumns = $c2
$Panel.SetPlan('Descriptions', $m0)
# 'LongDescriptions'
$m1 = $m0.Clone()
$m1.IsFullScreen = $true
$Panel.SetPlan('LongDescriptions', $m1)

### Idled: checks new data and updates
$Panel.add_Idled({&{
	$job = $this.Data.Host
	if (!$job.Parameters.Done) {
		if ($job.Output.Count) {
			$this.Update($false)
		}
		$title = '{0}: {1} lines in {2} files' -f $job.JobStateInfo.State, $this.Files.Count, $job.Parameters.Total
		if ($this.Title -ne $title) {
			$this.Title = $title
			$this.Redraw()
		}
		if ($job.JobStateInfo.State -ne 'Running') {
			$job.Parameters.Done = $true
			$Far.UI.SetProgressFlash()
		}
	}
}})

### UpdateFiles: reads found (with wrapper - workaround Find mode)
$Panel.add_UpdateFiles({&{
	$job = $this.Data.Host
	foreach($e in $job.Output.ReadAll()) {
		$this.Files.Add($e)
	}
}})

### KeyPressed: handles keys
$Panel.add_KeyPressed({&{
	### [Enter] opens an editor at the selected match
	if ($_.Code -eq [FarNet.VKeyCode]::Enter -and $_.State -eq 0 -and !$Far.CommandLine.Length) {
		$i = $this.CurrentFile
		if (!$i -or $i.Description -notmatch '^\s*(\d+):') { return }
		$_.Ignore = $true
		$e = New-FarEditor $i.Name ($matches[1]) -DisableHistory
		$f = $e.Frame
		$e.Open()
		$m = $i.Data
		$f.VisibleLine = $f.CaretLine - $Host.UI.RawUI.WindowSize.Height / 3
		$f.CaretColumn = $m[0] + $m[1]
		$e.Frame = $f
		$c = $e[-1] # can be null if a file is already opened
		if ($m[1] -and $c) {
			$c.SelectText($m[0], $f.CaretColumn)
			$e.Redraw()
		}
		return
	}
	### [F1] opens Search-Regex help topic
	if ($_.Code -eq [FarNet.VKeyCode]::F1 -and $_.State -eq 0) {
		$Far.ShowHelp($Psf.AppHome, 'SearchRegex', 'Path')
		$_.Ignore = $true
		return
	}
}})

### Escaping: ask for exit
$Panel.add_Escaping({&{
	# processed
	$_.Ignore = $true
	# close if empty:
	if (!$this.Files.Count) {
		$this.Close()
		return
	}
	# not empty; ask
	$r = Show-FarMessage "How would you like to continue?" -Caption $this.Title -Choices '&Close', '&Push', 'Cancel'
	# close
	if ($r -eq 0) {
		$this.Close()
	}
	# push
	elseif ($r -eq 1) {
		$this.Push()
	}
}})

### Go!
$Panel.Open()
