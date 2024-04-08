<#
.Synopsis
	How to use Send-FarRedisTask.ps1

.Description
	Sending tasks between two paired Far instances.
#>

# send the task from this (1) to another started Far (2)
Send-FarRedisTask {
	# show ping message in Far 2
	job {
		Show-FarMessage "Ping to $($Data._sub) from $($Data._pub)"
	}

	# send the task from Far 2 back to Far 1
	Send-FarRedisTask {
		# show pong message in Far 1
		job {
			Show-FarMessage "Pong to $($Data._sub) from $($Data._pub)"
		}
	}

	# exit Far 2
	job {
		$Far.Quit()
	}
}
