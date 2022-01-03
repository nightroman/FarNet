
job {
	# ensure no variables
	Remove-Variable 'a[12]' -Scope Global

	# open panel
	go Variable:\
}
job {
	Assert-Far @(
		$Far.Panel.CurrentDirectory -eq 'Variable:\' #_110227_123432
		!$Far.Panel.CurrentFile # dots
	)
}

# view all
keys F3
job {
	Assert-Far -Viewer
}
keys Esc
job {
	Assert-Far -Panels
}

# new variable
# 090824 fix: with panel path 'Microsoft.PowerShell.Core\Variable::' new variable name was '\a1', not 'a1'
macro 'Keys"F7 a 1 AltT i n t AltV 1 2 3 4 5 Enter"'
job {
	# columns =
	Assert-Far -FileName 'a1' -FileDescription '12345' -FileOwner ''
	Assert-Far @($Far.Panel.CurrentFile.Columns)[0] -eq 'None'
}

# not supported
keys ShiftEnter
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[1].Text -eq 'Operation is not supported by the provider.'
}
keys Esc
job {
	Assert-Far -Panels
}

# view file
keys F3
job {
	Assert-Far -Viewer
}
keys Esc
job {
	Assert-Far -Panels
}

# open members
keys CtrlPgDn
job {
	# to Value
	Find-FarFile 'Value'
}

# edit Value
macro 'Keys"Enter N e w V a l u e Enter"'
job {
	Assert-Far -FileDescription 'NewValue'
}

job {
	# to Description
	Find-FarFile 'Description'
}

# edit Description, then we show that it is not copied on 'copy here'
macro 'Keys"Enter N e w D e s c r i p t i o n Enter"'
job {
	Assert-Far -FileDescription 'NewDescription'
}

# exit members
keys Esc
job {
	Assert-Far -FileName 'a1' -FileOwner 'NewDescription'
}

# copy here
macro 'Keys"ShiftF5 a 2 Enter"'
job {
	# columns =
	Assert-Far -FileName 'a2' -FileDescription 'NewValue' -FileOwner ''
	Assert-Far @($Far.Panel.CurrentFile.Columns)[0] -eq 'None'
}

# delete a2
keys Del
job {
	# v4.0
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[2].Text.ToUpper().Contains('TARGET "ITEM: A2"')
}
keys y
job {
	Assert-Far -Panels
	Assert-Far ($Far.Panel.CurrentFile.Name -ne 'a2')
}

# find a1, delete
job {
	Find-FarFile 'a1'
}
keys Del
job {
	# v4.0
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[2].Text.ToUpper().Contains('TARGET "ITEM: A1"')
}
keys y
job {
	Assert-Far -Panels
	Assert-Far ($Far.Panel.CurrentFile.Name -ne 'a1')
}

# exit panel
keys Esc
