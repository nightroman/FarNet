﻿<#
.Synopsis
	Sends Far task messages to another Far.
	Author: Roman Kuzmin

.Description
	Requires:
	- FarNet.Redis library, $env:FARNET_REDIS_CONFIGURATION
	- Register-FarRedisTask.ps1 and Start-Far.ps1 in the path

	The script uses Redis pub/sub in order to pair two Far instances and send
	messages between them. The second Far is started automatically when needed.

.Parameter Task
		The Far task script to send to another Far.

.Parameter Data
		Optional data sent as JSON and then converted back to hashtable.
		The received hashtable is provided for the task script as $Data.

.Parameter SkipExited
		Tells to immediately return true if the pair process has exited.
#>

[CmdletBinding()]
param(
	[Parameter(Position=0, Mandatory=1)]
	[scriptblock]$Task
	,
	[Parameter(Position=1)]
	[hashtable]$Data
	,
	[switch]$SkipExited
)

$ErrorActionPreference=1

# check the pair
if ($FarRedisPair = [FarNet.User]::Data['FarRedisPair']) {
	if ($FarRedisPair.HasExited) {
		[FarNet.User]::Remove('FarRedisPair')
		if ($SkipExited) {
			return $true
		}
	}
}

# Redis message data
$taskMessageData = [ordered]@{
	Data = $Data
	Task = $Task.ToString()
}

if ($FarRedisPair = [FarNet.User]::Data['FarRedisPair']) {
	#: we are paired, send the task message to the pair
	$message = $taskMessageData | ConvertTo-Json -Depth 99 -Compress
	Send-RedisMessage "FarRedisHandler:$($FarRedisPair.Id)" $message
}
else {
	#: not yet paired, subscribe to messages
	if (![FarNet.User]::Data['FarRedisHandler']) {
		# here we will receive the pair message
		Register-FarRedisTask
	}

	# keep data and start the pair, data will be send when we receive the pair message
	[FarNet.User]::Data.FarRedisData = $taskMessageData
	Start-Far ps:Register-FarRedisTask $Far.CurrentDirectory -Environment @{FAR_REDIS_PAIR = $PID}
}
