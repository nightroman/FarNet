
<#
.Synopsis
	Tests FarNet.Tools.SubsetForm.
	Author: Roman Kuzmin

.Description
	Invoke this script and play with the form by selecting, deselecting, and
	sorting selected items.
#>

# add the tools
Add-Type -Path $env:FARHOME\FarNet\FarNet.Tools.dll

# create and setup the form
$form = New-Object FarNet.Tools.SubsetForm
$form.Title = "TEST SUBSET FORM"

# set items to select from
$form.Items = 0..9

# set some preselected indexes
$form.Indexes = @(0, 2, 4, 6)

# set optional converter to strings
$form.ItemToString = {param($1) "Item $1"}

# show
if ($form.Show()) {
	# get results
	$form.Indexes
}
