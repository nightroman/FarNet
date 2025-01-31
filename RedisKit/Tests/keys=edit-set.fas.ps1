
job {
	Import-Module $PSScriptRoot\zoo.psm1
	Remove-RedisKey (Search-RedisKey test:*)

	Set-RedisSet test:edit q1
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
	Assert-Far $Editor.Title -eq edit
	Assert-Far $Editor.GetText() -eq 'q1'

	$Editor.SetText("q2`nq1`nq2")
	$Editor.Save()

	$r = (Get-RedisSet test:edit | Sort-Object) -join '#'
	Assert-Far $r -eq q1#q2

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
