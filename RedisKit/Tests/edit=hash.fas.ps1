
job {
	Import-Module $PSScriptRoot\zoo.psm1
	Remove-RedisKey test:edit
	Set-RedisHash test:edit f1 v1

	$Far.InvokeCommand('rk:edit key=test:edit')
}

job {
	Assert-Far -Editor
	$Editor = $Far.Editor
	Assert-Far $Editor.Title -eq 'Hash test:edit'
	Assert-Far ($Editor.Strings -join '|') -eq 'f1|v1|'

	$Editor[0].Text = 'f2'
	$Editor.Save()

	$r = Get-RedisHash test:edit | ConvertTo-Json -Compress
	Assert-Far $r -eq '{"f2":"v1"}'

	$Editor.Close()
}

job {
	Assert-Far -Panels
	Remove-RedisKey test:edit
}
