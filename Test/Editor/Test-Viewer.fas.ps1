<#
.Synopsis
	Viewer tests.
.Description
	*) Depends on hardcoded file names, may fail after file changes.
	*) Depends on sorting in panels, it should be 'n'.
	*) Restart Far after failures in the middle.
#>

$Data.Log = ''
$Data.File1 = "$env:FarHome\FarNet\FarNet.dll"
$Data.File2 = "$env:FarHome\FarNet\FarNet.pdb"
$Data.Opened = [EventHandler]{ $Data.Log += 'Opened:{0};' -f $this.FileName }
$Data.Closed = [EventHandler]{ $Data.Log += 'Closed:{0};' -f $this.FileName }

# [_100117_101226] there are several Opened and one Closed events
job {
	# go to a file with another file after it
	$Far.AnyViewer.add_Opened($Data.Opened)
	$Far.AnyViewer.add_Closed($Data.Closed)
	$__.GoToPath($Data.File1)
}
macro 'Keys"F3" -- open file in the viewer'
job {
	Assert-Far $__.FileName -eq $Data.File1
}
macro 'Keys"Add" -- open next file in the same viewer'
job {
	Assert-Far $__.FileName -eq $Data.File2
}
macro 'Keys"Subtract" -- open prev file in the same viewer'
job {
	Assert-Far $__.FileName -eq $Data.File1
}
macro 'Keys"Esc" -- close viewer'
job {
	# end
	$Far.AnyViewer.remove_Opened($Data.Opened)
	$Far.AnyViewer.remove_Closed($Data.Closed)
	Assert-Far -Panels
	Assert-Far $Data.Log -eq "Opened:$($Data.File1);Opened:$($Data.File2);Opened:$($Data.File1);Closed:$($Data.File1);"
}
