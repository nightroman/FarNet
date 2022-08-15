# Assert/Debug moved from _220809_2057

function global:Set-AddDebuggerIO {
	Set-Alias Read-Debugger Read-Debugger2 -Scope global -Option AllScope
	function global:Read-Debugger2 {
		param($Prompt, $Default)
		Read-Host $Prompt
	}

	Set-Alias Write-Debugger Write-Debugger2 -Scope global -Option AllScope
	function global:Write-Debugger2 {
		param($Data)
	}
}

function global:Remove-AddDebuggerIO {
	Remove-Alias Read-Debugger -Scope global -ErrorAction 0
	Remove-Alias Write-Debugger -Scope global -ErrorAction 0
}
