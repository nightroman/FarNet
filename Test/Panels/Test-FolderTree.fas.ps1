
job {
	# open the panel
	$Panel = New-Object PowerShellFar.FolderTree 'C:\ROM'
	$Panel.Open()
}
job {
	Assert-Far $__.Title -eq 'Tree'
}

# go to the root item
keys Down
job {
	Assert-Far $__.Title -eq 'Tree: C:\ROM'
}

# find DEV, enter
job {
	Find-FarFile 'DEV'
}
keys Enter
job {
	Assert-Far $Far.Panel2.CurrentDirectory -eq 'C:\ROM\DEV'
}

# expand
keys Right
job {
	Assert-Far $__.Title -eq 'Tree: C:\ROM'
}
job {
	Find-FarFile 'Achilles'
}
job {
	Assert-Far $__.Title -eq 'Tree: C:\ROM\DEV'
}

keys Enter
job {
	Assert-Far $Far.Panel2.CurrentDirectory -eq 'C:\ROM\DEV\Achilles'
}

# go up
keys CtrlPgUp
job {
	Assert-Far -FileName 'ROM'
	Assert-Far $__.Title -eq 'Tree: C:\'
}

# exit
keys Esc
