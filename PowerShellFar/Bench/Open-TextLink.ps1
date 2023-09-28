<#
.Synopsis
	Opens a text link contained in the text.
	Author: Roman Kuzmin

.Description
	The script searches for a link in:
	- specified text, parameter or clipboard
	- editor selected or current line text
	- all editor URLs (call on empty line)
	- user screen text (call in panels)

	The found link is opened in the editor or browser.

	Recognised text link types: Visual Studio, PowerShell (error messages or
	Select-String output), file system paths, URLs, markdown file links, etc.

	Links may include a hint, the original line text after a column:
	- <File>(<Line>):<Text>
	- <File>:<Line>:<Text>
	In this case after opening the editor the script compares the text with the
	target line and, if it is different, tries to find the nearest line with
	the same text. Thus, such links are corrected dynamically in many cases
	when target lines are not changed.

	A "Visual Studio" link hint in the editor may be in the next line.

	File system links may start with environment %variable%.

	TEXT LINK EXAMPLES

	Web link
		http://www.farmanager.com/

	VS link with a hint
		C:\Program Files\Far\FarEng.lng(55):"Warning"
		<or this line is a hint>

	SS link with a hint
		C:\Program Files\Far\FarEng.lng:55:"Warning"

	Others
		C:\Program Files\Far\FarEng.lng:36 char:22
		C:\Program Files\Far\FarEng.lng(36,22)
		C:\Program Files\Far\FarEng.lng(32)
		C:\Program Files\Far\FarEng.lng:32
		"C:\Program Files\Far\FarEng.lng"
		C:\WINDOWS\setuplog.txt
		"..\Read Me.txt"
		.\ReadMe.txt

	NuGet
		PackageReference ... Include="FarNet"

.Parameter Text
		Text with a text link.

.Parameter Clip
		Tells to get the clipboard text.

.Link
	Get-TextLink.ps1
#>

[CmdletBinding(DefaultParameterSetName='Text')]
param(
	[Parameter(ParameterSetName='Text', Position=0)]
	[string]$Text
	,
	[Parameter(ParameterSetName='Clip')]
	[switch]$Clip
)

### Regex for text links
#! Order:
# Visual Studio
# ClearScript (at file:2:3 -> text)
# Select-String
#  - use `\.\w+` (likely file extension) to exclude noise like `<date>:<time>`
# PowerShell (file:2 char:3), F# (file:line 2)
$regexTextLink = [regex]@'
(?x)
(?<VS> (?<File>(?:\b\w:|%\w+%)[\\\/].+?)\((?<Line>\d+),?(?<Char>\d+)?\)(?::\s*(?<Text>.*))? )
|
(?<CS> ^ \s* at \s+ (?:\w+ \s+)? \(? (?<File>.+?\.\w+) : (?<Line>\d+) : (?<Char>\d+) \)? (?:\s*->\s* (?<Text>.*))? )
|
(?<SS> ^>?\s*(?<File>.+?\.\w+):(?<Line>\d+):(?<Text>.*) )
|
(?<PS> (?<File>(?:\b\w:|%\w+%)[\\\/][^:]+):(?:line\s)?(?<Line>\d+)(?:\s+\w+:(?<Char>\d+))? )
'@

# Processes $matches of the text link regex
function Open-Match {
	$file = [System.Environment]::ExpandEnvironmentVariables($matches.File)
	if (![IO.File]::Exists($file)) {
		Show-FarMessage "File '$file' does not exist."
		return
	}

	$hintText = "$($matches.Text)".Trim()
	if (!$hintText -and $Editor -and $matches['VS']) {
		$findLine = $Editor.Caret.Y + 1
		if ($findLine -lt $Editor.Count) {
			$hintText = $Editor[$findLine].Text.Trim()
		}
	}

	# new editor with set position
	$Editor = $Far.CreateEditor()
	$Editor.FileName = $file
	$index = ([int]$matches.Line) - 1
	$char = if ($matches.Char) {([int]$matches.Char) - 1} else {0}
	$Editor.GoTo($char, $index)

	# open editor without hint
	if (!$hintText) {
		$Editor.Open()
		return
	}

	# search for the nearest line with hint and open
	$Editor.add_Opened({
		if ($Editor.Line.Text.Trim() -eq $hintText) {
			return
		}

		$index1 = $Editor.Caret.Y - 1
		$index2 = $index1 + 2
		$count = $Editor.Count
		while(($index1 -ge 0) -or ($index2 -lt $count)) {
			if (($index1 -ge 0) -and ($Editor[$index1].Text.Trim() -eq $hintText)) {
				$Editor.GoTo($char, $index1)
				return
			}
			if (($index2 -lt $count) -and ($Editor[$index2].Text.Trim() -eq $hintText)) {
				$Editor.GoTo($char, $index2)
				return
			}
			--$index1
			++$index2
		}
	})
	$Editor.Open()
}

