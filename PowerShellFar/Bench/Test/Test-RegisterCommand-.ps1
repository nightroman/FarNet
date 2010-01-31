
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

if (!$TestCommand) {

	# install the handler
	$global:TestCommand = {&{
		Show-FarMessage $_.Command "PSF test command"
	}}

	# register the handler
	$null = $Far.RegisterCommand($null, "PSF test command", "test", $TestCommand)
	Show-FarMessage "Command 'test' is registered, type:`ntest:<your text>[Enter]"
}
else {
	# unregister and uninstall the handler
	$Far.UnregisterCommand($TestCommand)
	Remove-Variable TestCommand -Scope Global
	Show-FarMessage "Test command is unregistered"
}
