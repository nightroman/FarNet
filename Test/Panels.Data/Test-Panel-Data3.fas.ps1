<#
.Synopsis
	DB 3. End! [ShiftDel] to enters nulls in a member panels.
#>

job {
	& "$env:PSF\Samples\Tests\Test-Panel-DbCategories-.ps1"
}
job {
	Find-FarFile Task
}
keys Enter
job {
	Assert-Far @(
		$Far.Panel.Value.Category -isnot [System.DBNull]
		$Far.Panel.Value.Remarks -isnot [System.DBNull]
	)
	$Far.Panel.SelectNames(('Category', 'Remarks'))
}
keys ShiftDel
job {
	Assert-Far @(
		$Far.Panel.Value.Category -is [System.DBNull]
		$Far.Panel.Value.Remarks -is [System.DBNull]
	)
}
macro 'Keys"Esc Right"'
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog.Focused.Text -eq 'No'
}
keys Enter
job {
	Assert-Far -Panels
	Assert-Far $Far.Panel.Title -eq 'TestCategories'
}
keys Esc

### End of DB tests
job {
	$DbConnection.Close()
	Remove-Variable -Scope Global DbConnection, DbProviderFactory
}
