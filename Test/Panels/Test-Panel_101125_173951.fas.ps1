<#
.Synopsis
	Show Type column only for mixed types

.Link
	_101125_173951
#>

job {
	'42', 'foo' | Out-FarPanel
}
job {
	Assert-Far -Plugin
	$columns = $Far.Panel.GetPlan(0).Columns
	Assert-Far @(
		$columns.Count -eq 1
		$columns[0].Name -eq 'System.String'
	)
}
keys Esc
job {
	Assert-Far -Native
}

job {
	42, 'foo' | Out-FarPanel
}
job {
	Assert-Far -Plugin
	$columns = $Far.Panel.GetPlan(0).Columns
	Assert-Far @(
		$columns.Count -eq 3
		$columns[0].Name -eq '##'
		$columns[1].Name -eq 'Value'
		$columns[2].Name -eq 'Type'
	)
}
keys Esc
