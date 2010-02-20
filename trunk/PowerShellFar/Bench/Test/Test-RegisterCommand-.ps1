
<#
.SYNOPSIS
	Test a command handler.
	Author: Roman Kuzmin

.DESCRIPTION
	Steps:
	- invoke this script to register the command;
	- type in the command line any text with prefix test:
	- invoke this script again to unregister the command.
#>

$command = $Far.GetModuleCommand("053a9a98-db98-415c-9c80-88eee2f336ae")
if ($command) {
	# unregister
	$command.Unregister()
	Show-FarMessage "Test command is unregistered"
}
else {
	# register
	$command = $Psf.Manager.RegisterModuleCommand(
		"053a9a98-db98-415c-9c80-88eee2f336ae",
		(New-Object FarNet.ModuleCommandAttribute -Property @{ Name = "PSF test command"; Prefix = "test" }),
		{ Show-FarMessage $_.Command "PSF test command" }
	)
	Show-FarMessage "Command 'test' is registered, type:`n$($command.Prefix):<text>[Enter]"
}
