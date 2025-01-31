
job {
	Import-Module $PSScriptRoot\zoo.psm1
	Remove-RedisKey test:list

	$Far.InvokeCommand('rk:list key=test:list')
}

job {
	$r = $Far.Panel
	Assert-Far $r.GetType().Name -eq ListPanel
	Assert-Far $r.Title -eq 'List test:list'
	Assert-Far $r.Files.Count -eq 0
}

keys Esc

job {
	Assert-Far -Native
	Assert-Far (Test-RedisKey test:list) -eq 0L

	$Far.InvokeCommand('rk:list key=test:list')
}

job {
	Assert-Far -ExplorerTypeId be46affb-dd5c-436b-99c3-197dfd6e9d1f
}

keys F7 q 2 Enter

job {
	Assert-Far -FileName q2
	Assert-Far (Get-RedisList test:list -Index 0) -eq q2
}

job {
	$Far.Panel.Close()
	Remove-RedisKey test:list
}
