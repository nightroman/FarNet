
job {
	$Global:db = Import-Module FarNet.Redis
	Remove-RedisKey 1L

	$Far.InvokeCommand('rk:list key=1L')
}

job {
	$r = $Far.Panel
	Assert-Far $r.GetType().Name -eq ListPanel
	Assert-Far $r.Title -eq 'List 1L'
	Assert-Far $r.Files.Count -eq 0
}

keys Esc

job {
	Assert-Far -Native
	Assert-Far (Test-RedisKey 1L) -eq 0L

	$Far.InvokeCommand('rk:list key=1L')
}

job {
	Assert-Far -ExplorerTypeId be46affb-dd5c-436b-99c3-197dfd6e9d1f
}

keys F7 q 2 Enter

job {
	Assert-Far -FileName q2
	Assert-Far (Get-RedisList 1L -Index 0) -eq q2
}

job {
	$Far.Panel.Close()
}
