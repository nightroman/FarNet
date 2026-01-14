<#
.Synopsis
	How to combine task scripts.

.Description
	Jobs call `Start-FarTask ... -AsTask` and return tasks.
	The job runner awaits tasks and outputs their results.
#>

# Run DialogNonModalInput.fas.ps1 and keep the result as $text.
$text = fun {
	Start-FarTask $PSScriptRoot\DialogNonModalInput.fas.ps1 -AsTask
}

# Run InputEditorMessage.fas.ps1 using the result $text as $Var.text.
fun {
	Start-FarTask $PSScriptRoot\InputEditorMessage.fas.ps1 -Text $Var.text -AsTask
}
