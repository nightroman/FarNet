<#
.Synopsis
	Subscribes to Far task messages from another Far.
	Author: Roman Kuzmin

.Description
	This script should be in the path for using by Send-FarRedisTask.ps1.

.Parameter Unregister
		Tells to unregister this subscription.
#>

[CmdletBinding()]
param(
	[switch]$Unregister
)

Set-StrictMode -Version 3
$ErrorActionPreference = 1

Import-Module FarNet.Redis
$db = Open-Redis localhost:3278

if ([FarNet.User]::Data['FarRedisSub']) {
	if ($Unregister) {
		Unregister-RedisSub FarRedisSub ([FarNet.User]::Data['FarRedisSub'])
		[FarNet.User]::Data['FarRedisSub'] = $null
		[FarNet.User]::Data['FarRedisProcess'] = $null
	}
	return
}

[FarNet.User]::Data['FarRedisSub'] = Register-RedisSub FarRedisSub {
	param($channel, $message)

	$data = ConvertFrom-Json $message -AsHashtable

	if ($data._pub -eq $PID -or $data._sub -ne $PID) {
		return
	}

	Start-FarTask -Data $data ([scriptblock]::Create($data._task))
}

if ($pid2 = $env:FAR_REDIS_PROCESS) {
	$env:FAR_REDIS_PROCESS = $null

	$far2 = Get-Process -Id $pid2 -ErrorAction Ignore
	if (!$far2 -or $far2.HasExited) {
		return
	}

	[FarNet.User]::Data['FarRedisProcess'] = $far2
	Set-RedisString far:RedisProcess $PID -Expiry ([timespan]::FromSeconds(20))
}
