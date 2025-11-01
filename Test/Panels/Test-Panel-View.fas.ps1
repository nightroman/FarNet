
job {
	Assert-Far -Title Ensure -NoError

	# open Alias location
	Go-To Alias:
}
job {
	Assert-Far ($__ -is [PowerShellFar.ItemPanel])

	#! 090823 used to return null istead of dots
	Assert-Far $__.Files[0].Name -eq '..'
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
	Assert-Far -NoError -Viewer
}

# exit viewer
keys Esc
job {
	Assert-Far -Panels
}

# exit panel
keys Esc
