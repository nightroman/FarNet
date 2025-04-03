# Start-FarTask samples

`Start-FarTask` starts task scripts with jobs and macros.

To view help, use:

    vps: help -full Start-FarTask

There are two kind of scripts:

- Scripts `*.far.ps1` are normal scripts. They work with FarNet as usual and
  then they call `Start-FarTask` with the task script and prepared data.

- Scripts `*.fas.ps1` are task scripts. They are invoked by `Start-FarTask`,
  for example by the association `ps: Start-FarTask (Get-FarPath)`.

How to run `*.fas.ps1` normally:

    ps: Start-FarTask .\Basics.fas.ps1

How to run `*.fas.ps1` by steps:

    ps: Start-FarTask .\Basics.fas.ps1 -Step

Keep clicking Continue and see steps in the debugger console and their actions in Far Manager.\
Get `Add-Debugger.ps1` from PSGallery -- <https://www.powershellgallery.com/packages/Add-Debugger>

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

### Share data between tasks

Use `[FarNet.User]::Data`, the concurrent dictionary, for sharing global data
between different tasks.

See [ConsoleGitStatus.far.ps1](ConsoleGitStatus.far.ps1), it uses the global
data in order to run and stop the only task instance.

### Output task messages to console

Tasks running in the background may output their messages to the console in
order to notify about the progress, error, completion. This way may be less
intrusive than, say, showing message boxes. Use the `ps:` jobs. They output
to the console, as if they are called from the command line.

See [ConsoleGitStatus.far.ps1](ConsoleGitStatus.far.ps1), it prints some git
info when the current panel directory changes.

### Beware of various current paths

Keep in mind, there are various current paths in tasks and jobs and they may or
may not be the same. Interesting paths:

- Far Manager process current directory, usually where it starts
- Far Manager internal current directory, the active panel path
- Task current location, usually where it starts, may change
- Job current location, the main session current location

See [CurrentLocations.fas.ps1](Case/CurrentLocations.fas.ps1), it shows how
these paths change and may be same or different depending on actions.
