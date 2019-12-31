<#
.Synopsis
	Shows markdown as HTML or Far help file.
	Author: Roman Kuzmin

.Description
	Requires pandoc.exe and HtmlToFarHelp.exe in the path.

	The script opens the current topic from the editor as HLF in help viewer
	(example: Profile-Editor.ps1) or as HTML in the default browser (example:
	Invoke-Editor-.ps1).

	Markdown:
	- if $env:markdown is set then it is used as pandoc --from
	- .text files are treated as `markdown_phpextra`
	- .md, .markdown are converted as `gfm`

.Parameter FileName
		Specifies the Markdown file. If it is omitted then the file is taken
		from the current editor.

.Parameter Topic
		Specifies the help topic.

.Parameter Help
		Tells to open as help.
#>

[CmdletBinding()]
param(
	[string]$FileName,
	[string]$Topic,
	[switch]$Help
)

$ErrorActionPreference = 1
trap {Write-Error -ErrorRecord $_}

function Show-Markdown {
	param(
		[string]$FileName,
		[string]$Extension = ([System.IO.Path]::GetExtension($FileName))
	)

	$htm = "$env:TEMP\markdown.htm"
	$name = [System.IO.Path]::GetFileNameWithoutExtension($FileName)
	$title = $name + ' - ' + [System.IO.Path]::GetDirectoryName($FileName)
	if ($env:markdown) {
		$format = $env:markdown
	}
	elseif ($Extension -eq '.text') {
		$format = 'markdown_phpextra'
	}
	else {
		$format = 'gfm'
	}

	pandoc.exe $FileName --output=$htm --from=$format --standalone --metadata=pagetitle=$title
	if ($LastExitCode) {throw 'pandoc.exe failed.'}

	if ($Help) {
		$hlf = "$env:TEMP\HtmlToFarHelp.hlf"
		HtmlToFarHelp.exe from=$htm to=$hlf
		if ($LastExitCode) {throw 'HtmlToFarHelp failed.'}
		$Far.ShowHelp($hlf, $Topic, 'File')
	}
	elseif ($Topic) {
		$url = "file://$htm#$Topic"
		Start-Process $url
	}
	else {
		Invoke-Item $htm
	}
}

### open by path and topic
if ($FileName) {
	Show-Markdown (Resolve-Path $FileName)
	return
}

### open help from editor with current topic

# check editor
$editor = $Far.Editor
if (!$editor) {
	Show-FarMessage "Run it with FileName or from editor."
	return
}

# check file
$FileName = $editor.FileName
$Extension = [System.IO.Path]::GetExtension($FileName)
if (@('.md', '.text', '.markdown') -notcontains $Extension) {
	Show-FarMessage "Run it with FileName or .md, .text, .markdown in editor."
	return
}

# commit
$editor.Save()

# open from editor with the current topic
if ($Extension -eq '.text') {
	# manual heading identifiers
	for($i = $editor.Caret.Y; $i -ge 0; --$i) {
		if ($editor[$i].Text -match '^#+.*{#([a-zA-Z][a-zA-Z0-9_\-:.]*)}') {
			$Topic = $matches[1]
			break
		}
	}
}
else {
	# generated heading identifiers
	for($i = $editor.Caret.Y; $i -ge 0; --$i) {
		if ($editor[$i].Text -match '^##?\s+(.*)' -and $matches[1]) {
			$Topic = ($matches[1] -replace '\s+', '-' -replace '[^\w\-]').ToLower()
			break
		}
	}
}
Show-Markdown $FileName $Extension
