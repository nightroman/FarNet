<#
.Synopsis
	Opens Invoke-Build scripts in the editor.
	Author: Roman Kuzmin

.Description
	Invoke-Build should be available, either module or script.
	How to get it: https://github.com/nightroman/Invoke-Build

	You may call this script from any child folder of a build script.

	This script is usually called by PowerShellFar. But you may call it from
	a console, too. Then the script opens Far Manager and calls itself. This
	requires Far.exe, Start-Far.ps1, and Edit-IB.ps1 in the path.

	If the build script is found then the list of its tasks is shown, including
	*new*. Select a task. This opens the editor at the selected or added task.

	If the build script is not found then a new `.build.ps1` is opened in the
	editor and a new task is added. You may exit without saving or edit and
	save with the default or different script name.

	Tips:
	- In the build script editor press [F5] to run the current task.
	- After looking at the output press [Esc] to return to the editor.

.Parameter Task
		The optional selected task to edit right away.
#>

[CmdletBinding()]
param(
	[string]$Task
)

$ErrorActionPreference = 1
trap { $PSCmdlet.ThrowTerminatingError($_) }

### not far host? start new far
if ($Host.Name -ne 'FarHost') {
	return Start-Far.ps1 ($Task ? "ps:Edit-IB.ps1 $Task" : 'ps:Edit-IB.ps1')
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
	$items = $(
		@{Name = $fileName}
		(Invoke-Build ?? $file).Values
		@{Name = '*new*'}
	)

	# input task?
	$item = $null
	if ($_Task) {
		$item = $items.Where({$_.Name -eq $_Task})
	}

	# select item
	if (!$item) {
		$item = $items | Out-FarList -Title Tasks -Text {$_.Name}
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
	Open-FarEditor $file
	return
}

### edit existing task
if ($item.Name -ne '*new*') {
	$ii = $item.InvocationInfo
	Open-FarEditor $file -LineNumber $ii.ScriptLineNumber
	return
}

### edit a new task
$editor = $Far.CreateEditor()
$editor.FileName = $file
$editor.add_Opened({
	$this.GoToEnd($true)
	$this.InsertText("`r`ntask  {`r`n}`r`n")
	$this.GoTo(5, $this.Count - 3)
})
$editor.Open()
