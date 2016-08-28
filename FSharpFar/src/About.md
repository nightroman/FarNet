
# 2016-08-26 How `#r` works

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
