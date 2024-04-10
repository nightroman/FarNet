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
		Specifies the Far task to be sent to another Far.

.Parameter Data
		Optional data to be sent as JSON and then used by Start-FarTask.
		The Task script uses $Data containing the original and meta data.
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

$ErrorActionPreference = 1

# message data
$Data = $Data ? @{} + $Data : @{}
$Data._task = $Task.ToString()

# subscribe to messages
if (![FarNet.User]::Data['FarRedisSub'] -and ([runspace]::DefaultRunspace.Id -eq 1)) {
	Register-FarRedisTask
}

# check pair Far
if ([FarNet.User]::Data['FarRedisPair']) {
	if ([FarNet.User]::Data['FarRedisPair'].HasExited) {
		[FarNet.User]::Data['FarRedisPair'] = $null
	}
}

if ([FarNet.User]::Data['FarRedisPair']) {
	# send data
	$null = [FarNet.User]::Data['FarRedisDB'].Publish(
		"FarRedisSub:$([FarNet.User]::Data['FarRedisPair'].Id)",
		($Data | ConvertTo-Json -Depth 99 -Compress)
	)
}
else {
	# keep data and start pair Far
	[FarNet.User]::Data['FarRedisData'] = $Data
	Start-Far ps:Register-FarRedisTask -Environment @{FAR_REDIS_PAIR = $PID}
}
