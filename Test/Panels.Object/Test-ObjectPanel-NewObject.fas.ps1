<#
.Synopsis
	Create a new object in an empty object panel.
#>

job {
	# open panel as output panel with no input
	Out-FarPanel
}
job {
	# empty object panel?
	Assert-Far ($Far.Panel -is [PowerShellFar.ObjectPanel])
	$Columns = $Far.Panel.GetPlan(0).Columns
	Assert-Far $Columns.Count -eq 1
	Assert-Far $Columns[0].Name -eq '<empty>'
}

### new object with general names
keys F7
job {
	# new property dialog?
	Assert-Far -Dialog
}
macro @'
print("New-Object psobject -Property @{ PropertyName1 = 'PropertyValue1' }")
Keys('Enter')
'@
job {
	Assert-Far -Panels
	# with 1 column and 1 file
	$Columns = $Far.Panel.GetPlan(0).Columns
	Assert-Far -FileName 'PropertyValue1'
	Assert-Far @(
		$Columns.Count -eq 1
		$Columns[0].Name -eq 'PropertyName1'
		$Far.Panel.ShownFiles.Count -eq 1
	)
}

### enter the member panel to add another property
keys Enter
job {
	Assert-Far ($Far.Panel -is [PowerShellFar.MemberPanel])
}
keys F7
job {
	# dialog
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'New property'
}
macro 'Keys"O t h e r N a m e AltV V a l u e 2 Enter"'
job {
	Assert-Far -FileName 'OtherName' -FileDescription 'Value2'
	Assert-Far $Far.Panel.Value.OtherName -eq 'Value2'
}

# exit member panel
keys Esc
job {
	# object panel
	Assert-Far ($Far.Panel -is [PowerShellFar.ObjectPanel])
	# with still 1 column
	$Columns = $Far.Panel.GetPlan(0).Columns
	Assert-Far @(
		$Columns.Count -eq 1
		$Columns[0].Name -eq 'PropertyName1'
	)
}

### force refresh, reformat
keys CtrlR
job {
	# 2 columns
	# N ~ OtherName, Z ~ PropertyName1
	$Columns = $Far.Panel.GetPlan(0).Columns
	Assert-Far -FileName 'Value2' -FileDescription 'PropertyValue1'
	Assert-Far @(
		$Columns.Count -eq 2
		$Columns[0].Name -eq 'PropertyName1'
		$Columns[1].Name -eq 'OtherName'
	)
}

### remove the object
keys Del
job {
	# dialog
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq 'Remove'
}
keys Enter
job {
	$Columns = $Far.Panel.GetPlan(0).Columns
	Assert-Far -Panels
	Assert-Far @(
		$Columns.Count -eq 2
		$Columns[0].Name -eq 'PropertyName1'
		$Columns[1].Name -eq 'OtherName'
		$Far.Panel.ShownFiles.Count -eq 0
	)
}

### refresh
keys CtrlR
job {
	# with 1 column
	$Columns = $Far.Panel.GetPlan(0).Columns
	Assert-Far -Panels
	Assert-Far @(
		$Columns.Count -eq 1
		$Columns[0].Name -eq '<empty>'
	)
}

# exit
keys Esc
