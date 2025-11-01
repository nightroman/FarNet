<#
.Synopsis
	DB 3. End! [ShiftDel] to enters nulls in a member panels.
#>

job {
	& "$env:FarNetCode\Samples\Tests\Test-Panel-DBCategories.far.ps1"
}
job {
	Find-FarFile Task
}
keys Enter
job {
	Assert-Far @(
		$__.Value.Category -isnot [System.DBNull]
		$__.Value.Remarks -isnot [System.DBNull]
	)
	$__.SelectNames(('Category', 'Remarks'))
}
keys ShiftDel
job {
	Assert-Far @(
		$__.Value.Category -is [System.DBNull]
		$__.Value.Remarks -is [System.DBNull]
	)
}
macro 'Keys"Esc Right"'
job {
	Assert-Far -Dialog
	Assert-Far $__.Focused.Text -eq 'No'
}
keys Enter
job {
	Assert-Far -Panels
	Assert-Far $__.Title -eq 'TestCategories'
}
keys Esc

### End of DB tests
job {
	$DbConnection.Close()
	Remove-Variable -Scope Global DbConnection, DbProviderFactory
}
