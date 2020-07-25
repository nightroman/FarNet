module TestPSFar
open FarNet.FSharp

// PowerShellFar unwraps PSObject unless its BaseObject is PSCustomObject.
// In this case the original PSObject is returned.
[<Test>]
let PSCustomObject () =
    let r = PSFar.Invoke """ $Host; [PSCustomObject]@{name='foo'; version='bar'} """
    Assert.Equal(2, r.Length)

    let r1 = r.[0]
    Assert.Equal("System.Management.Automation.Internal.Host.InternalHost", r1.GetType().FullName)

    let r2 = r.[1]
    Assert.Equal("System.Management.Automation.PSObject", r2.GetType().FullName)

// PowerShellFar runspace is designed for advanced uses, for example with
// System.Management.Automation or FarNet.FSharp.PowerShell NuGet library.
[<Test>]
let Runspace () =
    let r1 = PSFar.Runspace
    let r2 = (PSFar.Invoke "[runspace]::DefaultRunspace").[0]
    Assert.True(obj.ReferenceEquals(r1, r2))
