<#
.Synopsis
	How to use Send-FarRedisTask.ps1

.Description
	Sending tasks between two paired Far instances.
#>

# send from this Far 1 to another Far 2
Send-FarRedisTask {
	# show ping message in Far 2
	job {
		Show-FarMessage Ping
	}

	# send from Far 2 to Far 1
	Send-FarRedisTask {
		# show pong message in Far 1
		job {
			Show-FarMessage Pong
		}
	}

	# exit Far 2
	job {
		$Far.Quit()
	}
}
