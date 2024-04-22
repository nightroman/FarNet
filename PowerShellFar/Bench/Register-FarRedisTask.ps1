<#
.Synopsis
	Subscribes to Far task messages from another Far.
	Author: Roman Kuzmin

.Description
	See Send-FarRedisTask.ps1
#>

$ErrorActionPreference = 1

if ([FarNet.User]::Data['FarRedisSub']) {
	return
}

Import-Module $env:FARHOME\FarNet\Lib\FarNet.Redis\FarNet.Redis.psd1
[FarNet.User]::Data.FarRedisDB = Open-Redis 127.0.0.1:3278

[FarNet.User]::Data.FarRedisSub = Register-RedisSub FarRedisSub:$PID -Database ([FarNet.User]::Data.FarRedisDB) {
	param($channel, $message)
	$id = 0
	if ([int]::TryParse($message, [ref]$id)) {
		[FarNet.User]::Data.FarRedisPair = Get-Process -Id $id

		$data = [FarNet.User]::Data['FarRedisData']
		[FarNet.User]::Remove('FarRedisData')

		$null = [FarNet.User]::Data.FarRedisDB.Publish(
			"FarRedisSub:$id",
			($data | ConvertTo-Json -Depth 99 -Compress)
		)
	}
	else {
		$data = ConvertFrom-Json $message -AsHashtable
		Start-FarTask -Data $data.Data ([scriptblock]::Create($data.Task))
	}
}

if ($pid2 = $env:FAR_REDIS_PAIR) {
	$env:FAR_REDIS_PAIR = $null
	$far2 = Get-Process -Id $pid2
	[FarNet.User]::Data.FarRedisPair = $far2
	$null = [FarNet.User]::Data.FarRedisDB.Publish("FarRedisSub:$($far2.Id)", $PID)
}
