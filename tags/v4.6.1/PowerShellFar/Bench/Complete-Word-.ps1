
<#
.Synopsis
	Completes the current word in editor, command line or edit box.
	Author: Roman Kuzmin

.Description
	The script implements a classic task of completing a current word. The
	script can be run for the current editor, the command line or any dialog
	edit control. Candidate words are taken from the current text in editor or
	from Far commands history or from edit control history if any.

	Words are grouped by the preceding symbol and only then sorted. The first
	group candidates are usually used more frequently, at least in source code.

	Autoloaded function technique: only the first call loads the script from
	disk then the global function is called from memory. Rules:
	- keep it in a directory included in the path;
	- call it just by name with no extension.

.Link
	Help: Autoloaded functions

.Example
	Complete-Word-
#>

# Autoloaded
function global:Complete-Word-
{
	# get edit line
	$Line = $Far.Line
	if (!$Line) {
		return
	}

	# current word
	$pos = $Line.Caret
	$text = $Line.Text
	$word = $text.Substring(0, $pos)
	if ($word -notmatch '(^|\W)(\w[-\w]*)$') {
		return
	}
	$pref = $matches[1]
	$word = $matches[2]
	$skip = $null
	if ($text.Substring($pos) -match '^([-\w]+)') {
		$skip = $word + $matches[1]
	}

	# collect matching words in editor or\and history
	$words = @{}
	$re = New-Object Regex "(^|\W)($word[-\w]+)", 'IgnoreCase'
	filter CollectWords
	{
		for($m = $re.Match($_); $m.Success; $m = $m.NextMatch()) {
			$w = $m.Groups[2].Value
			if ($w -eq $skip) { continue }
			$p = $m.Groups[1].Value
			if ($words.Contains($w)) {
				if ($p -eq $pref) {
					$words[$w] = $pref
				}
			} elseif ($p -eq $pref) {
				$words.Add($w, $pref)
			}
			else {
				$words.Add($w, $null)
			}
		}
	}
	# cases: source
	switch($Line.WindowKind) {
		'Editor' {
			$Editor = $Far.Editor
			$Editor.Lines | CollectWords
			if ($Editor.FileName -like '*.psfconsole') {
				$Psf.GetHistory(0) | CollectWords
			}
		}
		'Dialog' {
			$control = $Far.Dialog.Focused
			if ($control.History) {
				$Far.GetDialogHistory($control.History) | CollectWords
			}
			else {
				$Far.GetHistory('SavedHistory') | CollectWords
			}
		}
		default {
			$Far.GetHistory('SavedHistory') | CollectWords
		}
	}
	if ($words.Count -eq 0) {
		return
	}

	# select a word
	if ($words.Count -eq 1) {
		# 1 word
		$w = @($words.Keys)[0]
	}
	else {
		# select 1 word from list
		$cursor = $Far.UI.WindowCursor
		$w = .{
			$words.GetEnumerator() | .{process{ if ($_.Value) { $_.Key } }} | Sort-Object
			$words.GetEnumerator() | .{process{ if (!$_.Value) { $_.Key } }} | Sort-Object
		} |
		Out-FarList -Popup -IncrementalOptions 'Prefix' -Incremental "$word*" -X $cursor.X -Y $cursor.Y
		if (!$w) {
			return
		}
	}

	# complete by the selected word
	$Line.InsertText($w.Substring($word.Length))
}

Complete-Word-
