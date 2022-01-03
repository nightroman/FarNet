
(*
    Our config must be used on getting options from script. This test sets
    `--lib` in the config file -> `#r "PowerShellFar.dll"` should work.

    NB Use full paths for `--lib` to avoid problems. F# looks for DLLs in lib
    paths but it does not combine lib paths in the same way.
*)

#r "PowerShellFar.dll"
open PowerShellFar

Job.Jobs
