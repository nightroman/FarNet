<#
.Synopsis
	Shows how to register tools dynamically.

.Description
	Steps:
	- invoke this script to register the tool;
	- try it in F11, disk and config menus;
	- invoke this script again to unregister the tool.
#>

$tool = $Far.GetModuleAction("f2a1fc38-35d0-4546-b67c-13d8bb93fa2e")
if ($tool) {
	# unregister
	$tool.Unregister()
	Show-FarMessage "Test tool is unregistered"
}
else {
	# register
	$null = $Psf.Manager.RegisterTool(
		[FarNet.ModuleToolAttribute]@{Name="PSF test tool"; Options="AllAreas"; Id="f2a1fc38-35d0-4546-b67c-13d8bb93fa2e"},
		{ Show-FarMessage ("Hello from " + $_.From) }
	)
	Show-FarMessage "Test tool is registered. Try it in [F11] menus, for example, right now."
}
