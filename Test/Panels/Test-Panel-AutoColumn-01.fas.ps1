<#
.Synopsis
	Fixed: null Name; auto column types on many columns.
#>

job {
	# make more columns than max
	$props = 'Name', 'Description', 'InvariantName', 'AssemblyQualifiedName', 'q1', 'q2', 'q3', 'q4', 'q5'
	Assert-Far ($props.Count -gt $Psf.Settings.MaximumPanelColumnCount)

	# couple of objects
	$ob1 = 1 | Select-Object $props
	$ob2 = 1 | Select-Object $props

	# out
	$ob1, $ob2 | Out-FarPanel -Title AutoColumnType
}
job {
	Assert-Far $Far.Panel.Title -eq AutoColumnType
}
keys Esc
