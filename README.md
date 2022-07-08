<img src="https://raw.githubusercontent.com/wiki/nightroman/FarNet/images/FarNetLogo.png" align="right"/>

# FarNet

Far Manager platform for .NET modules, Windows PowerShell, and F#.

- [FarNet/wiki](https://github.com/nightroman/FarNet/wiki) - all about framework, modules, and packages.
- [FarNet/issues](https://github.com/nightroman/FarNet/issues) - for bug reports and problems
- [FarNet/discussions](https://github.com/nightroman/FarNet/discussions) - for questions and ideas

## Prerequisites

**.NET 6**

Download and install [.NET 6 SDK or runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).
SDK is needed for developing FarNet modules but recommended in any case, FarNet is tested with it.

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

This way avoids several manual steps and possible mistakes.
And it allows automated updates later.

Close Far Manager and start the PowerShell console

    powershell

Change to the Far Manager directory

    cd "C:\Program Files\Far Manager"

Invoke the preliminary command

    iex (New-Object Net.WebClient).DownloadString('https://raw.githubusercontent.com/nightroman/FarNet/master/web.ps1')

If it fails on older systems, run this first

    [Net.ServicePointManager]::SecurityProtocol = "Tls11,Tls12,$([Net.ServicePointManager]::SecurityProtocol)"

Install FarNet

    Install-FarPackage FarNet

Install modules, for example

    Install-FarPackage FarNet.PowerShellFar

You may start Far Manager after this.

Take a look at the installed files, there may be manuals, samples, and etc. The
folder FarNet contains `About-FarNet.htm` и `FarNetApi.chm`. Modules normally have
their files in folders like `FarNet\Modules\ModuleName`.


## Update using PowerShell

FarNet and modules installed by `Install-FarPackage` may be updated in the same way.

Close Far Manager and start the PowerShell console

    powershell

Change to the Far Manager directory

    cd "C:\Program Files\Far Manager"

Invoke the preliminary command

    iex (New-Object Net.WebClient).DownloadString('https://raw.githubusercontent.com/nightroman/FarNet/master/web.ps1')

To update all packages, invoke

    Update-FarPackage

To update one package, use `Install-FarPackage`, for example

    Install-FarPackage FarNet.PowerShellFar

To remove one package, use `Uninstall-FarPackage`, for example

    Uninstall-FarPackage FarNet.PowerShellFar


## Install FarNet manually

Download the FarNet package as

    https://nuget.org/api/v2/package/FarNet

The file is called `FarNet.<version>.nupkg`. This is a zip archive, you can
save it with the zip extension for easier unpacking.

All needed files are in the folder "tools". This folder contains "FarHome",
"FarHome.x64" and "FarHome.x86" folders.

Copy items of "FarHome" to Far Manager preserving the folder structure.
Depending on Far Manager, x64 or x86, copy items of "FarHome.x64" or
"FarHome.x86" to Far Manager as well.


## Install modules manually

Steps may depend on a module. But the common rule for any module `Bar` is: in
`Far Manager\FarNet\Modules` there is a folder `Bar` which contains the module
files. One of these files is the assembly `Bar.dll`.

Download the NuGet package `Bar` as

    https://nuget.org/api/v2/package/Bar

The file is called `Bar.<version>.nupkg`. This is a zip archive, you can save
it with the zip extension for easier unpacking. All needed files are in the
folder "tools". In order to install the module, copy items of the folder
"FarHome" to Far Manager preserving the folder structure. If there are
"FarHome.x64" and "FarHome.x86" folders, copy required items as well.
