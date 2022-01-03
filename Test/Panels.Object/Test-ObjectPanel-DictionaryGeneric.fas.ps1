<#
.Synopsis
	Special processing of generic KeyValuePair in object panels

.Description
	When dictionary entries are shown on an object panel:
	-- [Enter] on KeyValuePair opens a panel for its Value
	-- [CtrlPgDn] still opens a member panel for KeyValuePair
#>

job {
	# panel with a dictionary
	$d = New-Object 'System.Collections.Generic.Dictionary[string, object]'
	$d.Name = 'name1'
	$d.Data = @{
		Name2 = 'name2'
		Data2 = $Host
	}
	$d | Out-FarPanel
}
job {
	Assert-Far ($Far.Panel -is [PowerShellFar.ObjectPanel])
	Assert-Far $Far.Panel.ShownFiles.Count -eq 1
}
macro 'Keys"Down Enter" -- Enter the only item'
job {
	Find-FarFile Data
}
macro 'Keys"Enter" -- Open Data in *object* panel'
job {
	Assert-Far @(
		$Far.Panel -is [PowerShellFar.ObjectPanel]
		$Far.Panel.ShownFiles.Count -eq 2
	)
}
keys Esc
job {
	Assert-Far -FileName Data
}
macro 'Keys"CtrlPgDn" -- Open Data in member panel'
job {
	$files = $Far.Panel.ShownFiles
	Assert-Far @(
		$Far.Panel -is [PowerShellFar.MemberPanel]
		$Far.Panel.Title -eq 'Members: KeyValuePair`2'
		# Key, Value
		$files.Count -eq 2
		$files[0].Name -eq 'Key'
		$files[1].Name -eq 'Value'
	)
}
keys ShiftEsc
