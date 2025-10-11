<#
.Synopsis
	Shows how to register commands dynamically.

.Description
	Steps:
	- invoke this script to register the command
	- type in the command line
		mycmd: <any text>
#>

Register-FarCommand -Prefix mycmd 'PSF test command' 053a9a98-db98-415c-9c80-88eee2f336ae {
	Show-FarMessage ($_ | Format-List | Out-String).Trim()
}

Show-FarMessage 'Command prefix "mycmd:" is registered.'
