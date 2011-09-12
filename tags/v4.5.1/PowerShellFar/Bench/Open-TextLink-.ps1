
<#
.SYNOPSIS
	Opens a text link contained in the text.
	Author: Roman Kuzmin

.DESCRIPTION
	The script parses the passed text, the selected editor text, or the current
	line for a text link to some object (file, URL) and opens it in the editor,
	browser, etc. Recognised text link types: Visual Studio, PowerShell (error
	messages or Select-String), full and relative file system paths, URLs.

	"Visual Studio" and "Select-String" links may include the original line
	text after a column: <File>(<Line>):<Text> | <File>:<Line>:<Text>. In this
	case after opening the editor the script compares the text with the target
	line and, if it is different, tries to find the nearest line with the same
	text. Thus, such links are corrected dynamically in many cases when target
	lines are not changed.

	TEXT LINK EXAMPLES
	http://www.farmanager.com/
	C:\Program Files\Far\FarEng.lng(55):"Warning"
	C:\Program Files\Far\FarEng.lng:55:"Warning"
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
#! Order: Visual Studio, Select-String, PowerShell
if ($Text -match '\b(?<File>\w:[\\/].+?)\((?<Line>\d+),?(?<Char>\d+)?\)(?::(?<Text>.*))?' -or `
	$Text -match '^>?\s*(?<File>.+?):(?<Line>\d+):(?<Text>.*)' -or `
	$Text -match '\b(?<File>\w:[\\/][^:]+):(?<Line>\d+)(?:\s+\w+:(?<Char>\d+))?') {
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
		Open-FarEditor -Path $file
	}
	return
}

### Relative file system paths: quoted or simple.
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
		Open-FarEditor -Path $file
	}
	return
}

### URL
# From Colorer default.hrc NetURL scheme
$url = New-Object Regex @'
#REGEX
\b ((https?|ftp|news|nntp|wais|wysiwyg|gopher|javascript|castanet|about)
\:\/\/  | (www|ftp|fido[0-9]*)\.)
[\[\]\@\%\:\+\w\.\/\~\?\-\*=_#&;]+\b\/?
'@, 'IgnoreCase, IgnorePatternWhitespace'
if ($Text -match $url) {
	Start-Process $matches[0]
	return
}
