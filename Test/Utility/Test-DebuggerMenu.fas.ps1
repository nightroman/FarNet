
# show menu
run {
	if (Get-PSBreakpoint) {throw "Remove breakpoints."}

	$Psf.Settings.DisableAttachDebuggerDialogOnBreakpoint = $true

	$Psf.ShowDebugger()
}

# menu?
job {
	Assert-Far -Menu
}

# open line bp dialog
keys Enter
job {
	# line bp dialog?
	Assert-Far -Dialog
	Assert-Far $__[0].Text -eq "Line breakpoint"
}

# type
macro "Keys'AltL 2 AltS'; print[[$env:FarNetCode\web.ps1]]"
keys Enter

# go to new bp
keys 1

# edit, exit
keys F4
job {
	Assert-Far -EditorFileName *\web.ps1
	Assert-Far $__.Caret.Y -eq 1
	$__.Close()
}

# show menu
run {
	$Psf.ShowDebugger()
}

# go to bp
keys 1

# toggle
keys Space
job {
	# disabled?
	$bp = Get-PSBreakpoint
	Assert-Far (!$bp.Enabled)
}

# toggle
keys Space
job {
	# enabled?
	$bp = Get-PSBreakpoint
	Assert-Far $bp.Enabled
}

# delete bp
keys Del

# no bp?
job {
	Assert-Far (!(Get-PSBreakpoint))
}

# exit menu
keys Esc

# done
job {
	$Psf.Settings.DisableAttachDebuggerDialogOnBreakpoint = $false
}
