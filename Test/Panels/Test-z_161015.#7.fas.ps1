#! _201225_28 Panel is opened from Desktop window

macro 'print "ps: 42; $Host | Out-FarPanel"; Keys "Enter"' # $r
job {
	Assert-Far -Plugin
	$__.Close()

	# REPL $r
	Assert-Far $r -eq 42
}
