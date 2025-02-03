
job {
	Import-Module $PSScriptRoot\zoo.psm1
	Remove-RedisKey (Search-RedisKey test:*)

	Set-RedisHash test:edit f1 v1
	Set-RedisKey test:edit -TimeToLive 1:0

	$Far.InvokeCommand('rk:keys mask=test:')
}

job {
	$Panel = $Far.Panel
	Assert-Far $Panel.GetType().Name -eq KeysPanel
	Assert-Far ($Panel.Title -like 'Keys test:')

	$r = $Panel.CurrentFile
	Assert-Far $r.Name -eq edit
	Assert-Far ($r.LastWriteTime -gt (Get-Date))
}

keys F4

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
	Assert-Far -Plugin
	$Panel = $Far.Panel

	$r = $Panel.CurrentFile
	Assert-Far $r.Name -eq edit
	Assert-Far ($r.LastWriteTime -eq ([datetime]::MinValue))

	$Panel.Close()
	Remove-RedisKey test:edit
}
