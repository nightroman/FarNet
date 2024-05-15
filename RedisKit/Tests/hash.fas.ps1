
job {
	$Global:db = Import-Module FarNet.Redis
	Remove-RedisKey 1H

	$Far.InvokeCommand('rk:hash key=1H')
}

job {
	$r = $Far.Panel
	Assert-Far $r.GetType().Name -eq HashPanel
	Assert-Far $r.Title -eq 'Hash 1H'
	Assert-Far $r.Files.Count -eq 0
}

keys Esc

job {
	Assert-Far -Native
	Assert-Far (Test-RedisKey 1H) -eq 0L

	$Far.InvokeCommand('rk:hash key=1H')
}

job {
	Assert-Far -ExplorerTypeId 29ae0735-2a00-43be-896b-9e2e8a67d658
}

keys F7 q 2 Enter

job {
	Assert-Far -FileName q2 -FileDescription ''
	Assert-Far (Test-RedisKey 1H) -eq 1L
}

keys F4 v F2

job {
	Assert-Far -Editor
	Assert-Far (Get-RedisHash 1H q2) -eq v
}

keys 2 ShiftF10

job {
	Assert-Far -Panels -FileName q2 -FileDescription v2
	Assert-Far (Get-RedisHash 1H q2) -eq v2
}

keys F7 q 1 Enter

job {
	Assert-Far -FileName q1 -FileDescription ''
	Assert-Far (Get-RedisHash 1H q1) -eq ''
}

job {
	$Far.Panel.Close()
}
