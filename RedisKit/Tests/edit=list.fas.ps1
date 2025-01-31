
job {
	Import-Module $PSScriptRoot\zoo.psm1
	Remove-RedisKey test:edit
	Set-RedisList test:edit q1

	$Far.InvokeCommand('rk:edit key=test:edit')
}

job {
	Assert-Far -Editor
	$Editor = $Far.Editor
	Assert-Far $Editor.Title -eq test:edit
	Assert-Far $Editor.GetText() -eq 'q1'

	$Editor.SetText("q1`nq2")
	$Editor.Save()

	$r = (Get-RedisList test:edit) -join '#'
	Assert-Far $r -eq q1#q2

	$Editor.Close()
}

job {
	Assert-Far -Panels
	Remove-RedisKey test:edit
}
