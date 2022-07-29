<#
.Synopsis
	Special processing of DictionaryEntry in object panels

.Description
	When dictionary entries are shown on an object panel:
	-- [Enter] on DictionaryEntry opens a panel for its Value
	-- [CtrlPgDn] still opens a member panel for DictionaryEntry

	DictionaryEntry member panel only shows Key and Value.
#>

# _131002_111804 In a DictionaryEntry we avoid Name (same as Key).
# Let's test that Name actually exists and our trick is needed.
$de = New-Object Collections.DictionaryEntry @('key1', 'value1')
if ($de.Name -ne 'key1') {throw}

job {
	# panel with a dictionary
	@{
		Long = 1L
		Double = 3.14
		String = 'name1'
		Array = 1, 2, 3
		Dictionary = @{
			Long = 2L
			Double = 3.14
			String = 'name2'
		}
	} | Out-FarPanel

}
job {
	Assert-Far ($Far.Panel -is [PowerShellFar.ObjectPanel])
	Assert-Far $Far.Panel.GetFiles().Count -eq 1
}
macro 'Keys"Down Enter" -- Enter the only item'
job {
	Find-FarFile Array
}
macro 'Keys"Enter" -- Open Array in *object* panel'
job {
	Assert-Far @(
		$Far.Panel -is [PowerShellFar.ObjectPanel]
		$Far.Panel.GetFiles().Count -eq 3
	)
}
keys Esc
job {
	Assert-Far -FileName Array
}
macro 'Keys"CtrlPgDn" -- Open Array in member panel'
job {
	Assert-Far @(
		$Far.Panel -is [PowerShellFar.MemberPanel]
		$Far.Panel.Title -eq 'Members: DictionaryEntry'
	)
}
keys Esc
job {
	Find-FarFile String
}
macro 'Keys"Enter" -- Open String in *member* panel'
job {
	$files = $Far.Panel.GetFiles()
	Assert-Far @(
		$Far.Panel -is [PowerShellFar.MemberPanel]
		$Far.Panel.Title -eq 'Members: DictionaryEntry'
		#_131002_111804
		$files.Count -eq 2
		$files[0].Name -ceq 'Key'
		$files[1].Name -ceq 'Value'
	)
}
keys ShiftEsc
