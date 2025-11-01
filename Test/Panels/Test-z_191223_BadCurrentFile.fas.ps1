# Panel used to open with the current item 3 due to the empty "name".
# https://forum.farmanager.com/viewtopic.php?f=8&t=11965#p158120

job {
	$Explorer = [PowerShellFar.ObjectExplorer]::new()
	$Explorer.AsGetData = {
		[PSCustomObject]@{_id = 1; name = 'name1'}
		[PSCustomObject]@{_id = 2; name = 'name2'}
		[PSCustomObject]@{_id = 3}
	}
	$Explorer.CreatePanel().Open()
}
job {
	# current item is ".."
	Assert-Far -Plugin
	Assert-Far $(
		$__.CurrentIndex -eq 0
		$__.Files[0].Name -eq '..'
	)
}
keys Esc
