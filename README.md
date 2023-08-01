<img src="https://github.com/nightroman/FarNet/raw/main/Zoo/FarNetLogo.png" align="right"/>

# FarNet

Far Manager platform for .NET modules and scripts in PowerShell, F#, JavaScript

- [Wiki](https://github.com/nightroman/FarNet/wiki) - framework, modules, libraries
- [Issues](https://github.com/nightroman/FarNet/issues) - bug reports and problems
- [Discussions](https://github.com/nightroman/FarNet/discussions) - questions and ideas

## Prerequisites

**.NET 7**

Download and install [.NET 7 SDK or runtime](https://aka.ms/dotnet/download), **x64 or x86 depending on Far Manager**.\
SDK is needed for developing FarNet modules but recommended in any case.

Check for existing installations by these commands:

    dotnet --version
    dotnet --info

**Visual C++ Redistributable**

Look at the installed programs and find entries like:

- Microsoft Visual C++ 2015-2022 Redistributable (x64)
- Microsoft Visual C++ 2015-2022 Redistributable (x86)

If they are missing, then install the required.
Links are not provided, they keep changing.

**Far Manager**

Choose the required from [downloads](https://www.farmanager.com/download.php?l=en).\
Normally the stable build is recommended.

## Install using PowerShell

This way avoids manual steps and allows updates later.

Close Far Manager and start the PowerShell console.

Change to the Far Manager directory

```powershell
Set-Location "C:\Program Files\Far Manager"
```

Import Far package functions

```powershell
Invoke-Expression (Invoke-WebRequest https://raw.githubusercontent.com/nightroman/FarNet/main/web.ps1)
```

If it fails on older systems, try

```powershell
[Net.ServicePointManager]::SecurityProtocol = "Tls11,Tls12,$([Net.ServicePointManager]::SecurityProtocol)"
Invoke-Expression (New-Object Net.WebClient).DownloadString('https://raw.githubusercontent.com/nightroman/FarNet/main/web.ps1')
```

Install FarNet

```powershell
Install-FarPackage FarNet
```

Install modules

```powershell
Install-FarPackage FarNet.CopyColor
Install-FarPackage FarNet.Drawer
Install-FarPackage FarNet.EditorKit
Install-FarPackage FarNet.Explore
Install-FarPackage FarNet.FolderChart
Install-FarPackage FarNet.FSharpFar
Install-FarPackage FarNet.GitKit
Install-FarPackage FarNet.JavaScriptFar
Install-FarPackage FarNet.PowerShellFar
Install-FarPackage FarNet.RightControl
Install-FarPackage FarNet.RightWords
Install-FarPackage FarNet.Vessel
```

Install libraries

```powershell
Install-FarPackage FarNet.FSharp.Charting
Install-FarPackage FarNet.FSharp.Data
Install-FarPackage FarNet.FSharp.PowerShell
Install-FarPackage FarNet.FSharp.Unquote
Install-FarPackage FarNet.ScottPlot
Install-FarPackage FarNet.SQLite
```

You may start Far Manager after this.
Modules are installed in `%FARHOME%\FarNet\Modules`.
Libraries are installed in `%FARHOME%\FarNet\Lib`.


## Update using PowerShell

FarNet packages installed by `Install-FarPackage` may be updated in the same way.

Close Far Manager, open PowerShell console, and invoke

```powershell
Set-Location "C:\Program Files\Far Manager"
Invoke-Expression (Invoke-WebRequest https://raw.githubusercontent.com/nightroman/FarNet/main/web.ps1)
```

To update all packages, use

```powershell
Update-FarPackage
```

To update one package, use `Install-FarPackage`

```powershell
Install-FarPackage FarNet.PowerShellFar
```

To remove one package, use `Uninstall-FarPackage`

```powershell
Uninstall-FarPackage FarNet.PowerShellFar
```

## Install packages manually

Given a package `Bar`, download it as:

    https://nuget.org/api/v2/package/Bar

The downloaded file name is `Bar.<version>.nupkg`. This is a zip archive, you
may save it with the zip extension for easier unpacking.

All needed files are in the folder `tools`. This folder contains `FarHome` and
may contain `FarHome.x64` and `FarHome.x86` folders.

Copy `FarHome` items to the Far Manager home directory preserving the folder
structure. For example, by this command in Far Manager:

    robocopy FarHome "%FARHOME%" /s

If `FarHome.x64` and `FarHome.x86` exist then, depending on x64 or x86, copy
items of `FarHome.x64` or `FarHome.x86` to Far Manager:

    robocopy FarHome.x64 "%FARHOME%" /s
    robocopy FarHome.x86 "%FARHOME%" /s
