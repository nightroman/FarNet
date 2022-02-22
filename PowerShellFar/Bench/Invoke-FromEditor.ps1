<#
.Synopsis
	Invokes a file from the current editor.
	Author: Roman Kuzmin

.Description
	Saves the editor file and invokes it depending on its type.

	*.build.ps1, *.test.ps1 ~ the current task is invoked by
	Invoke-Build (https://github.com/nightroman/Invoke-Build)
	in a new console. Note that built-in [F5] invokes tasks
	in the Far Manager console and PowerShellFar session.

	*.Rule.ps1 are invoked by Assert-PSRule (https://github.com/microsoft/PSRule).
	The input is provided by your command Get-PSRuleInput, e.g. script in the path.

	*.ps1 are invoked by powershell.exe

	Markdown files are opened by Show-Markdown-.ps1

	*.bat, *.cmd are invoked by cmd.exe

	Other files are invoked by Invoke-Item.
#>

# save, get normalized path and extension
$editor = $Psf.Editor()
$editor.Save()
$path = [System.IO.Path]::GetFullPath($editor.FileName)
$ext = [System.IO.Path]::GetExtension($path)

### PowerShell
if ($ext -eq '.ps1') {
	if ($path -match '\.(?:build|test)\.ps1$') {
		# Invoke-Build
		$task = '.'
		$line = $editor.Caret.Y + 1
		foreach($t in (Invoke-Build ?? $path).Values) {
			if ($t.InvocationInfo.ScriptName -ne $path) {continue}
			if ($t.InvocationInfo.ScriptLineNumber -gt $line) {break}
			$task = $t.Name
		}
		$arg = "-NoExit -NoProfile -ExecutionPolicy Bypass -Command Invoke-Build '{0}' '{1}'" -f @(
			$task.Replace("'", "''").Replace('"', '\"')
			$path.Replace("'", "''")
		)
	}
	elseif ($path -like '*.Rule.ps1') {
		# PSRule
		$arg = "-NoExit -NoProfile -ExecutionPolicy Bypass -Command `"Get-PSRuleInput | Assert-PSRule -Path '{0}'`"" -f (
			$path.Replace("'", "''")
		)
	}
	else {
		# PS script
		$arg = "-NoExit -NoProfile -ExecutionPolicy Bypass -Command . '{0}'" -f (
			$path.Replace("'", "''")
		)
	}
	Start-Process powershell.exe $arg
	return
}

### Markdown
if ('.text', '.md', '.markdown' -contains $ext) {
	Show-Markdown-.ps1
	return
}

### Batch
if ('.bat', '.cmd' -contains $ext) {
	cmd.exe /c start cmd /k "`"$path`""
	return
}

### Others
Invoke-Item -LiteralPath $path
