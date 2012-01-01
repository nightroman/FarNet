
<#
.Synopsis
	Starts Far and manages start and exit current directory.
	Author: Roman Kuzmin

.Description
	It starts Far Manager in the current PowerShell console. On exit it sets
	the current directory and provider location to the last current panel path.

	All arguments are passed in Far. Do not confuse PowerShell: use '/', not
	'-' for switches. Mind other PowerShell parsing rules different from Cmd.

.Example
	# Use alias 'far'; we still can call Far directly by 'far.exe'
	Set-Alias far Start-Far

	# Start Far with the current directory on the active and passive panels
	far . .

	# Edit a file
	far /e readme.txt
#>

# sync location
[Environment]::CurrentDirectory = (Get-Location -PSProvider FileSystem).ProviderPath

# run
far.exe /w $args

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
