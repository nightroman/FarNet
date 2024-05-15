
job {
	$Global:db = Import-Module FarNet.Redis
	Remove-RedisKey 1

	$Far.InvokeCommand('rk:edit key=1')
}

job {
	Assert-Far -Editor
	$r = $Far.Editor
	Assert-Far $r.Title -eq '1'
	Assert-Far $r.GetText() -eq ''
}

keys Esc

job {
	Assert-Far -Panels
	Assert-Far (Test-RedisKey 1) -eq 0L

	$Far.InvokeCommand('rk:edit key=1')
}

job {
	Assert-Far -Editor
	$Far.Editor.SetText('line0')
}

keys F2

job {
	Assert-Far -Editor
	Assert-Far (Get-RedisString 1) -eq line0

	$Far.Editor.SetText("0`nline1`nline2`n")
}

keys Esc Enter

job {
	Assert-Far -Panels
	Assert-Far (Get-RedisString 1) -eq "0`r`nline1`r`nline2`r`n"
}
