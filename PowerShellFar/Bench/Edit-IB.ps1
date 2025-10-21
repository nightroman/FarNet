<#
.Synopsis
	Opens Invoke-Build scripts in the editor.
	Author: Roman Kuzmin

.Description
	Invoke-Build should be available, either module or script.
	How to get it: https://github.com/nightroman/Invoke-Build

	You may call this script from any child folder of a build script.

	This script runs in FarHost. If it runs in console, it opens Far Manager.
	This requires "Far.exe", "Start-Far.ps1", and "Edit-IB.ps1" in the path.

	If the build script is not found then a new "1.build.ps1" is opened in the
	editor with a new task added. You may exit editor without saving anything.

	The menu is shown: the file, its tasks, <new task>. Select an item to
	open in the editor. The task is opened at its location. A new task is
	added to the end.

	Tips:
	- In the build script editor press [F5] to run the current task.
	- After looking at the output, press [Esc] to return to the editor.

.Parameter Task
		The task name to edit right away.

.Example
	>
	# Find the build script and edit its task "test"

	ps: Edit-IB.ps1 test

.Example
	>
	# The same using Lua macro command "ib:"

		ib: test

	Macro command "ib:"

		CommandLine {
		  prefixes = "ib";
		  description = "Edit-IB.ps1";
		  action = function(prefix, text)
		    Plugin.SyncCall("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "vps:Edit-IB.ps1 " .. text)
		  end;
		}
#>

[CmdletBinding()]
param(
	[string]$Task
)

#requires -Version 7.4
Set-StrictMode -Version 3
$ErrorActionPreference=1; trap {$PSCmdlet.ThrowTerminatingError($_)}; if ($Host.Name -ne 'FarHost') {
	return Start-Far.ps1 "vps:Edit-IB.ps1 $Task"
}

function __new_task {
	$editor = $Far.CreateEditor()
	$editor.FileName = $BuildFile
	$editor.add_Opened({
		$this.BeginUndo()
		$this.GoToEnd($true)
		$this.InsertText("`rtask  {`r}`r")
		$this.GoTo(5, $this.Count - 3)
		$this.EndUndo()
	})
	$editor.Open()
}

function __open_the_task {
	$ii = $TheTask.InvocationInfo
	Open-FarEditor $ii.ScriptName -LineNumber $ii.ScriptLineNumber
}

function __item_open_file {
	$item = [FarNet.SetItem]::new()
	$item.Text = [System.IO.Path]::GetFileName($BuildFile)
	$item.Click = {
		Open-FarEditor $BuildFile
	}
	$item
}

function __item_new_task {
	$item = [FarNet.SetItem]::new()
	$item.Text = '<new task>'
	$item.Click = ${function:__new_task}
	$item
}

### dot-source
$_Task = $Task
try { . Invoke-Build }
catch { throw "Invoke-Build: $_" }

### find build
$BuildFile = Get-BuildFile $PWD

### not found, new task
if (!$BuildFile) {
	$BuildFile = Join-Path $PWD 1.build.ps1
	return __new_task
}

### get tasks
$all = Invoke-Build ?? $BuildFile

### open given task
if ($_Task -and ($TheTask = $all[$_Task])) {
	return __open_the_task
}

### menu
$TheTask = @(
	__item_open_file
	$all.Values
	__item_new_task
) |
Out-FarList -Title Tasks -Text {$_.Name}

### open selected task
if ($TheTask) {
	return __open_the_task
}
