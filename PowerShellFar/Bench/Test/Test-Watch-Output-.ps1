
<#
.Synopsis
	Test the script Watch-Output-.ps1.
	Author: Roman Kuzmin

.Description
	Requires:
	- Format-Chart.ps1 https://github.com/nightroman/PowerShelf

	The script shows some features of Watch-Output-.ps1
	- combined output of several commands;
	- output of external console program;
	- work with long and many lines.

	Just invoke the test and watch changes in the editor. Then try to scroll
	horizontally (test long lines) and vertically (test many lines).

.Link
	Watch-Output-.ps1
#>

Assert-Far ((Get-Command Format-Chart.ps1 -ErrorAction 0) -ne $null) "Format-Chart.ps1 is not found." "Assert"

Watch-Output- -Title "EXAMPLE: COMBINED OUTPUT" {

	"`n===== SOME FREQUENTLY CHANGED DATA ====="
	"Current time : $(Get-Date)"
	"Working set  : $([int]((Get-Process -Id $Pid).WorkingSet / 1Mb)) Mb"
	"Managed mem  : $([int]([gc]::GetTotalMemory($false) / 1Mb)) Mb"

	"`n===== OUTPUT OF POWERSHELL COMMANDS ====="
	Get-Process | ?{$_.WS -gt 10mb} | Format-Chart.ps1 Name, WS

	"`n===== OUTPUT OF CONSOLE APPLICATIONS ====="
	netstat -e

	"`n===== LONG AND MANY LINES (YOU CAN SCROLL) ====="
	++$global:x
	[string]($x..($x + 100))
	($x + 1)..($x + 100)
}
