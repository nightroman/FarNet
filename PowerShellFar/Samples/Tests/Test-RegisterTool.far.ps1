<#
.Synopsis
	Shows how to register tools dynamically.

.Description
	Steps:
	- invoke this script to register the tool
	- try it in F11 menus
#>

Register-FarTool -Options F11Menus 'PSF test tool' f2a1fc38-35d0-4546-b67c-13d8bb93fa2e {
	Show-FarMessage "Hello from $($_.From)"
}

Show-FarMessage "Tool 'PSF test tool' is registered. Try it in F11 menus, e.g. now."
