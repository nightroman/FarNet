
job {
	Import-Module $PSScriptRoot\zoo.psm1
	Remove-RedisKey test:edit

	$Far.InvokeCommand('rk:edit key=test:edit')
}

job {
	Assert-Far -Editor
	$r = $Far.Editor
	Assert-Far $r.Title -eq test:edit
	Assert-Far $r.GetText() -eq ''
}

keys Esc

job {
	Assert-Far -Panels
	Assert-Far (Test-RedisKey test:edit) -eq 0L

	$Far.InvokeCommand('rk:edit key=test:edit')
}

job {
	Assert-Far -Editor
	$Far.Editor.SetText('line0')
}

keys F2

job {
	Assert-Far -Editor
	Assert-Far (Get-RedisString test:edit) -eq line0

	$Far.Editor.SetText("0`nline1`nline2`n")
}

keys Esc Enter

job {
	Assert-Far -Panels
	Assert-Far (Get-RedisString test:edit) -eq "0`r`nline1`r`nline2`r`n"
	Remove-RedisKey test:edit
}
