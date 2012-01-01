
<#
.Synopsis
	Shows .hlf help file.
	Author: Roman Kuzmin

.Description
	It is similar to the plugin HlfViewer.

	"What's it for if there is HlfViewer?" Well, originally it was created to
	test IFar.ShowHelp(). Then, as far as it is created, why not to use it?
	Besides, this way is perhaps more flexible.

.Example
	# Show Far help
	Show-Hlf- "$env:FARHOME\FarEng.hlf"

	# Show PowerShellFar help topic Cmdlets
	Show-Hlf- "$($Psf.AppHome)\PowerShellFar.hlf" Cmdlets

.Link
	Profile-Editor-.ps1 - how to call it for .hlf file in editor by F1
	(this is for demo, a better way is to use the menu and a macro).
#>

param
(
	[string]
	# .hlf file path; if none then a file is taken from the editor.
	$FileName
	,
	[string]
	# Help topic in a file.
	$Topic
)
if ($args) { throw "Unknown parameters: $args" }

# open help by path and topic
if ($FileName) {
	$Far.ShowHelp((Resolve-Path $FileName), $Topic, 'File')
	return
}

# from editor?
$editor = $Far.Editor
if (!$editor -or $editor.FileName -notlike '*.hlf') {
	Show-FarMessage "Run it with parameters or .hlf file in the editor."
	return
}

# commit
$editor.Save()

# open a file from editor with the current topic
for($e = $editor.Caret.Y; $e -ge 0; --$e) {
	$text = $editor[$e].Text
	if ($text.StartsWith('@')) {
		$Topic = $text.Substring(1)
		break
	}
}
$Far.ShowHelp($editor.FileName, $Topic, 'File')
