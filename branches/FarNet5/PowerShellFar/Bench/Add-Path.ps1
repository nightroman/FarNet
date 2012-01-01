
<#
.Synopsis
	Adds a directory path to the current process path.
	Author: Roman Kuzmin

.Description
	It is a standard PowerShell script for any host. But in the first place it
	was designed for use in Far Manager to add the active panel path to paths.

.Example
	# Far user menu command to add the active path
	>: Add-Path $Far.CurrentDirectory #

	# Add the Bench scripts path, if not yet
	>: Add-Path "$($Psf.AppHome)\Bench"
#>

param
(
	# Path to be added to an environment variable.
	$Path = '.',

	# Environment variable name. Default: 'PATH'.
	$Name = 'PATH'
)

# normalize and check
$Path = [System.IO.Path]::GetFullPath($Path)
if (![System.IO.Directory]::Exists($Path)) {
	throw "Directory '$Path' doesn't exist."
}

# is it already added?
$var = [Environment]::GetEnvironmentVariable($Name)
$trimmed = $Path.TrimEnd('\')
foreach($dir in $var.Split(';')) {
	if ($dir.TrimEnd('\') -eq $trimmed) {
		return
	}
}

# add the path
[Environment]::SetEnvironmentVariable($Name, $Path + ';' + $var)
