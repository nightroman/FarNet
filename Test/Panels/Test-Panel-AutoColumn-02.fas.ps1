<#
.Synopsis
	Fixed: unwanted columns; original property order.
#>

job {
	[System.Data.Common.DbProviderFactories]::GetFactoryClasses() | Out-FarPanel
}
job {
	$Columns = @($Far.Panel.GetPlan(0).Columns)
	Assert-Far @(
		$Columns.Count -eq 4
		$Columns[0].Name -eq 'Name'
		$Columns[0].Kind -eq 'N'
		$Columns[1].Name -eq 'Description'
		$Columns[1].Kind -eq 'Z'
		$Columns[2].Name -eq 'InvariantName'
		$Columns[2].Kind -eq 'O'
		$Columns[3].Name -eq 'AssemblyQualifiedName'
		$Columns[3].Kind -eq 'C0'
	)
}
keys Esc
