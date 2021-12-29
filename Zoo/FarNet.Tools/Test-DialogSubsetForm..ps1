<#
.Synopsis
	Tests subset form, covers Far crash.
#>

# add the tools
Add-Type -Path $env:FARHOME\FarNet\FarNet.Tools.dll

{{
	# create and setup the form
	$form = New-Object FarNet.Tools.SubsetForm

	# set items to select from
	$form.Items = 0..9

	# show
	$form.Show()
}}

# move the first item to the right
'Keys"Enter"'
{
	Assert-Far -Dialog
}

# go to the right, the last empty line is active, go to the first
'Keys"Tab Up"'

# move the item back
# Far 2.0.1267 crashes
'Keys"Enter"'
{
	Assert-Far -Dialog
}

# exit
'Keys"Esc"'
{
	Assert-Far ($Far.Window.Kind -ne 'Dialog')
}
