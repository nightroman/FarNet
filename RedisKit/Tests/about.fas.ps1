
job {
	$Far.InvokeCommand('rk:keys')
}

job {
	$r = $Far.Panel
	Assert-Far $r.GetType().Name -eq KeysPanel
	Assert-Far ($r.Title -like 'Keys 127.0.0.1:3278,*')
	$r.Close()
}
