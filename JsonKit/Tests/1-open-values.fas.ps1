
job {
	$Far.InvokeCommand('jk:open file=x-values.json')
}

job {
	Assert-Far -Plugin
	Assert-Far $Far.Panel.Title -eq 'Array $'
}

job {
	$Far.Panel.Close()
}
