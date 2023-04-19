
job {
	$Far.InvokeCommand("gk:branches repo=$PSScriptRoot")
}

job {
	Assert-Far $Far.Panel.GetType().Name -eq BranchesPanel
	Find-FarFile main
}

keys Enter

job {
	Assert-Far $Far.Panel.GetType().Name -eq CommitsPanel
}

keys Down Enter

job {
	Assert-Far $Far.Panel.GetType().Name -eq ChangesPanel
}

keys ShiftEsc
