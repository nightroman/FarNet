<#
.Synopsis
	Subscribes to Far task messages from another Far.
	Author: Roman Kuzmin

.Description
	See Send-FarRedisTask.ps1
#>

$ErrorActionPreference=1

if ([FarNet.User]::Data['FarRedisHandler']) {
	return
}

Import-Module $env:FARHOME\FarNet\Lib\FarNet.Redis\FarNet.Redis.psd1
$db = Open-Redis

[FarNet.User]::Data.FarRedisHandler = Add-RedisHandler FarRedisHandler:$PID {
	param($channel, $message)
	$pairId = 0
	if ([int]::TryParse($message, [ref]$pairId)) {
		#: pair message, keep the pair process
		[FarNet.User]::Data.FarRedisPair = Get-Process -Id $pairId

		# pop the pending task message data
		$taskMessageData = [FarNet.User]::Pop('FarRedisData')

		# send the task message to the pair
		$message = $taskMessageData | ConvertTo-Json -Depth 99 -Compress
		Send-RedisMessage "FarRedisHandler:$pairId" $message
	}
	else {
		#: task message, decode and start the task
		$taskMessageData = ConvertFrom-Json $message -AsHashtable
		Start-FarTask -Data $taskMessageData.Data ([scriptblock]::Create($taskMessageData.Task))
	}
}

if ($pairId = $env:FAR_REDIS_PAIR) {
	#: started as pair, send this pair id to that pair
	$env:FAR_REDIS_PAIR = $null
	[FarNet.User]::Data.FarRedisPair = Get-Process -Id $pairId
	Send-RedisMessage "FarRedisHandler:$pairId" $PID
}
