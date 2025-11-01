<#
.Synopsis
	FarNet disk menu, RegisterTool, PostMacro.
#>

$macro = job {
	# register
	$null = $Psf.Manager.RegisterTool(
		[FarNet.ModuleToolAttribute]@{Name="Test FarNet disk menu"; Options = "Disk"; Id="e3caaf07-e63d-42cf-a31c-c35b91fbdc49"},
		{ $Psf.ShowPanel() }
	)

	# open tree panel via the disk menu
	$(
		if ($__.IsLeft) {'Keys("AltF1")'} else {'Keys("AltF2")'}
		'if Menu.Select("Test FarNet disk menu", 2) > 0 then Keys("Enter") if Menu.Select("Folder tree", 0) > 0 then Keys("Enter") end end'
	) | Out-String
}

macro $macro

job {
	# unregister
	$tool = $Far.GetModuleAction("e3caaf07-e63d-42cf-a31c-c35b91fbdc49")
	Assert-Far ($tool -ne $null)
	$tool.Unregister()

	# tree panel
	Assert-Far $__.Title -eq 'Tree'
}

macro 'Keys"Esc" -- exit the panel'
