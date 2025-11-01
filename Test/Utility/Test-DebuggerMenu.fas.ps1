
run {
	# show menu
	$bp = Get-PSBreakpoint
	Assert-Far (!$bp) -Message "Please remove breakpoints."
	$Psf.ShowDebugger()
}

job {
	# debugger menu?
	Assert-Far $__.Focused.Text -eq "&Line breakpoint..."
}

# open line bp dialog
keys Enter
job {
	# line bp dialog?
	Assert-Far $__[0].Text -eq "Line breakpoint"
}

# type
macro 'Keys"AltL 1 AltS c : / r o m / a p s / a b o u t . p s 1 Enter"'

# go to new bp
keys 1
job {
	# the current item is..
	Assert-Far $__.Focused.Text -eq "&1 Line breakpoint on 'C:\rom\aps\about.ps1:1'"
}

# edit
keys F4
job {
	Assert-Far -Editor
}

# exit editor
keys Esc
run {
	# show menu
	$Psf.ShowDebugger()
}

# go to bp
keys 1
job {
	# the current item is..
	Assert-Far $__.Focused.Text -eq "&1 Line breakpoint on 'C:\rom\aps\about.ps1:1'"
}

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
job {
	# no bp?
	$bp = Get-PSBreakpoint
	Assert-Far (!$bp)
}

# exit dialog
keys Esc