### Get text

$From = $Far.Window.Kind
$Editor = if ($From -eq 'Editor') {$Far.Editor}

if ($Clip) {
	$Text = $Far.PasteFromClipboard()
}
elseif (!$Text) {
	$Text = $Psf.ActiveText

	### Find in user screen
	if (!$Text -and $From -eq 'Panels') {
		$place = $Far.UI.WindowPlace
		$Far.UI.ShowUserScreen()
		for($i = $place.Bottom; $i -ge $place.Top; --$i) {
			if (($_ = $Far.UI.GetBufferLineText($i)) -match $regexTextLink) {
				$Text = $_
				break
			}
		}
		$Far.UI.SaveUserScreen()
		if ($Text) {
			Open-Match
			return
		}
	}
}

### Link with a position
if ($Text -match $regexTextLink) {
	Open-Match
	return
}

### Full file system paths: quoted and not, either no spaces or space+word+\.
if ($Text -match '"((?:\w+:|%\w+%)\\[^"]+)"' -or $Text -match '((?:\b\w+:|%\w+%)\\([^\s:]|\s(?=\w+\\))+)') {
	$file = [System.Environment]::ExpandEnvironmentVariables($matches[1])
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

# simplified from Colorer default.hrc NetURL
$regexUrl = [regex]@'
(?x)
\b (?: \w+ :// | (www|ftp|fido[0-9]*) \. )
[\w\[\]\@\%\:\+\.\/\~\?\-\*=_#&;,]+\b/?
'@

if ($Text -match $regexUrl) {
	$url = $matches[0]
	if ($url -match '^(\w+)://' -and $matches[0] -notmatch '^https?$') {
		# app URL, possible output
		$Far.UI.ShowUserScreen()
		try {
			Start-Process $url
			Start-Sleep 2
		}
		finally {
			$Far.UI.SaveUserScreen()
		}
	}
	else {
		# usual URL
		Start-Process $url
	}
	return
}

### NuGet
if ($Text -match '\bPackageReference\b.*?\bInclude\b\s*=\s*\"(.+?)\"') {
	Start-Process "https://www.nuget.org/packages/$($matches[1])"
	return
}

### Markdown in the editor: jump to the internal link target
if ($Editor -and $Editor.FileName -match '\.(text|md|markdown)$') {
	# [...](#link)
	$match = $Editor.Line.MatchCaret('(?:\[.*\])\(#(.*)\)')
	if ($match) {
		$pattern = '^#{1,6}.*?\{#' + [regex]::Escape($match.Groups[1]) + '\}'
		foreach($line in $Editor.Lines) {
			if ($line.Text -match $pattern) {
				$Editor.GoToLine($line.Index)
				$Editor.Redraw()
				return
			}
		}
	}
	# [...](file)
	$match = $Editor.Line.MatchCaret('(?:\[.*\])\((.*)\)')
	if ($match) {
		$dir = [IO.Path]::GetDirectoryName($Far.Editor.FileName)
		$file = [IO.Path]::GetFullPath([IO.Path]::Combine($dir, $match.Groups[1]))
		if ([IO.File]::Exists($file)) {
			Open-FarEditor -Path $file
			return
		}
	}
}

### All URLs in the editor
if ($Editor) {
	$items = @(
		foreach($line in $Editor.Lines) {
			if ($line.Text -match $regexUrl) {
				New-FarItem $line.Text -Data ($matches[0])
			}
		}
	)
	if ($items) {
		if ($data = Out-FarList URL -Items $items) {
			Start-Process $data
		}
	}
}
