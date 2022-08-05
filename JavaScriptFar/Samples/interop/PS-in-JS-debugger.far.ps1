<#
.Synopsis
	How to run PowerShellFar from VSCode debug console.

.Description
	Assuming you have setup VSCode debugger (see JavaScriptFar/README)
	- Change to this panel and F11 / JavaScriptFar / Start debugging.
	- When VSCode opens, start debugger there (F5).
		(do not open any files or set breakpoints)
	- Then run this script in PowerShellFar:
		ps: .\PS-in-JS-debugger.far.ps1
	- Debugger stops at the `debugger` statement in the below `$code`.
	- In debug console run `ps(...)` and see results:
		ps('dir')
		ps('$Far')
		...
	- At this point Far Manager is blocked because this script is running.
	To unblock, F5 in VSCode. The script exits and unblocks Far Manager.
	- But VSCode debugger and the function `ps` still work:
		ps('$Far.Message("Hello from VSCode debugger!")')
		...
#>

$ModuleManager = $Far.GetModuleManager('JavaScriptFar')
$EvaluateCommand = $ModuleManager.Interop('EvaluateCommand', $null)

# this JS code is called via interop
$code = @'
// keep the runner
runner = args.runner

// run and log output
function ps(code) {
    console.log(runner.Run(code))
}

// break
debugger
'@

# this PS runner is passed in JS code
class PowerShellRunner {
	[string] Run([string]$code) {
		$ErrorView = 'ConciseView'
		$_ = [scriptblock]::Create($code)
		return ($(try {. $_} catch {$_}) | Out-String).Trim()
	}
}

# call JS with the PS runner parameter
$null = $EvaluateCommand.Invoke($code, @{
	runner = [PowerShellRunner]::new()
})
