
<#
.Synopsis
	Shows markdown as HTML or Far help file.
	Author: Roman Kuzmin

.Description
	Requires [HelpDown](http://code.google.com/p/farnet/downloads/list).
	Copy these files to the same directory which included in %PATH%:
	- MarkdownToHtml.exe
	- HtmlToFarHelp.exe
	- MarkdownDeep.dll

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

param
(
	[Parameter()][string]$FileName,
	[string]$Topic,
	[switch]$Help
)

function Show {
	$htm = "$env:TEMP\MarkdownToHtml.htm"
	MarkdownToHtml "From=$FileName" "To=$htm"
	if ($LastExitCode) {throw "MarkdownToHtml failed."}
	if ($Help) {
		$hlf = "$env:TEMP\HtmlToFarHelp.hlf"
		HtmlToFarHelp "From=$htm" "To=$hlf"
		if ($LastExitCode) {throw "HtmlToFarHelp failed."}
		$Far.ShowHelp($hlf, $Topic, 'File')
	}
	elseif ($Topic) {
		$url = "$env:TEMP\MarkdownToHtml.url"
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
