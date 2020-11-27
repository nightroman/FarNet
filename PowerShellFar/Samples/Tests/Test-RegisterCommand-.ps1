
<#
.Synopsis
	Shows how to register commands dynamically.
	Author: Roman Kuzmin

.Description
	Steps:
	- invoke this script to register the command;
	- type in the command line: mycmd: <any text>
	- invoke this script again to unregister the command.
#>

# try to find this command already registered in order to test Unregister();
# you should not check this, say, if you register commands from the profile
$command = $Far.GetModuleAction("053a9a98-db98-415c-9c80-88eee2f336ae")

# not found, register
if (!$command) {
	$command = $Psf.Manager.RegisterModuleCommand(
		"053a9a98-db98-415c-9c80-88eee2f336ae",
		(New-Object FarNet.ModuleCommandAttribute -Property @{ Name = "PSF test command"; Prefix = "mycmd" }),
		{ Show-FarMessage ($_ | Format-List | Out-String).Trim() "PSF test command" -LeftAligned }
	)
	Show-FarMessage "Command is registered, type:`n$($command.Prefix):<text>[Enter]"
}
# found, unregister (just for testing, normally this is unlikely needed)
else {
	$command.Unregister()
	Show-FarMessage "Command is unregistered"
}
