
<#
.SYNOPSIS
	Opens a file in editor by a text link under the caret.
	Author: Roman Kuzmin

.DESCRIPTION
	The script parses the passed text, the selected editor text, or the current
	line for a link to another file and opens it in the editor. Recognised text
	link types: Visual Studio, PowerShell, full and relative file system paths.

	"Visual Studio" links may include the original line text after a column:
	<File>(<Line>):<Text>. In this case after opening the editor the script
	compares the text with the target line and, if it is different, tries to
	find the nearest line with the same text. Thus, such links are corrected
	dynamically in many cases when target lines are not changed.

	TEXT LINK EXAMPLES
	C:\Program Files\Far\FarEng.lng(55): "Warning"
	C:\Program Files\Far\FarEng.lng:36 char:22
	C:\Program Files\Far\FarEng.lng(36,22)
	C:\Program Files\Far\FarEng.lng(32)
	C:\Program Files\Far\FarEng.lng:32
	"C:\Program Files\Far\FarEng.lng"
	C:\WINDOWS\setuplog.txt
	"..\Read Me.txt"
	.\ReadMe.txt

.LINK
	Get-TextLink-.ps1
#>

param
(
	# Text with embedded text links. Default: editor active text.
	$Text = $Psf.ActiveText
)

### Link with a position
if ($Text -match '\b(?<File>\w:[\\/].+?)\((?<Line>\d+),?(?<Char>\d+)?\)(?::(?<Text>.*))?' -or $Text -match '\b(?<File>\w:[\\/][^:]+):(?<Line>\d+)(?:\s+\w+:(?<Char>\d+))?') {
	$file = $matches.File
	if (![IO.File]::Exists($file)) {
		Show-FarMessage "File '$file' does not exist."
		return
	}

	### Create editor
	$Editor = $Far.CreateEditor()
	$Editor.FileName = $file
	$iLine = ([int]$matches.Line) - 1
	if ($matches.Char) {
		$Editor.GoTo((([int]$matches.Char) - 1), $iLine)
	}
	else {
		$Editor.GoToLine($iLine)
	}
	if (!$matches.Text -or !$matches.Text.Trim()) {
		$Editor.Open()
		return
	}

	### 'Opened' handler checks the line or searches the nearest by text
	$line = $matches.Text.Trim()
	$Editor.add_Opened({
		if ($Editor[-1].Text.Trim() -eq $line) { return }

		$index1 = $Editor.Caret.Y - 1
		$index2 = $index1 + 2
		while(($index1 -ge 0) -or ($index2 -lt $Editor.Count)) {
			if (($index1 -ge 0) -and ($Editor[$index1].Text.Trim() -eq $line)) {
				$Editor.GoToLine($index1)
				return
			}
			if (($index2 -lt $Editor.Count) -and ($Editor[$index2].Text.Trim() -eq $line)) {
				$Editor.GoToLine($index2)
				return
			}
			--$index1
			++$index2
		}
	})
	$Editor.Open()
	return
}

### Full file system paths: quoted and simple.
if ($Text -match '"(\w+:\\[^"]+)"' -or $Text -match '\b(\w+:\\[^\s:]+)') {
	$file = $matches[1]
	if (![IO.File]::Exists($file)) {
		Show-FarMessage "File '$file' does not exist."
	}
	else {
		Start-FarEditor -Path $file
	}
	return
}

### Relative file system paths: quoted and simple.
if ($Text -match '"(\.{1,2}[\\/][^"]+)"' -or $Text -match '(?:^|\s)(\.{1,2}[\\/][^\s:]+)') {
	if ($Far.Window.Kind -eq 'Editor') {
		$dir = [IO.Path]::GetDirectoryName($Far.Editor.FileName)
	}
	else {
		$dir = [Environment]::CurrentDirectory
	}
	$file = [IO.Path]::GetFullPath([IO.Path]::Combine($dir, $matches[1]))
	if (![IO.File]::Exists($file)) {
		Show-FarMessage "File '$file' does not exist."
	}
	else {
		Start-FarEditor -Path $file
	}
	return
}
