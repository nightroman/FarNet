#! _201225_28 Panel is opened from Desktop window

macro @'
print "ps: 42; $Host | Out-FarPanel"
Keys "Enter"
'@
job {
	Assert-Far -Plugin
}
keys Esc
