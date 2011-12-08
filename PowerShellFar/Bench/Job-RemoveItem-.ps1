
<#
.Synopsis
	Job: remove the specified provider item(s).
	Author: Roman Kuzmin

.Description
	Removing of large directories may be very time consuming. This script does
	the job in the background, so that Far is not blocked during this time.

	Input items to be removed are any PowerShell provider items, e.g. results
	of Get-Item or Get-ChildItem commands. In PowerShellFar there are more
	commands getting items from panels: Get-FarItem, Get-FarItem -Selected, ...

.Example
	# Remove selected items on the active panel (suitable for Far user menu)
	Job-RemoveItem- (Get-FarItem -Selected)
#>

param
(
	[Parameter(Mandatory=$true)] [object[]]
	# Provider items to be removed.
	$Item
)

# no items?
if (!$item) {
	Show-FarMessage "No items to remove."
	return
}

# ask
$jobName = "Remove: '$item'"
if (0 -ne (Show-FarMessage $jobName -Buttons "YesNo")) {
	return
}

# start the job
Start-FarJob -Name:$jobName -Parameters:$Item -KeepSeconds:15 {
	$args | Remove-Item -Force -Recurse -ErrorAction Continue
}
