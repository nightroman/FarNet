# This script shows how to combine task scripts.
# Jobs call and return `Start-FarTask ... -AsTask`.

# Run DialogNonModalInput.fas.ps1 and keep the result in $Data.
$Data.Text = job {
	Start-FarTask $PSScriptRoot\DialogNonModalInput.fas.ps1 -AsTask
}

# Run InputEditorMessage.fas.ps1 using $Data.Text as the parameter.
job {
	Start-FarTask $PSScriptRoot\InputEditorMessage.fas.ps1 -Text $Data.Text -AsTask
}
