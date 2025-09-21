
### open branches panel
job {
	$Far.InvokeCommand("gk:branches repo=$PSScriptRoot")
}
job {
	Assert-Far $Far.Panel.GetType().Name -eq BranchesPanel
	Find-FarFile main
}

### test F#
job {
	$Far.InvokeCommand("fs:exec file=FSF\PanelBranch.fsx")
	Assert-Far ([FarNet.User]::Data["PanelBranch"].Author.Email.Contains('@'))
}

### branch copy sha
macro 'Keys "F11" if Menu.Select("GitKit", 2) > 0 then Keys "Enter" if Menu.Select("Copy SHA-1", 2) > 0 then Keys "Enter" end end'
job {
	Assert-Far -Dialog
	Assert-Far ($Far.Dialog[1].Text -match '^\w+ \d\d\d\d-\d\d-\d\d ')
}
keys Esc

### enter commits panel
keys Enter
job {
	Assert-Far $Far.Panel.GetType().Name -eq CommitsPanel
}

### commit copy sha
keys Down
macro 'Keys "F11" if Menu.Select("GitKit", 2) > 0 then Keys "Enter" if Menu.Select("Copy SHA-1", 2) > 0 then Keys "Enter" end end'
job {
	Assert-Far -Dialog
	Assert-Far ($Far.Dialog[1].Text -match '^\w+ \d\d\d\d-\d\d-\d\d ')
}
keys Esc

### enter/exit changes panel
keys Enter
job {
	Assert-Far $Far.Panel.GetType().Name -eq ChangesPanel
	Assert-Far ($Far.Panel.Title -match '^\w{7}: ')
}
keys Esc

### compare commits in commits panel
keys Down
macro 'Keys "F11" if Menu.Select("GitKit", 2) > 0 then Keys "Enter" if Menu.Select("Compare commits", 2) > 0 then Keys "Enter" end end'
job {
	Assert-Far $Far.Panel.GetType().Name -eq ChangesPanel
	Assert-Far ($Far.Panel.Title -match '^\w{7}/\w{7} ')
}

keys ShiftEsc
