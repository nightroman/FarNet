<#
.Synopsis
	Job: remove the specified provider items.
	Author: Roman Kuzmin

.Description
	Removing large directories may be very time consuming. This script runs
	the job in the background, so that Far is not blocked during this time.

	Input items to be removed are any provider items, e.g. results of Get-Item
	or Get-ChildItem commands. In PowerShellFar there are more commands getting
	items from panels: Get-FarItem, Get-FarItem -Selected, ...

.Parameter Item
		Specifies the items to remove.

.Example
	># Remove selected items on the active panel
	Job-RemoveItem- (Get-FarItem -Selected)
#>

param(
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
$jobName = "Remove: $item"
if (0 -ne (Show-FarMessage $jobName -Buttons "YesNo")) {
	return
}

# start the job
Start-FarJob -Name:$jobName -Parameters:$Item -KeepSeconds:15 {
	$args | Remove-Item -Force -Recurse -ErrorAction Continue
}
