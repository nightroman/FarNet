
# 2016-08-25 FSharp.Compiler.Service requires MSBuild 12

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
