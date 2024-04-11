<#
.Synopsis
	How to use Send-FarRedisTask.ps1

.Description
	Sending tasks between two paired Far instances with some data.

	Scenario:
	- edit file in a new Far as non-modal and with panels available
	- when the editor closes, send some task back to the first Far
	- quit if nothing else is opened
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
