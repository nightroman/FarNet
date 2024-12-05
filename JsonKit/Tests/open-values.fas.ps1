
job {
	$Far.InvokeCommand('jk:open file=x-values.json')
}

job {
	Assert-Far -Plugin
	Assert-Far $Far.Panel.Title -eq 'Values'
}

job {
	$Far.Panel.Close()
}
