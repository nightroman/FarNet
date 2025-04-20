
job {
	Import-Module $PSScriptRoot\zoo.psm1
	Remove-RedisKey zoo:hash1

	Set-RedisHash zoo:hash1 f1 v1

	Set-RedisHash zoo:hash1 f2 v2
	Set-RedisHash zoo:hash1 -Persist f2 -TimeToLive 0:1:0

	$Far.InvokeCommand('rk:hash key=zoo:hash1; eol=1')
}

job {
	Assert-Far -Plugin -FileName f1 -FileDescription v1

	# TTL not set
	$file = $Far.Panel.CurrentFile
	Assert-Far $file.LastWriteTime -eq ([datetime]::MinValue)

	# ~1 min
	Find-FarFile f2
	$file = $Far.Panel.CurrentFile
	$ttl = ($file.LastWriteTime - [datetime]::Now).TotalSeconds
	Assert-Far ($ttl -gt 50 -and $ttl -lt 60)

	Remove-RedisKey zoo:hash1

	$Far.Panel.Close()
}
