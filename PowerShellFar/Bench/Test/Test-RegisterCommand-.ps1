
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
	$attr = New-Object FarNet.ModuleCommandAttribute -Property @{ Name = "PSF test command"; Prefix = "test" }
	$Far.RegisterCommand($null, "053a9a98-db98-415c-9c80-88eee2f336ae", $TestCommand, $attr)
	Show-FarMessage "Command 'test' is registered, type:`n$($attr.Prefix):<text>[Enter]"
}
else {
	# unregister and uninstall the handler
	$Far.UnregisterCommand($TestCommand)
	Remove-Variable TestCommand -Scope Global
	Show-FarMessage "Test command is unregistered"
}
