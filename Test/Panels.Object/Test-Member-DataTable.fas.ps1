<#
.Synopsis
	DataTable properties should be opened as DataPanel.
#>

# new data table with columns
$dt = [System.Data.DataTable]'Test'
$c1 = $dt.Columns.Add('Name', [string])
$c2 = $dt.Columns.Add('Age', [int])

# add one row
$r = $dt.NewRow()
$r.Name = 'Joe'
$r.Age = 42
$dt.Rows.Add($r)

# finish
$dt.AcceptChanges()

# object to be used by panel
$Data.SUT = [PSCustomObject]@{
	Table = $dt
}

job {
	# open member panel with SUT
	[PowerShellFar.MemberPanel]::new($Data.SUT).Open()
}

job {
	# find the table property
	Find-FarFile Table
	Assert-Far -FileDescription Test
}

keys Enter # enter the table property

job {
	# it is the DataPanel
	Assert-Far $Far.Panel.GetType() -eq ([PowerShellFar.DataPanel])
}

job {
	# exit both panels
	$Far.Panel.Close()
	$Far.Panel.Close()
}
