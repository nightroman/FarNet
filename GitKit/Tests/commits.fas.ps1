
job {
	$Far.InvokeCommand("gk:commits repo=$PSScriptRoot")
}

job {
	Assert-Far $Far.Panel.GetType().Name -eq CommitsPanel
}

keys Enter

job {
	Assert-Far $Far.Panel.GetType().Name -eq ChangesPanel
}

keys ShiftEsc
