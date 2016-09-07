
<#
.Synopsis
	Shows markdown as HTML or Far help file.
	Author: Roman Kuzmin

.Description
	Requires pandoc.exe and HtmlToFarHelp.exe (NuGet) in the path.

	The script opens the current topic from the Far editor as HLF in the Far
	help viewer (example: Profile-Editor-.ps1) or as HTML in the default
	browser (example: Invoke-Editor-.ps1).

.Parameter FileName
		Markdown path; if none then a file is taken from the editor.

.Parameter Topic
		Help topic in a file.

.Parameter Help
		Tells to open as Far help.
#>

[CmdletBinding()]
param(
	[string]$FileName,
	[string]$Topic,
	[switch]$Help
)

function Show {
	$htm = "$env:TEMP\markdown.htm"

	if ($Help) {
		MarkdownToHtml.exe "From=$FileName" "To=$htm"
	}
	else {
		$title = [System.IO.Path]::GetFileName($FileName) + ' - ' + [System.IO.Path]::GetDirectoryName($FileName)
		$format = if ($env:markdown) {
			$env:markdown
		}
		elseif ($ext -eq '.text') {
			'markdown_phpextra'
		}
		elseif ($Path -match '\bwiki\b') {
			'markdown_github'
		}
		else {
			'markdown_strict+backtick_code_blocks'
		}
		pandoc.exe --standalone --title-prefix=$title --from=$format -o $htm $FileName
	}

	if ($LastExitCode) {throw 'pandoc.exe failed.'}

	if ($Help) {
		$hlf = "$env:TEMP\HtmlToFarHelp.hlf"
		HtmlToFarHelp "From=$htm" "To=$hlf"
		if ($LastExitCode) {throw 'HtmlToFarHelp failed.'}
		$Far.ShowHelp($hlf, $Topic, 'File')
	}
	elseif ($Topic) {
		$url = "$env:TEMP\markdown.url"
		Set-Content $url -Encoding Unicode "[InternetShortcut]`r`nURL=file://$htm#$Topic"
		Invoke-Item $url
	}
	else {
		Invoke-Item $htm
	}
}

# open help by path and topic
if ($FileName) {
	$FileName = Resolve-Path $FileName
	Show
	return
}

# from editor?
$editor = $Far.Editor
if (!$editor -or $editor.FileName -notmatch '\.(text|md|markdown)$') {
	Show-FarMessage "Run it with parameters or a markdown file in the editor."
	return
}

# commit
$editor.Save()

# open a file from editor with the current topic
for($e = $editor.Caret.Y; $e -ge 0; --$e) {
	if ($editor[$e].Text -match '^#+.*{#([a-zA-Z][a-zA-Z0-9_\-:.]*)}') {
		$Topic = $matches[1]
		break
	}
}
$FileName = $editor.FileName
Show
