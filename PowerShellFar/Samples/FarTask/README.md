# Start-FarTask samples

There are two kind of scripts:

- Scripts `*.far.ps1` are usual scripts. They may work with `$Far` as usual and
  then they call `Start-FarTask`, usually as the last command before exiting.

- Scripts `*.fas.ps1` are invoked by `Start-FarTask`, for example by the
  association command defined as `ps: Start-FarTask (Get-FarPath) #`.
  They may work with `$Far` only using `job` blocks.

Apart from practical async applications, `Start-FarTask` is suitable for tests
implemented as task scripts. For example, the script `KeysAndMacro.fas.ps1` is
test like: it does not require user interaction and it checks for the expected
results.
