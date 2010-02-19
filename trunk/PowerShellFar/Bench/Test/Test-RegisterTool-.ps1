
<#
.SYNOPSIS
	Test a tool handler.
	Author: Roman Kuzmin

.DESCRIPTION
	Steps:
	- invoke this script to register the tool;
	- try it in F11, disk and config menus;
	- invoke this script again to unregister the tool.
#>

if (!$TestTool) {

	# install the handler
	$global:TestTool = {&{
		Show-FarMessage ("Hello from " + $_.From)
	}}

	# register the handler
	$attr = New-Object FarNet.ModuleToolAttribute -Property @{ Name = "PSF test tool"; Options = "AllAreas" }
	$Far.RegisterTool($null, "f2a1fc38-35d0-4546-b67c-13d8bb93fa2e", $TestTool, $attr)
	Show-FarMessage "Test tool is registered. Try it in [F11] menus, for example, right now."
}
else {
	# unregister and uninstall the handler
	$Far.UnregisterTool($TestTool)
	Remove-Variable TestTool -Scope Global
	Show-FarMessage "Test tool is unregistered"
}
