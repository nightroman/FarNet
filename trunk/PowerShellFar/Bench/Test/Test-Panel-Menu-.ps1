
<#
.Synopsis
	Test panel with demo menu called on [ShiftF3]
	Author: Roman Kuzmin

.Description
	Requires: Test-Panel-Lookup-.ps1 in the same directory.

	This script shows how to add items to the panel menu and process panel data
	by event handlers. You can add menu items to any panel derived from
	AnyPanel, no matter before or after Show(), i.e. it is OK to add menu items
	to already started panel.
#>

# create a panel (use another test panel)
$script = Join-Path (Split-Path $MyInvocation.MyCommand.Path) 'Test-Panel-Lookup-'
$Panel = & $script -NoShow

# called on creation of a menu; we may add our items
$Panel.add_MenuCreating({

	# to set data; mind Update and Redraw
	$_.Menu.Items.Add((New-FarItem -Text '1. Set default values' -Click {&{
		$v = $this.Value
		$v.Any = 'String 1'
		$v.Item = Get-Item "$env:FARHOME\Far.exe"
		$v.Process = Get-Process -Id $pid
		$this.Update($true)
		$this.Redraw()
	}}))

	# to process current and selected items
	$_.Menu.Items.Add((New-FarItem -Text '2. Get current and selected' -Click {&{
		$text = .{
			# current item
			$ci = $this.CurrentItem
			if ($ci) {
				"Current item: $($ci.Name)"
			}
			else {
				"No current item"
			}
			# selected items
			$si = @($this.SelectedItems)
			"Selected $($si.Count) items"
			$si | .{process{
				"$($_.Name) = $($_.Value)"
			}}
		} | Out-String
		$Far.Message($text)
	}}))

	# to check that handlers have global scope: variables $tmp1 and $tmp2 are global
	$_.Menu.Items.Add((New-FarItem -Text '3. Store current and selected' -Click {
		$tmp1 = $this.CurrentItem
		$tmp2 = @($this.SelectedItems)
	}))
})

# Go!
$Panel.Open()
