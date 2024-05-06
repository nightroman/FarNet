<#
.Synopsis
	Shows .hlf help file.
	Author: Roman Kuzmin

.Description
	It is similar to the plugin HlfViewer.

.Parameter FileName
		Specifies the help file. If it is omitted then the file is taken from
		the current editor.

.Parameter Topic
		Specifies the help topic.

.Example
	># Show Far help
	Show-Hlf "$env:FARHOME\FarEng.hlf"

.Example
	># Show PowerShellFar help topic "profiles"
	Show-Hlf "$($Psf.AppHome)\PowerShellFar.hlf" profiles

.Link
	Profile-Editor.ps1 - how to call it for .hlf file in editor by F1
	(this is for demo, a better way is to use the menu and a macro).
#>

[CmdletBinding()]
param(
	[string]$FileName
	,
	[string]$Topic
)

#requires -Version 7.4
$ErrorActionPreference = 1; trap {$PSCmdlet.ThrowTerminatingError($_)}; if ($Host.Name -ne 'FarHost') {throw 'Requires FarHost.'}

# open by path and topic
if ($FileName) {
	$Far.ShowHelp((Resolve-Path $FileName), $Topic, 'File')
	return
}

# from editor?
$editor = $Far.Editor
if (!$editor -or $editor.FileName -notlike '*.hlf') {
	Show-FarMessage "Run it with FileName or .hlf in the editor."
	return
}

# commit
$editor.Save()

# open from editor with the current topic
for($i = $editor.Caret.Y; $i -ge 0; --$i) {
	if ($editor[$i].Text -match '^@(\w\S*)') {
		$Topic = $matches[1]
		break
	}
}
$Far.ShowHelp($editor.FileName, $Topic, 'File')
