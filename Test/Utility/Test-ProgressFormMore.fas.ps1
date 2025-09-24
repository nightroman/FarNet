<#
.Synopsis
	Test progress form with tasks.
#>

### Task completes
job {
	$progress = New-Object FarNet.Tools.ProgressForm
	$progress.Title = "Show(Task) complete"

	$task = Start-FarTask -AsTask {
		Start-Sleep 1
	}

	$r = $progress.Show($task)
	Assert-Far $r -eq $true
}

### Task cancels by OperationCanceledException
job {
	$progress = New-Object FarNet.Tools.ProgressForm
	$progress.Title = "Show(Task) cancel exception"

	$task = Start-FarTask -AsTask {
		Start-Sleep 1
		throw [OperationCanceledException]''
	}

	$r = $progress.Show($task)
	Assert-Far $r -eq $false
	$global:Error.Clear()
}

### Task throws generic exception.
job {
	$progress = New-Object FarNet.Tools.ProgressForm
	$progress.Title = "Show(Task) other exception"

	$task = Start-FarTask -AsTask {
		Start-Sleep 1
		throw 'Test exception.'
	}

	$r = try {$progress.Show($task)} catch {$_}
	Assert-Far "$r" -eq 'Exception calling "Show" with "1" argument(s): "Test exception."'
	Assert-Far $r.Exception.InnerException.Message -eq 'Test exception.'
	$global:Error.Clear()
}

### User cancels the form and task.
run {
	$progress = New-Object FarNet.Tools.ProgressForm
	$progress.Title = "Show(Task) user cancel"
	$progress.CanCancel = $true

	$null = $progress.CancellationToken.Register({$Data._220709_2007 = 'CancellationToken action called'})

	$task = Start-FarTask -AsTask -Data progress, Data {
		while(!$Data.progress.CancellationToken.IsCancellationRequested) {
			Start-Sleep -Milliseconds 30
		}
		$Data.Data._220709_1006 = 'Exiting canceled task'
	}

	$Data._220709_0637 = $progress.Show($task)
}
job {
	Assert-Far -Dialog
	Assert-Far $Far.Dialog[0].Text -eq "Show(Task) user cancel"
	$Far.Dialog.Close()
}
job {
	Start-Sleep -Milliseconds 300
	Assert-Far $Data._220709_0637 -eq $false
	Assert-Far $Data._220709_1006 -eq 'Exiting canceled task'
	Assert-Far $Data._220709_2007 -eq 'CancellationToken action called'
}
