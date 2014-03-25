
<#
.Synopsis
	Invokes a file from the current editor.
	Author: Roman Kuzmin

.Description
	Saves a file in the editor and invokes it depending on the file type.

	If a file is *-.ps1 it is executed in the current PowerShell session by
	$Psf.InvokeScriptFromEditor() with $ErrorActionPreference = 'Inquire'

	If a file is *.build|test.ps1 then the current task is invoked by
	Invoke-Build.ps1 (https://github.com/nightroman/Invoke-Build)

	If a file is *.ps1 it is invoked by PowerShell.exe outside of Far. When it
	is done you can watch the console output and close the window by [Enter].
	If it fails the PowerShell is not exited, but stopped, you may work in
	failed PowerShell session to investigate problems just in place.

	Markdown files are opened by Show-Markdown-.ps1

	*.*proj files are processed by Start-MSBuild-.ps1

	If a file is .bat, .cmd, .pl, .mak, makefile, etc. then some typical action
	is executed, mostly as demo, use your own invocation for practical tasks.

	As for the other files, the script simply calls Invoke-Item for them, i.e.
	starts a program associated with a file type.
#>

# Save the file and get the path
$editor = $Psf.Editor()
$path = $editor.FileName

# Commit
$editor.Save()

# Extension
$ext = [IO.Path]::GetExtension($path)

### PowerShell.exe
if ($ext -eq '.ps1') {
	if ($path -match '\.(?:build|test)\.ps1$') {
		$task = '.'
		for($e = $editor.Caret.Y; $e -ge 0; --$e) {
			if (($text = $editor[$e].Text) -match '^\s*task\s+') {
				$null, $t2, $null = [System.Management.Automation.PSParser]::Tokenize($text, [ref]$null)
				Assert-Far ($t2 -and ($t2.Type -eq 'CommandArgument' -or $t2.Type -eq 'String' -or $t2.Type -eq 'Number')) `
				"Cannot get task name at line '$text'." Invoke-Editor-.ps1
				$task = $t2.Content
				break
			}
		}
		$arg = "-NoExit Invoke-Build '$($task.Replace("'", "''").Replace('"', '\"'))' '$($path.Replace("'", "''"))'"
	}
	else {
		$arg = "-NoExit . '$($path.Replace("'", "''"))'"
	}
	Start-Process PowerShell.exe $arg
	return
}

### MSBuild
if ($ext -like '.*proj') {
	Start-MSBuild-.ps1 $path
	return
}

$arg = "`"$path`""

### Markdown
if ('.text', '.md', '.markdown' -contains $ext) {
	Show-Markdown-.ps1
}

### Cmd
elseif ('.bat', '.cmd' -contains $ext) {
	cmd /c start cmd /k $arg
}

### Perl
elseif ('.pl' -eq $ext) {
	cmd /c start cmd /k perl $arg
}

### Makefile
elseif ('.mak' -eq $ext -or [IO.Path]::GetFileName($path) -eq 'makefile') {
	cmd /c start cmd /k nmake /f $arg /nologo
}

### Others
else {
	Invoke-Item -LiteralPath $path
}
