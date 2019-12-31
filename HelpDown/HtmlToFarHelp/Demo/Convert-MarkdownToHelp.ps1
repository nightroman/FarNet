<#
.Synopsis
	Demo script for converting Markdown to HTM and HLF.
#>

[CmdletBinding()]
param(
	[string]$Path = "$PSScriptRoot\README.md"
)

$ErrorActionPreference = 1

# HTML title is the file name
$title = [System.IO.Path]::GetFileNameWithoutExtension($Path)

# output HTML has the same name with extension .htm
$htm = [System.IO.Path]::ChangeExtension($Path, '.htm')

# output HLF has the same name with extension .hlf
$hlf = [System.IO.Path]::ChangeExtension($Path, '.hlf')

# convert Markdown to HTML
pandoc $Path --output=$htm --from=gfm --wrap=preserve --standalone --metadata=pagetitle:$title
if ($LASTEXITCODE) {throw "pandoc failed."}

# convert HTML to HLF
HtmlToFarHelp from=$htm to=$hlf
if ($LASTEXITCODE) {throw "HtmlToFarHelp failed."}
