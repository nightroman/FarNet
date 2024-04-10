<#
.Synopsis
	How to use Send-FarRedisTask.ps1

.Description
	Scenario:
	- edit file in a new Far, make it non-modal and with panels available
	- close Far when the editor closes but only if nothing else is opened
	- send some task back to the first Far when the editor closes

	For more advanced scenario see Bench/Edit-FarFileSync.ps1
#>

# get the cursor file in editor, viewer, panel
$file = $Far.FS.CursorFile
if (!$file) {return}

# from this Far 1, send data and task to another Far 2
Send-FarRedisTask -Data @{file = $file.FullName} {
	# open non-modal editor in Far 2 and wait for exit
	job {
		$editor = $Far.CreateEditor()
		$editor.FileName = $Data.file
		[FarNet.Tasks]::Editor($editor)
	}

	# send data and task back to Far 1
	Send-FarRedisTask -Data @{file = $Data.file} {
		# show message in Far 1
		run {
			Show-FarMessage "Finished editing $($Data.file)"
		}
	}

	# exit Far 2 if nothing is opened
	job {
		if ($Far.Window.Count -eq 2) {
			$Far.Quit()
		}
	}
}
