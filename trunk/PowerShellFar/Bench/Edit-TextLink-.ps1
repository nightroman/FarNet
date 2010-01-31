
<#
.SYNOPSIS
	Opens a file in editor by a link at the current text.
	Author: Roman Kuzmin

.DESCRIPTION
	The script parses the passed text, the selected editor text, or the current
	line for a link to another file and opens it in the editor. Recognised text
	link types: Visual Studio, PowerShell, full and relative file system paths.

	Visual Studio links may include the original line text after a column:
	<file path>(<line number>):<line text>

	In this case after opening the editor the script compares the text with the
	target line and, if it is different, tries to find the nearest line with
	the same text. Thus, the link may work "correctly" in more cases even if
	some lines were added or removed before the target line.

	TEXT LINK EXAMPLES
	C:\Program Files\Far\FarEng.lng(55): "Warning"
	C:\Program Files\Far\FarEng.lng(32)
	C:\Program Files\Far\FarEng.lng:32
	"C:\Program Files\Far\FarEng.lng"
	C:\WINDOWS\setuplog.txt
	"..\Read Me.txt"
	.\ReadMe.txt

	SEE ALSO
	Get-TextLink-.ps1
#>

param
(
	# Text with embedded text links. Default: editor hot text.
	$Text = $Psf.HotText
)

### Link with a line number.
if ($Text -match '\b(\w:[\\/].+?)\((\d+)\)(?::(.*))?' -or $Text -match '\b(\w:[\\/][^:]+):(\d+)') {
	$file = $matches[1]
	if (![IO.File]::Exists($file)) {
		Show-FarMessage "File '$file' does not exist."
	}
	else {
		### Create editor
		$Editor = $Far.CreateEditor()
		$Editor.FileName = $file
		$Editor.GoToLine(([int]$matches[2]) - 1)
		if (!$matches[3] -or !$matches[3].Trim()) {
			$Editor.Open()
			return
		}

		### 'Opened' handler checks the line or searches the nearest by text
		$line = $matches[3].Trim()
		$Editor.add_Opened({
			if ($Editor.CurrentLine.Text.Trim() -eq $line) { return }

			$Lines = $Editor.Lines
			$index1 = $Editor.Cursor.Y - 1
			$index2 = $index1 + 2
			while(($index1 -ge 0) -or ($index2 -lt $Lines.Count)) {
				if (($index1 -ge 0) -and ($Lines[$index1].Text.Trim() -eq $line)) {
					$Editor.GoToLine($index1)
					return
				}
				if (($index2 -lt $Lines.Count) -and ($Lines[$index2].Text.Trim() -eq $line)) {
					$Editor.GoToLine($index2)
					return
				}
				--$index1
				++$index2
			}
		})
		$Editor.Open()
	}
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
	if ($Far.WindowType -eq 'Editor') {
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
