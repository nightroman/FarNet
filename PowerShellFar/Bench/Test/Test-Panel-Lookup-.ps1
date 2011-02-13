
<#
.SYNOPSIS
	Test panel lookup actions
	Author: Roman Kuzmin

.DESCRIPTION
	Lookup panels are used by ListPanel when an object or a value has to be
	selected from a set represented by another, lookup panel.

	In this test try to press [Enter] on properties: at first to set a property
	value, then to change it. Note that in the latter case lookup panels should
	be normally shown with a current item properly positioned.

	[CtrlPgDn] can be used in both ListPanel and lookup panels. As usual it
	shows a list of object members (MemberPanel).
#>

param
(
	# used by Test-Panel-Menu-.ps1 to continue panel setup
	[switch]$NoShow
)

# simple way to get a new custom object with some properties
$myObject = 1 | Select-Object Any, Item, Process

# create a panel to show and change $myObject properties
$p = New-Object PowerShellFar.MemberPanel $myObject
$p.Title = 'Press [Enter] on properties'

# property 'Any' has to be a value from the fixed set
$p.AddLookup('Any', {&{
	$p = New-Object PowerShellFar.UserPanel
	$p.Title = 'Press [Enter] on a value'
	$p.ViewMode = 'Medium'
	$p.AddObjects(@(
		'String1'
		'String2'
		3.1415
		2007
	))
	$p.SetLookup({
		$this.Parent.Value.Any = $_.File.Data
	})
	$p.OpenChild($this)
}})

# property 'Item' has to be a selected file or folder
$p.AddLookup('Item', {&{
	$p = New-Object PowerShellFar.UserPanel
	$p.Title = 'Press [Enter] on an item'
	$p.AddObjects((Get-ChildItem $env:FARHOME))
	$p.SetLookup({
		$this.Parent.Value.Item = $_.File.Data
	})
	$p.OpenChild($this)
}})

# property 'Process' has to be a selected process
$p.AddLookup('Process', {&{
	$p = New-Object PowerShellFar.UserPanel
	$p.Title = 'Press [Enter] on a process'
	$p.AddObjects((Get-Process))
	$p.SetLookup({
		$this.Parent.Value.Process = $_.File.Data.Name
	})
	$p.OpenChild($this)
}})

# return?
if ($NoShow) {
	return $p
}

# Go!
$p.Open()
