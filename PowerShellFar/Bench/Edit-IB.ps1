<#
.Synopsis
	Finds and edits selected Invoke-Build task.
	Author: Roman Kuzmin

.Description
	Invoke-Build should be available, either module or script.
	How to get it: https://github.com/nightroman/Invoke-Build

	Invoke-Build looks for build scripts in the current and parent folders.
	So you may call this script from any child folder of a build script root.

	This script is usually called by FarNet.PowerShellFar. But you may call it
	from PowerShell consoles, too. Then the script opens Far Manager and calls
	itself. This requires Far.exe, Start-Far.ps1, and Edit-IB.ps1 in the path.

	If the build script is found then the list of its tasks is shown, including
	`<new>`. Select a task. This opens the editor at the selected or added task.

	If the build script is not found then a new `.build.ps1` is opened in the
	editor and a new task is added. You may exit without saving or edit and
	save with the default or different script name.

	Tips:
	- In the build script editor press [F5] to run the current task.
	- After looking at its output press [Esc] to return the editor.
#>

$ErrorActionPreference=1
trap {Write-Error $_}

### not far host? start new far
if ($Host.Name -ne 'FarHost') {
	Start-Far.ps1 ps:Edit-IB.ps1
	return
}

### dot-source Invoke-Build
try {
	. Invoke-Build
}
catch {
	throw "Cannot use Invoke-Build: $_"
}

### find file, select task
$file = Get-BuildFile $PWD
if ($file) {
	$fileName = [IO.Path]::GetFileName($file)
	$task = $(
		@{Name = $fileName}
		(Invoke-Build ?? $file).Values
		@{Name = '<new>'}
	) |
	Out-FarList -Title Tasks -Text {$_.Name}
	if (!$task) {
		return
	}
}
else {
	$fileName = $null
	$file = "$PWD\.build.ps1"
	$task = @{Name = '<new>'}
}

### just edit file
if ($task.Name -eq $fileName) {
	Open-FarEditor $file
	return
}

### edit existing task
if ($task.Name -ne '<new>') {
	$ii = $task.InvocationInfo
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
