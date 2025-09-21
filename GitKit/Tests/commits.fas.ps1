
### commits panel
job {
	$Far.InvokeCommand("gk:commits repo=$PSScriptRoot")
}

job {
	Assert-Far $Far.Panel.GetType().Name -eq CommitsPanel
}

### test F#
job {
	$Far.InvokeCommand("fs:exec file=FSF\PanelCommit.fsx")
	Assert-Far ([FarNet.User]::Data["PanelCommit"].Author.Email.Contains('@'))
}

### changes panel
keys Enter
job {
	Assert-Far $Far.Panel.GetType().Name -eq ChangesPanel
}
keys ShiftEsc
