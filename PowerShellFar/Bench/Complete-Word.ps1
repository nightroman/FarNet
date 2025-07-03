<#
.Synopsis
	Completes the current word in editors.
	Author: Roman Kuzmin

.Description
	The script completes the current word in editors, command line, dialog
	edit controls. Candidate words are taken from the current editor text,
	command history, or dialog history respectively.

	Words are grouped by the preceding symbol and sorted. The first group
	candidates are usually used more frequently, at least in source code.
#>

# get edit line
$Line = $Far.Line
Assert-Far ($Line -and !$Line.IsReadOnly) -Message 'Missing or read only edit line.' -Title 'Complete word'

# current word
$match = $Line.MatchCaret('\w[-\w]*')
if (!$match) {
	return
}

$text = $Line.Text
$word = $text.Substring($match.Index, $Line.Caret - $match.Index)
if (!$word) {
	return
}

$Pref = if ($match.Index) {[string]$text[$match.Index - 1]}

# collect matching words in editor or history
$words = [hashtable]::new()
$re = [regex]::new("(^|\W)($word[-\w]+)", 'IgnoreCase')

class CompleteWordInfo {
	[string]$Name
	[string]$Pref
	[int]$Count
}

filter collect_words {
	for($m = $re.Match($_); $m.Success; $m = $m.NextMatch()) {
		$w = $m.Groups[2].Value

		$info = $words[$w]
		if ($info) {
			++$info.Count
		}
		else {
			$info = [CompleteWordInfo]::new()
			$info.Name = $w
			$words.Add($w, $info)
		}

		if ($Pref -and $Pref -eq $m.Groups[1].Value) {
			$info.Pref = $Pref
		}
	}
}

# cases: source
switch($Line.WindowKind) {
	Editor {
		$Editor = $Far.Editor
		$Editor.Lines | collect_words
		if ($Editor.FileName -like '*.interactive.ps1') {
			$Psf.GetHistory(0) | collect_words
		}
	}
	Dialog {
		$control = $Far.Dialog.Focused
		if ($control.History) {
			$Far.History.Dialog($control.History).ForEach('Name') | collect_words
		}
	}
	default {
		$Far.History.Command().ForEach('Name') | collect_words
	}
}

if ($words.get_Count() -eq 0) {
	return
}

# select a word
if ($words.get_Count() -eq 1) {
	$selected = @($words.get_Keys())[0]
}
else {
	# select from list
	$sort = @{Expression='Count'; Descending=$true}, 'Name'
	$cursor = $Far.UI.WindowCursor
	$info = $(
		$words.get_Values().Where({$_.Pref}) | Sort-Object $sort
		$words.get_Values().Where({!$_.Pref}) | Sort-Object $sort
	) |
	Out-FarList -Text Name -Popup -IncrementalOptions Prefix -Incremental "$word*" -X $cursor.X -Y $cursor.Y
	if (!$info) {
		return
	}
	$selected = $info.Name
}

# complete by the selected word
$Line.InsertText($selected.Substring($word.Length))
if ($Line.WindowKind -eq 'Editor') {
	$Editor.Redraw()
}
