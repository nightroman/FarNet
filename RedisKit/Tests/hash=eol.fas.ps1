
job {
	$Global:db = Import-Module FarNet.Redis

	Remove-RedisKey temp:hash1

	Set-RedisHash temp:hash1 f1 v1

	Set-RedisHash temp:hash1 f2 v2
	Set-RedisHash temp:hash1 -Persist f2 -TimeToLive 0:1:0

	$Far.InvokeCommand('rk:hash key=temp:hash1; eol=1')
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

	# remove f1 to see what happens when the last field f2 expires
	Set-RedisHash temp:hash1 -Remove f1

	$Far.Panel.Close()
}
