
<#
.SYNOPSIS
	Shows how to register commands dynamically.
	Author: Roman Kuzmin

.DESCRIPTION
	Steps:
	- invoke this script to register the command;
	- type in the command line any text with prefix test:
	- invoke this script again to unregister the command.
#>

# try to find this command already registered in order to test Unregister();
# you should not check this, say, if you register commands from the profile
$command = $Far.GetModuleCommand("053a9a98-db98-415c-9c80-88eee2f336ae")

# not found, register
if (!$command) {
	$command = $Psf.Manager.RegisterModuleCommand(
		"053a9a98-db98-415c-9c80-88eee2f336ae",
		(New-Object FarNet.ModuleCommandAttribute -Property @{ Name = "PSF test command"; Prefix = "test" }),
		{ Show-FarMessage $_.Command "PSF test command" }
	)
	Show-FarMessage "Command 'test' is registered, type:`n$($command.Prefix):<text>[Enter]"
}
# found, unregister (just for testing, normally this is unlikely needed)
else {
	$command.Unregister()
	Show-FarMessage "Test command is unregistered"
}
