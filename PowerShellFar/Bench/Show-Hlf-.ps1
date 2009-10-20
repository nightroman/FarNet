
<#
.SYNOPSIS
	Shows .hlf help file
	Author: Roman Kuzmin

.DESCRIPTION
	It is similar to the plugin HlfViewer.

	"What's it for if there is HlfViewer?" Well, first of all, the script was
	created to test IFar.ShowHelp(). As it is created and one uses PowerShell
	anyway, why not to use Show-Hlf-.ps1? Besides, this way is configurable
	without any dialogs just in the script: here you can do what you want.

.EXAMPLE
	# Show Far help
	Show-Hlf- "$env:FARHOME\FarEng.hlf"

	# Show PowerShellFar help topic Cmdlets
	Show-Hlf- "$($Psf.AppHome)\PowerShellFar.hlf" Cmdlets

.LINK
	Profile-Editor-.ps1 - how to call it for .hlf file in editor by F1
	(this is for demo, another way is to use the user menu and a macro).

.PARAMETER FileName
		.hlf file path; if none then a file is taken from the editor.
.PARAMETER Topic
		Help topic in a file.
#>

param
(
	[string]$FileName,
	[string]$Topic
)
if ($args) { $Far.Msg("Unknown parameters: $args"); return }

# open help by path and topic
if ($FileName) {
	$Far.ShowHelp((Resolve-Path $FileName), $Topic, 'File')
	return
}

# from editor?
$editor = $Far.Editor
if (!$editor -or $editor.FileName -notlike '*.hlf') {
	return Show-FarMsg "Run it with parameters or for .hlf file in the editor."
}

# open a file from editor
if ($editor.IsModified) { $editor.Save() }
$editor.Begin()
for($e = $editor.Cursor.Y; $e -ge 0; --$e) {
	$text = $editor.Lines[$e].Text
	if ($text.StartsWith('@')) {
		$Topic = $text.Substring(1)
		break
	}
}
$editor.End()
$Far.ShowHelp($editor.FileName, $Topic, 'File')
