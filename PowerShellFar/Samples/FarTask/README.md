# Start-FarTask samples

There are two kind of scripts:

- Scripts `*.far.ps1` are usual scripts. They may work with FarNet and then
  they call `Start-FarTask`, usually as the last command before exiting.

- Scripts `*.fas.ps1` are invoked by `Start-FarTask`, for example by the
  association command defined as `ps: Start-FarTask (Get-FarPath) #`.
  They may work with FarNet only using `job` and `run` blocks.

Task scripts may call other tasks and consume their results, if any. Example:
`NestedTasks.fas.ps1` calls `DialogNonModalInput.fas.ps1` and uses its result
as the parameter for the next task `InputEditorMessage.fas.ps1`.

Apart from practical async applications, `Start-FarTask` is suitable for tests
implemented as task scripts. For example, the script `KeysAndMacro.fas.ps1` is
test like: it does not require user interaction and it checks for the expected
results.
