
<#
.SYNOPSIS
	Tests subset forms (ISubsetForm).
	Author: Roman Kuzmin

.DESCRIPTION
	Invoke this script and play with the form by selecting, deselecting, and
	sorting selected items.
#>

# create and setup the form
$form = $Far.CreateSubsetForm()
$form.Title = "TEST SUBSET FORM"

# set items to select from
$form.Items = 0..9

# set some preselected indexes
$form.Indexes = @(0, 2, 4, 6)

# set optional converter of items to strings
$form.ItemToString = [PowerShellFar.Wrap]::ConverterToString({ "Item $_" })

# show
if ($form.Show()) {
	# get results
	$form.Indexes
}
