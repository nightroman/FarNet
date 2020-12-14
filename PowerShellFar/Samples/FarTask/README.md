# Start-FarTask samples

`Start-FarTask` starts task scripts with Far job blocks and macros.

There are two kind of scripts:

- Scripts `*.far.ps1` are usual scripts. They may work with FarNet and then
  they call `Start-FarTask`, usually as the last command before exiting.

- Scripts `*.fas.ps1` are invoked by `Start-FarTask`, for example by the
  association command defined as `ps: Start-FarTask (Get-FarPath) #`.
  They may work with FarNet only using various job blocks.

[Basics.fas.ps1]: Basics.fas.ps1
[NestedTasks.fas.ps1]: NestedTasks.fas.ps1
[DialogNonModalInput.fas.ps1]: DialogNonModalInput.fas.ps1
[InputEditorMessage.fas.ps1]: InputEditorMessage.fas.ps1

Task scripts may call other tasks and consume their results, if any. Example:
[NestedTasks.fas.ps1] calls [DialogNonModalInput.fas.ps1] and uses its result
as the parameter for the next task [InputEditorMessage.fas.ps1].

[KeysAndMacro.fas.ps1]: KeysAndMacro.fas.ps1
[Test-Dialog.fas.ps1]: Test-Dialog.fas.ps1

Apart from practical async applications, `Start-FarTask` is suitable for tests
implemented as task scripts. For example, the script [KeysAndMacro.fas.ps1] is
test like: it does not require user interaction and it checks for the expected
results. Other tests: [Basics.fas.ps1] and [Test-Dialog.fas.ps1].

## Tips and tricks

### Share data between tasks and jobs

Use `[FarNet.User]::Data`, the concurrent dictionary, for sharing global data
between different tasks. Using PowerShell global variables of the main session
is not ideal. Tasks scripts are invoked in separate sessions and cannot access
main session variables directly. Using jobs for this is possible but tedious
for such a simple task.

See [ConsoleGitStatus.far.ps1](ConsoleGitStatus.far.ps1), it uses the global
data in order to run and stop the only task instance.

### Output task messages to console

Tasks running in the background may output their messages to the console in
order to notify about the progress, error, completion. This way may be less
intrusive than, say, showing message boxes. The `ps:` blocks are designed
for jobs with console output, as if they are called from the command line.

See [ConsoleGitStatus.far.ps1](ConsoleGitStatus.far.ps1), it prints some git
info when the current panel directory changes.

### Beware of unexpected current paths

Keep in mind, there are several different current paths in tasks and jobs and
they all may be out of sync. Some interesting paths:

- Far Manager process current directory, %FARHOME%, normally does not change
- Far Manager internal current directory, depends on the active panel path
- Task current location, normally its start panel directory, may change
- Job current location, it is the main session current location

See [CurrentLocations.fas.ps1](Case/CurrentLocations.fas.ps1), it shows how
they change and may be same or different depending on actions.

### Expose all task or job variables

If a task or job creates many variables to be shared, then instead of saving
them in shared `$Data` you may just expose all variables:

```powershell
$Data.Var = $ExecutionContext.SessionState.PSVariable
```

Then later get variable values as `$Data.Var.GetValue('myVar')`.
