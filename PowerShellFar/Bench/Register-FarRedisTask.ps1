<#
.Synopsis
	Subscribes to Far task messages from another Far.
	Author: Roman Kuzmin

.Description
	See Send-FarRedisTask.ps1
#>

$ErrorActionPreference = 1

if ([FarNet.User]::Data['FarRedisHandler']) {
	return
}

Import-Module $env:FARHOME\FarNet\Lib\FarNet.Redis\FarNet.Redis.psd1
[FarNet.User]::Data.FarRedisDB = $db = Open-Redis

[FarNet.User]::Data.FarRedisHandler = Add-RedisHandler FarRedisHandler:$PID {
	param($channel, $message)
	$pairId = 0
	if ([int]::TryParse($message, [ref]$pairId)) {
		#: pair message, keep the pair process
		[FarNet.User]::Data.FarRedisPair = Get-Process -Id $pairId

		# pop the pending task message data
		$data = [FarNet.User]::Pop('FarRedisData')

		# send the task message to the pair
		Send-RedisMessage "FarRedisHandler:$pairId" ($data | ConvertTo-Json -Depth 99 -Compress) -Database ([FarNet.User]::Data.FarRedisDB)
	}
	else {
		#: task message
		$data = ConvertFrom-Json $message -AsHashtable
		Start-FarTask -Data $data.Data ([scriptblock]::Create($data.Task))
	}
}

if ($pairId = $env:FAR_REDIS_PAIR) {
	#: started as pair, send this pair id to that pair
	$env:FAR_REDIS_PAIR = $null
	[FarNet.User]::Data.FarRedisPair = Get-Process -Id $pairId
	Send-RedisMessage "FarRedisHandler:$pairId" $PID
}
