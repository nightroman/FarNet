<#
.Synopsis
	Opens Invoke-Build scripts in the editor.
	Author: Roman Kuzmin

.Description
	Invoke-Build should be available, either module or script.
	How to get it: https://github.com/nightroman/Invoke-Build

	You may call this script from any child folder of a build script.

	This script runs in FarHost. But it may start from console, it opens Far
	Manager. This requires "Far.exe", "Start-Far.ps1", and "Edit-IB.ps1" in
	the path.

	If the build script is found then the list of its tasks is shown, including
	*new*. Select a task. This opens the editor at the selected or new task.

	If the build script is not found then a new ".build.ps1" is opened in the
	editor with a new task added.

	Tips:
	- In the build script editor press [F5] to run the current task.
	- After looking at the output press [Esc] to return to the editor.

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
$ErrorActionPreference = 1; trap {$PSCmdlet.ThrowTerminatingError($_)}; if ($Host.Name -ne 'FarHost') {
	return Start-Far.ps1 "vps:Edit-IB.ps1 $Task"
}

### dot-source Invoke-Build
$_Task = $Task
try {
	. Invoke-Build
}
catch {
	throw "Invoke-Build: $_"
}

### find file, select task
$file = Get-BuildFile $PWD
if ($file) {
	$fileName = [System.IO.Path]::GetFileName($file)

	# all items
	$list = @(
		@{Name = $fileName}
		(Invoke-Build ?? $file).Values
		@{Name = '*new*'}
	)

	# input task?
	$item = $null
	if ($_Task) {
		$item = $list.Where({$_.Name -eq $_Task})
	}

	# select item
	if (!$item) {
		$item = $list | Out-FarList -Title Tasks -Text {$_.Name}
		if (!$item) {
			return
		}
	}
}
else {
	$fileName = $null
	$file = "$PWD\.build.ps1"
	$item = @{Name = '*new*'}
}

### just edit file
if ($item.Name -eq $fileName) {
	return Open-FarEditor $file
}

### edit existing task
if ($item.Name -ne '*new*') {
	$ii = $item.InvocationInfo
	return Open-FarEditor $file -LineNumber $ii.ScriptLineNumber
}

### edit a new task
$editor = $Far.CreateEditor()
$editor.FileName = $file
$editor.add_Opened({
	$this.BeginUndo()
	$this.GoToEnd($true)
	$this.InsertText("`r`ntask  {`r`n}`r`n")
	$this.GoTo(5, $this.Count - 3)
	$this.EndUndo()
})
$editor.Open()
