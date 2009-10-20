
<#
.SYNOPSIS
	Shows a message box with data and choices.
	Author: Roman Kuzmin

.DESCRIPTION
	It is mostly for debugging, e.g. it can be used as a temporary hardcoded
	breakpoint. Input data are sent as arguments or piped. Data are shown
	together with their type names.

.EXAMPLE
	Debug-Msg- $Far
	$Far | Debug-Msg-
#>

# message body: data with their types
$dbg_message = $(if ($args) { $args } else { $input }) | &{process{
	if ($_ -eq $null) {
		'<null>'
	}
	else {
		$_
		"`r[" + $_.GetType().FullName + ']'
		($_ | Format-List * -Force | Out-String -Width ([console]::WindowWidth - 17)).Trim()
	}
}} | Out-String

# message title: invocation info
if ($args[0] -is [System.Management.Automation.ErrorRecord]) {
	$dbg_invocation = $args[0].InvocationInfo
}
else {
	$dbg_invocation = $MyInvocation
}

# show message box with choices
for(;;) {
	switch($Far.Msg(
		$dbg_message,
		$MyInvocation.PositionMessage,
		'LeftAligned',
		('&Close', '&Suspend', '&GoTo', '&Throw', '&Exit')
	)) {
		1 {
			$Psf.ShowConsole()
		}
		2 {
			if ($dbg_invocation.ScriptName -and (Test-Path $dbg_invocation.ScriptName)) {
				Start-FarEditor $dbg_invocation.ScriptName $dbg_invocation.ScriptLineNumber -Modal
			}
		}
		3 {
			throw $args[0]
		}
		4 {
			exit
		}
		default {
			return
		}
	}
}
