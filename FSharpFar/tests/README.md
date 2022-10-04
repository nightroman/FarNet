# FSharpFar tests

Tests cover FSharpFar and also some FarNet and PowerShellFar.

How to run all tests:

    fs: exec: file=App1.fsx

How to run a particular function:

    fs: ModuleName.FunctionName()

How to run a particular async job:

    fs: test ModuleName.AsyncName
