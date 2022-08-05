# JavaScriptFar interop

JavaScriptFar provides interop functions for other FarNet modules or scripts in
PowerShell, F#. They may be even useful in JavaScript for cross session calls.

**Step 1.** Get the module manager

```powershell
$ModuleManager = $Far.GetModuleManager('JavaScriptFar')
```

**Step 2.** Get one of the required functions

```powershell
$EvaluateCommand = $ModuleManager.Interop('EvaluateCommand', $null)
$EvaluateDocument = $ModuleManager.Interop('EvaluateDocument', $null)
```

`$EvaluateCommand` and `$EvaluateDocument` are delegates `Func<string, IDictionary, object>`.
The first argument is JavaScript command or file path. The second is a
dictionary of named parameters (`args` in JavaScript code). They return
an object, the last JavaScript statement result.

**Step 3.** Invoke the obtained function

## Example 1

[app1.far.ps1](app1.far.ps1)

This commands sends the object `$Host` to a JavaScript command as the parameter
named `LiveObject`. The command simply obtains and "returns" this object. The
live object travels from PowerShell to JavaScript and returns:

```powershell
$EvaluateCommand.Invoke('args.LiveObject', $PSScriptRoot,  @{LiveObject = $Host})
```

The example is contrived and but it gives the idea.
Interop call scenarios may have rich input and output.

## Example 2

[app2.far.ps1](app2.far.ps1)

This PowerShell command calls a script and gets it result:

```powershell
$user = $EvaluateDocument.Invoke("$PSScriptRoot\input.js", $null)
```

## Example 3

Interop and live objects open doors for interesting scenarios. For example we
can interactively run PowerShell commands in Far Manager using... VSCode debug
console.

See the doc comments in [PS-in-JS-debugger.far.ps1](PS-in-JS-debugger.far.ps1).

This example is a pattern for interactively driving something from VSCode:
- Create a .NET object with members designed for this scenario.
- Pass it in a crafted JavaScript code with more helpers.
- In VSCode debug console call helpers interactively.
