<#
.Synopsis
	Invokes a file from the current editor.
	Author: Roman Kuzmin

.Description
	Saves the editor file and invokes it depending on its type.

	For PowerShell scripts: if $env:pwsh is defined then it is used as the executable.
	Normally it is pwsh.exe, installed or downloaded from their repo releases page.
	Otherwise powershell.exe is used.

	*.build.ps1, *.test.ps1
	The current task is invoked in a new console by Invoke-Build (https://github.com/nightroman/Invoke-Build).
	Note that built-in [F5] invokes in the Far Manager console and PowerShellFar session.

	*.Rule.ps1
	The current rule is invoked in a new console by Assert-PSRule (https://github.com/microsoft/PSRule).
	Input is provided by your own command Get-PSRuleInput, e.g. a script in the path.

	*.ps1
	Other scripts are invoked by powershell in a new console.
	Note that built-in [F5] invokes in the PowerShellFar session.

	*.md, *.markdown, *.text
	Markdown files are opened by Show-Markdown-.ps1

	*.bat, *.cmd
	Batch file are invoked in a new console by cmd.

	Other files are invoked by Invoke-Item.
#>

trap {
	Show-FarMessage $_ Invoke-FromEditor -LeftAligned
	exit
}

# save, get normalized path and extension
$editor = $Psf.Editor()
$editor.Save()
$path = [System.IO.Path]::GetFullPath($editor.FileName)
$ext = [System.IO.Path]::GetExtension($path)

### PowerShell or $env:pwsh
if ($ext -eq '.ps1') {
	if ($path -match '\.(?:build|test)\.ps1$') {
		# Invoke-Build
		try {
			$tasks = (Invoke-Build ?? $path).Values
		}
		catch {
			throw "Cannot get tasks from '$path':`n$_"
		}
		$name = '.'
		$line = $editor.Caret.Y + 1
		foreach($_ in $tasks) {
			if ($_.InvocationInfo.ScriptName -ne $path) {continue}
			if ($_.InvocationInfo.ScriptLineNumber -gt $line) {break}
			$name = $_.Name
		}
		$arg = "-NoExit -NoProfile -ExecutionPolicy Bypass -Command Invoke-Build '{0}' '{1}'" -f @(
			$name.Replace("'", "''").Replace('"', '\"')
			$path.Replace("'", "''")
		)
	}
	elseif ($path -like '*.Rule.ps1') {
		# PSRule
		try {
			$rules = @(Get-PSRule $path)
		}
		catch {
			throw "Cannot get rules from '$path':`n$_"
		}
		if (!$rules) {
			return
		}
		$name = $rules[0].Name
		$line = $editor.Caret.Y + 1
		foreach($_ in $rules) {
			if ($_.Extent.Line -gt $line) {break}
			$name = $_.Name
		}
		$arg = "-NoExit -NoProfile -ExecutionPolicy Bypass -Command `"Get-PSRuleInput | Assert-PSRule -Path '{0}' -Name '{1}'`"" -f @(
			$path.Replace("'", "''")
			$name.Replace("'", "''").Replace('"', '\"')
		)
	}
	else {
		# PS script
		$arg = "-NoExit -NoProfile -ExecutionPolicy Bypass -Command . '{0}'" -f (
			$path.Replace("'", "''")
		)
	}

	if (!($pwsh = $env:pwsh)) {
		$pwsh = 'powershell.exe'
	}

	Start-Process $pwsh $arg
	return
}

### Markdown
if ('.md', '.markdown', '.text' -contains $ext) {
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
