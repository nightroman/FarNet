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
		Optional data sent as JSON and then converted back to hashtable.
		The received hashtable is provided for the Task script as $Data.

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

$ErrorActionPreference = 1

# check pair Far
if ($FarRedisPair = [FarNet.User]::Data['FarRedisPair']) {
	if ($FarRedisPair.HasExited) {
		[FarNet.User]::Remove('FarRedisPair')
		if ($SkipExited) {
			return $true
		}
	}
}

# message data
$Data = [ordered]@{
	Data = $Data
	Task = $Task.ToString()
}

if ($FarRedisPair = [FarNet.User]::Data['FarRedisPair']) {
	# send data
	$null = [FarNet.User]::Data['FarRedisDB'].Publish(
		"FarRedisSub:$($FarRedisPair.Id)",
		($Data | ConvertTo-Json -Depth 99 -Compress)
	)
}
else {
	# subscribe to messages
	if (![FarNet.User]::Data['FarRedisSub']) {
		Register-FarRedisTask
	}

	# keep data and start pair Far
	[FarNet.User]::Data.FarRedisData = $Data
	Start-Far ps:Register-FarRedisTask $Far.CurrentDirectory -Environment @{FAR_REDIS_PAIR = $PID}
}
