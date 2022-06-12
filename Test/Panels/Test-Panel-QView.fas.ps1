
job {
	# open Alias location
	Go-To Alias:
}

# find op
job {
	Find-FarFile 'op'
	Assert-Far -FileDescription 'Out-FarPanel'
}

# quick view
# 090823 issue: alternate name was null because of auto generation
keys CtrlQ
job {
	Assert-Far ($Far.Panel2.Kind -eq 'QView')

	# _091019_081503 Accept the error: 'The URL cannot be empty.*'
	# 110227 Disabled the error check, it is not triggered if we use text IO.
	<#
	Assert-Far ($Error -and $Error[0].ToString() -like 'The URL cannot be empty.*')
	$Error.RemoveAt(0)
	#>
}

# exit quick view
keys CtrlQ
job {
	Assert-Far ($Far.Panel2.Kind -ne 'QView')
}

# exit panel
keys Esc
