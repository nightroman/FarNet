
<#
.Synopsis
	Test base features.
	Author: Roman Kuzmin

.Description
	It demonstrates:
	- menu
	- input box
	- message box
	- throwing an exception
	- using UI help for a script
#>

# get this script folder path
$MyFolder = Split-Path $MyInvocation.MyCommand.Path

# help folder + topic name
$Help = "<$MyFolder\>TestBase"

function Test-Exception
{
	throw "Test exception"
}

function Test-InputBox
{
	$b = $Far.CreateInputBox()
	$b.Title = "Test input box"
	$b.Prompt = "Enter text"
	$b.HelpTopic = $Help
	if ($b.Show()) {
		Show-FarMessage "Entered: $($b.Text)"
	}
	else {
		Show-FarMessage 'Canceled'
	}
}

function Test-Message
{
	$null = Show-FarMessage -LeftAligned -IsWarning -HelpTopic $Help -Choices 'Button&1', 'Button&2', 'Button&3' @'
Left aligned
multiline
warning message
with custom buttons
'@
}

function Test-Menu
{
	$menu = New-FarMenu -Title 'Title' -Bottom 'Bottom' -HelpTopic $Help -Selected 2 -Items @(
		New-FarItem 'Checked item' -Checked
		New-FarItem 'Separator' -IsSeparator
		New-FarItem 'Selected item'
	)
	$null = $menu.Show()
	Show-FarMessage $menu.Selected
}

function Test-All
{
	Test-InputBox
	Test-Message
	Test-Menu
}

# create menu
$menu = New-FarMenu 'Select a test' -Bottom (Get-Date) -HelpTopic $Help -AutoAssignHotkeys -Items @(
	New-FarItem 'All tests' { Test-All }
	New-FarItem -IsSeparator
	New-FarItem 'Exception' { Test-Exception }
	New-FarItem 'InputBox' { Test-InputBox }
	New-FarItem 'Message' { Test-Message }
	New-FarItem 'Menu' { Test-Menu }
)

# show menu
$choice = $menu.Show()
