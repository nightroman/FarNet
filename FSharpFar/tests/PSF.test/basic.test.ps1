Set-StrictMode -Version 3

task invalid-parameter-open {
	try { throw $Far.InvokeCommand("fs:project open=z") }
	catch { equals $_.Exception.InnerException?.Message "Command: project`r`nParameter 'open': Invalid value 'z'. Valid values: VS, VSCode." }
}

task invalid-parameter-type {
	try { throw $Far.InvokeCommand("fs:project type=z") }
	catch { equals $_.Exception.InnerException?.Message "Command: project`r`nParameter 'type': Invalid value 'z'. Valid values: Normal, Script." }
}
