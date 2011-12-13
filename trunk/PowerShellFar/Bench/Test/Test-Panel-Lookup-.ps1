
<#
.Synopsis
	Test panel lookup actions
	Author: Roman Kuzmin

.Description
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

# simple way to get a new object with properties
$myObject = 1 | Select-Object Any, Item, Process

# create a panel to show and change the object properties
$Panel = New-Object PowerShellFar.MemberPanel $myObject
$Panel.Title = 'Press [Enter] on properties'

# Lookup
$Panel.AddLookup(@{
	### property 'Any' has to be a value from the fixed set
	'Any' = {
		param($0, $_)
		$Panel = New-Object PowerShellFar.ObjectPanel -Property @{
			Title = 'Press [Enter] on a value'
			ViewMode = 'Medium'
			Lookup = {
				param($0, $_)
				$0.Parent.Value.Any = $_.File.Data
			}
		}
		$Panel.AddObjects(@(
			'String1'
			'String2'
			3.1415
			2007
		))
		$Panel.OpenChild($0)
	}
	### property 'Item' has to be a selected file or folder
	'Item' = {
		param($0, $_)
		$Panel = New-Object PowerShellFar.ObjectPanel -Property @{
			Title = 'Press [Enter] on an item'
			Lookup = {
				param($0, $_)
				$0.Parent.Value.Item = $_.File.Data
			}
		}
		$Panel.AddObjects((Get-ChildItem $env:FARHOME))
		$Panel.OpenChild($0)
	}
	### property 'Process' has to be a selected process
	'Process' = {
		param($0, $_)
		$Panel = New-Object PowerShellFar.ObjectPanel -Property @{
			Title = 'Press [Enter] on a process'
			Lookup = {
				param($0, $_)
				$0.Parent.Value.Process = $_.File.Data.Name
			}
		}
		$Panel.AddObjects((Get-Process))
		$Panel.OpenChild($0)
	}
})

# return?
if ($NoShow) {
	return $Panel
}

# Go!
$Panel.Open()
