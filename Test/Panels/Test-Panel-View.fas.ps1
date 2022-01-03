
job {
	if ($global:Error) {throw 'Please remove errors.'}

	# open Alias location
	go Alias:
}
job {
	Assert-Far ($Far.Panel -is [PowerShellFar.ItemPanel])

	#! 090823 used to return null istead of dots
	Assert-Far $Far.Panel.ShownList[0].Name -eq '..'
}

# view all
keys F3
job {
	Assert-Far -Viewer
}

# exit viewer
keys Esc
job {
	Assert-Far -Panels
}

# find op
job {
	Find-FarFile 'op'
	Assert-Far -FileDescription 'Out-FarPanel'
}

# view
# 090823 fixed failure due to [select *]
keys F3
job {
	# _091019_081503 'The URL cannot be empty.*'
	<#
	Assert-Far ($Error -and $Error[0].ToString() -like 'The URL cannot be empty.*')
	$Error.RemoveAt(0)
	#>
	# 110301 It looks gone due to different methods in use
	Assert-Far -Viewer
	Assert-Far (!$global:Error)
}

# exit viewer
keys Esc
job {
	Assert-Far -Panels
}

# exit panel
keys Esc
