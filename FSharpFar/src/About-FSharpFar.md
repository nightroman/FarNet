# About FSharpFar

- [FSharp.Compiler.Service](https://github.com/fsharp/FSharp.Compiler.Service)
- [F# 4.7](https://devblogs.microsoft.com/dotnet/announcing-f-4-7/)
- [F# 4.6](https://blogs.msdn.microsoft.com/dotnet/2019/01/24/announcing-f-4-6-preview/)
- [FCS issue F# 4.7](https://github.com/fsharp/FSharp.Compiler.Service/issues/912)
- [FCS issue F# 4.6](https://github.com/fsharp/FSharp.Compiler.Service/issues/884)

***
### FCS Packages

see .\packages\FSharp.Compiler.Service\FSharp.Compiler.Service.nuspec

    <dependency id="System.Collections.Immutable" version="1.5.0" exclude="Build,Analyzers" />
    <dependency id="System.Reflection.Metadata" version="1.6.0" exclude="Build,Analyzers" />
    <dependency id="System.ValueTuple" version="4.4.0" exclude="Build,Analyzers" />

or see <https://github.com/fsharp/FSharp.Compiler.Service/blob/master/fcs/FSharp.Compiler.Service/FSharp.Compiler.Service.fsproj>

DO:

    "System.Collections.Immutable" Version="1.5.0"
    "System.Reflection.Metadata" Version="1.6.0"

-- need to pin, to keep in sync, latest versions make issues -- more dependencies??

    "System.ValueTuple" version="4.4.0"

-- we use System.ValueTuple (4.5) fine, keep using, mind Far.exe.config

***
### FSharp.Core assembly and package versions

Versions are different, 2019-02-02:

- Assembly version is 4.6.0. This version must be in `Far.exe.config`.
- Package version is 4.6.1 and it may change further.

***
### FSharpFar.fs.ini and module FSharpFar.X

UPDATE: We retired `Convert-Project.ps1`, so the problem is no more.

If we use `Convert-Project.ps1` to make `FSharpFar.fs.ini` then
some files works fine, some fail with

    System.Exception: not an assembly reference

Namely, files before

    load=.\FarModule.fs

work. `FarModule.fs` and all after it fails.

This line is the culprit:

    module FSharpFar.FarModule

If I change names then all is fine.
But it is legal and VS compiles.

It looks like an FCS bug.

***
### Editor flow notes

Why `post ... Open`. In modal windows `Open` blocks and the job code after
`Open` is not called till the exit (and that is why `Open` must be the last
code in its function). But `post` is not blocking, so the flow continues
even if `Open` blocks.

Why `.Closed.Add`. I do not have cases but it is possible that something closes
the editor before our next step. In this case we should skip waiting for exit,
all is done. So add the handler as store `closed`.

Why `try ... Open`. `Open` may throw. We cannot properly deal with an
error in the posted code, so we keep the error for the next flow code.

Next step. If `Open` failed then raise the stored error. If the editor is
already closed the do nothing. Else create and return the waiter. We cannot
wait in Far code.

Next step. The waiter waits.

***
### fsi object

**The bad.**

[This](https://fsharp.github.io/FSharp.Compiler.Service/interactive.html) tells how to enable `fsi` in a session.
It does not tell that *FSharp.Compiler.Interactive.Settings.dll* from *Microsoft SDKs* should be packaged for this.
If this DLL is not discovered, an app fails with not clear info.

- [692](https://github.com/fsharp/FSharp.Compiler.Service/issues/692)
- [127](https://github.com/fsharp/FSharp.Compiler.Service/issues/127)

In theory, we can make `fsi` available for scripts as

```FSharp
/// FSharpFar utilities available right away.
[<AutoOpen>]
module FSharpFarUtility
/// Interactive settings.
let fsi = FSharp.Compiler.Interactive.Shell.Settings.fsi
```

but this requires the reference to *FSharp.Compiler.Service*.
We do not want this for all scripts.

**The good.**

This fine tuning is not often needed.
When it is needed, a documented workaround exists, see *samples\fsi*.

NB This object seems to be global, if we change data in one session then all sessions are affected.

***
### System.Collections.Immutable and System.Reflection.Metadata

They are used in `FSharp.Compiler.Service\src\absil`

- `ilsign.fs`
- `ilwritepdb.fs`
- `ilwritepdb.fsi`

We do not package them.
Is there a use case when this is a problem?

### 2016-10-21 v1.0, self-contained package

**F# core is included. Why Far home?**

*FSharp.Core.dll* can probably live in some standard assembly search locations, not necessarily Far home.
But FCS also needs *FSharp.Core.optdata* and *FSharp.Core.sigdata* and its search is not the same.
Far home works in both cases, so let it be the location of our packaged F# core files.

**MSBuild is no longer needed with FCS 8.0**

See [#631](https://github.com/fsharp/FSharp.Compiler.Service/issues/631).
In 8.0 MSBuild is still loaded (not used) if it is present.

### 2016-09-04 `//exec` and console output

Intercept `Console.Out` / `Error` and use `ShowUserScreen` / `SaveUserScreen` automatically.

So `//exec` is fine for any script, except operating on console buffer.

**IDEA**
Intercept `Out` and `Error` separately. Different colors?

### 2016-08-26 How `#r` works

[from](https://sergeytihon.wordpress.com/2014/07/23/avoid-using-relative-paths-in-r-directives/)

> `#r` means to reference by dll-path; focusing on name. This means that FSI will
use the file name first, looking in the system-wide search path and only then
try to use the string after #r as a directory-relative hint

> So that means, `#r` is not reliable way to reference assemblies. You can get
into the situation when your script depends on the environment: assemblies in
GAC, installed software (like version of ASP.NET) and so on. To avoid this it
is better to explicitly specify an assembly search path (`#I`) and then
reference the assembly:

````
    #I "../packages/nuget.core.2.8.2/lib/net40-Client"
    #r "Nuget.Core.dll"
    #r "System.Xml.Linq.dll"
````

### 2016-08-25 FSharp.Compiler.Service requires MSBuild 12

FSharp.Compiler.Service depends on MSBuild 12 (VS 2013).

The team decided to use assembly redirection to MSBuild 14 (VS 2015).

- [#602](https://github.com/fsharp/FSharp.Compiler.Service/issues/602).
- [App.config](https://github.com/fsharp/FSharp.Compiler.Service/blob/61fb67134197c621411359ef1597360023c3775b/src/fsharp/FSharp.Compiler.Service.ProjectCrackerTool/App.config)

So we use `Far.exe.config` with assembly redirection and FSharpFar requires MSBuild 14.

````
    <dependentAssembly>
      <assemblyIdentity name="Microsoft.Build.Framework" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
      <bindingRedirect oldVersion="0.0.0.0-14.0.0.0" newVersion="14.0.0.0" />
    </dependentAssembly>
    <dependentAssembly>
      <assemblyIdentity name="Microsoft.Build.Engine" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
      <bindingRedirect oldVersion="0.0.0.0-14.0.0.0" newVersion="14.0.0.0" />
    </dependentAssembly>
````

**Q**
Will this work on a machine with MSBuild 12?

**N**
They also redirect FSharp.Core.
We just keep this in mind.

````
    <dependentAssembly>
      <assemblyIdentity name="FSharp.Core" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
      <bindingRedirect oldVersion="2.0.0.0-4.3.1.0" newVersion="4.4.0.0"/>
    </dependentAssembly>
````
