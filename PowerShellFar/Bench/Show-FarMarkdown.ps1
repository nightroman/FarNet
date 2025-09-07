<#
.Synopsis
	Shows markdown as HTML or HLF.
	Author: Roman Kuzmin

.Description
	Requires executables in the path:
	- Pandoc for converting Markdown to HTML
	- HtmlToFarHelp for converting HTML to HLF

	This script opens the current topic from the editor as HTML in the
	default browser (example: Invoke-FromEditor.ps1) or as HLF in help
	viewer (example: Profile-Editor.ps1).

	$env:Markdown
	-------------

	Specifies Markdown format for Pandoc.
	Default: .text files ~ `markdown_phpextra`, others ~ `gfm`.

	$env:MarkdownCss
	----------------

	Specifies CSS file or URL for HTML.

		GitHub styles:
		https://gist.github.com/dashed/6714393
		https://github.com/wklchris/markdown-css-for-pandoc

.Parameter FileName
		Specifies the Markdown file.
		Default: the current editor file.

.Parameter Topic
		Specifies the topic.

.Parameter Help
		Tells to create and open HLF.
#>

[CmdletBinding()]
param(
	[string]$FileName
	,
	[string]$Topic
	,
	[switch]$Help
)

# Gets topic ids from verbose output.
function Convert-HelpTopic($Lines) {
	switch -Regex ($Lines) {'^HeadingId=(.*)$' {$Matches[1]}}
}

# Finds current editor topic to open.
function Find-EditorTopic([string[]]$Topics) {
	$r = ''

	# find the current topic
	if ($Format -eq 'markdown_phpextra') {
		# manual heading identifiers
		for($i = $editor.Caret.Y; $i -ge 0; --$i) {
			if ($editor[$i].Text -match '^#{1,6}\s.*{#([a-zA-Z][a-zA-Z0-9_\-:.]*)}') {
				$_ = $matches[1]
				if (!$Topics -or ($Topics -ccontains $_)) {
					$r = $_
					break
				}
			}
		}
	}
	else {
		# generated heading identifiers
		for($i = $editor.Caret.Y; $i -ge 0; --$i) {
			if ($editor[$i].Text -match '^#{1,6}\s+(.*)') {
				$_ = ($matches[1] -replace '\s+', '-' -replace '[^\w\-]').ToLower()
				if (!$Topics -or ($Topics -ccontains $_)) {
					$r = $_
					break
				}
			}
		}
	}

	# adjust the first
	if ($Help -and $r -and $Topics -and ($r -ceq $Topics[0])) {
		'Contents'
	}
	else {
		$r
	}
}

#requires -Version 7.4
$ErrorActionPreference = 1; trap {$PSCmdlet.ThrowTerminatingError($_)}; if ($Host.Name -ne 'FarHost') {throw 'Requires FarHost.'}

### get file name and editor
if ($FileName) {
	$FileName = $PSCmdlet.GetUnresolvedProviderPathFromPSPath($FileName)
	if (![System.IO.File]::Exists($FileName)) {
		throw "Missing file '$FileName'."
	}
	$Editor = $null
	$Topic = $null
}
else {
	$Editor = $Far.Editor
	if (!$Editor) {
		throw "Specify FileName or invoke from editor with Markdown file."
	}
	$Editor.Save()
	$FileName = $Editor.FileName
	$Topic = Find-EditorTopic
}

### get format
if ($env:markdown) {
	$Format = $env:markdown
}
elseif ([System.IO.Path]::GetExtension($FileName) -eq '.text') {
	$Format = 'markdown_phpextra'
}
else {
	$Format = 'gfm'
}

### convert to HTML
$html = "$env:TEMP\Show-FarMarkdown.html"
$param = $(
	$FileName
	"--output=$html"
	"--from=$Format"
	if ($Help) {
		'--syntax-highlighting=none'
	}
	else {
		$name = [System.IO.Path]::GetFileNameWithoutExtension($FileName)
		$root = [System.IO.Path]::GetDirectoryName($FileName)
		$more = [System.IO.Path]::GetFileName($root)
		'--standalone'
		'--embed-resources'
		"--resource-path=$root"
		"--metadata=pagetitle:$name - $more"

		# topic to open
		if ($Topic) {
			"--variable=include-after=<script>window.location.hash = '$Topic'</script>"
		}

		# CSS, URI for Firefox
		if ($css = $env:MarkdownCss) {
			"--css=$(([Uri]$css).AbsoluteUri)"
		}
	}
)
$err = pandoc.exe $param 2>&1
if ($LastExitCode) {throw "pandoc.exe failed:`n$err"}

### show
if ($Help) {
	$hlf = "$env:TEMP\Show-FarMarkdown.hlf"

	# `verbose=true` gets topics + maybe error
	$topicIds = HtmlToFarHelp.exe "from=$html" "to=$hlf" verbose=true 2>&1
	if ($LastExitCode) {
		throw "HtmlToFarHelp failed:`n$topicIds"
	}

	if ($Editor) {
		$Topic = Find-EditorTopic (Convert-HelpTopic $topicIds)
	}

	$Far.ShowHelp($hlf, $Topic, 'File')
}
else {
	Invoke-Item $html
}
