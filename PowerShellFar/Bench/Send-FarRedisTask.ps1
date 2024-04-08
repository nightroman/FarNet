<#
.Synopsis
	Sends Far task messages to another Far.
	Author: Roman Kuzmin

.Description
	Requires:
	- FarNet.Redis library and Garnet/Redis server, port 3278.
	- Register-FarRedisTask.ps1 and Start-Far.ps1 in the path.

	The script uses Redis pub/sub in order to pair two Far instances and send
	messages between them. The second Far is started automatically when needed.

.Parameter Task
		Specifies the Far task script to be sent to another Far.

.Parameter Data
		Optional data to be sent as JSON and then used by Start-FarTask.
		The Task script uses $Data containing the original and some meta.
		Meta key names start with underscore, avoid using such keys in Data.
#>

[CmdletBinding()]
param(
	[Parameter(Position=0, Mandatory=1)]
	[scriptblock]$Task
	,
	[Parameter(Position=1)]
	[hashtable]$Data
)

Set-StrictMode -Version 3
$ErrorActionPreference = 1

Import-Module FarNet.Redis
$db = Open-Redis localhost:3278

# subscribe to messages
if (![FarNet.User]::Data['FarRedisSub'] -and ([runspace]::DefaultRunspace.Id -eq 1)) {
	Register-FarRedisTask
}

# check pair Far
if ([FarNet.User]::Data['FarRedisProcess']) {
	if ([FarNet.User]::Data['FarRedisProcess'].HasExited) {
		[FarNet.User]::Data['FarRedisProcess'] = $null
	}
}

# start pair Far
if (![FarNet.User]::Data['FarRedisProcess']) {
	# reset the flag and start
	Remove-RedisKey far:RedisProcess
	Start-Far ps:Register-FarRedisTask $Far.CurrentDirectory -Environment @{FAR_REDIS_PROCESS = $PID}

	# wait for the flag value
	if ($id = Wait-RedisString far:RedisProcess ([timespan]::FromSeconds(0.5)) ([timespan]::FromSeconds(10))) {
		[FarNet.User]::Data['FarRedisProcess'] = Get-Process -Id $id -ErrorAction Ignore
	}
	if (![FarNet.User]::Data['FarRedisProcess']) {
		return
	}
}

# message data
$Data = $Data ? @{} + $Data : @{}
$Data._pub = $PID
$Data._sub = [FarNet.User]::Data['FarRedisProcess'].Id
$Data._task = $Task.ToString()

# send message
$null = $db.Publish('FarRedisSub', ($Data | ConvertTo-Json -Depth 99 -Compress))
