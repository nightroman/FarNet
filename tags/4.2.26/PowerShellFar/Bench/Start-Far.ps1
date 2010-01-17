
<#
.SYNOPSIS
	Starts Far in /rc mode and manages start and exit current directory.
	Author: Roman Kuzmin

.DESCRIPTION
	It is a helper for running Far in the current PowerShell console in /rc
	mode (restore console on exit). On exit from Far it sets the last panel
	directory current and updates the current PowerShell location.

	All arguments are passed in Far, but do not confuse PowerShell - use '/',
	not '-' for switches. Also mind other PowerShell rules different from Cmd.

.EXAMPLE
	# Let's use alias 'far'; we still can call Far directly by 'far.exe'
	Set-Alias far Start-Far

	# Start Far with the current directory on the active and passive panels
	far . .

	# Edit a file
	far /e readme.txt
#>

# sync location
[Environment]::CurrentDirectory = (Get-Location -PSProvider FileSystem).ProviderPath

# run in /rc mode
far.exe /rc $args

# sync location
if (($args -notcontains '/e') -and ($args -notcontains '/v')) {
	# get the last directory
	$p = Get-ItemProperty -Path 'HKCU:\Software\Far2\Panel\Left'
	if ($p.Focus) {
		$newPath = $p.Folder
	}
	else {
		$newPath = (Get-ItemProperty -Path 'HKCU:\Software\Far2\Panel\Right').Folder
	}

	# change both the current location and directory
	if ($newPath) {
		Set-Location -LiteralPath $newPath
		[Environment]::CurrentDirectory = $newPath
	}
}
