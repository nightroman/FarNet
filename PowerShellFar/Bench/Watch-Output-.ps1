
<#
.SYNOPSIS
	Watches commands output repeatedly in the editor.
	Author: Roman Kuzmin

.DESCRIPTION
	It opens an editor with a temp file and periodically, when idle, updates it
	with output of a command. The command is a PowerShell script block; it may
	contain several commands and call external console applications (the only
	restriction: commands and applications should not directly operate on
	console).

	Notes:
	- you can open several "Watch-Output-" editors for several commands;
	- commands are invoked only for the active editor.

.LINK
	# Example of complex output, long lines and many lines
	Test-Watch-Output-.ps1
#>

param
(
	[scriptblock]
	# Command which output is being watched.
	$Command = { Get-Process },

	[string]
	# Editor window title; default is the command text.
	$Title = $Command,

	[double]
	# Interval in seconds. Default: 1.0
	$Seconds = 1.0
)

$editor = New-FarEditor $Far.TempName() -Title $Title -DisableHistory -Host $Command
$editor.add_Idled([FarNet.IdledHandler]::Create($Seconds, {
	# keep the frame because Undo will change it
	$frame = $this.Frame
	# undo to avoid memory "leak"
	$this.Undo()
	# invoke the script block
	$this.SetText((& $this.Host | Out-String))
	# restore the old frame
	$this.Frame = $frame
	# show the changes
	$this.Redraw()
}))
$editor.Open()
