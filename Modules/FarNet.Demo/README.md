# FarNet.Demo module

## Main features

This module implements and shows some features:

- Module command [DemoCommand.cs](DemoCommand.cs) for commands using the prefix.
- Module tool [DemoTool.cs](DemoTool.cs) for menu items in the plugin menu.
- Module host [DemoHost.cs](DemoHost.cs) called on every module load.
- Explorer [DemoExplorer.cs](DemoExplorer.cs) for module panels.
- Localisation, .restext files compiled to .resources.
- Settings using XML files.
- Module help files.

## Module methods

FarNet modules, like FarNet scripts, may have methods for calls by the command `fn:`.

Example: [DemoMethods.cs](DemoMethods.cs) method `Message` may be called as:

    fn: module=FarNet.Demo; method=FarNet.Demo.DemoMethods.Message :: name=John Doe; age=42

## Module settings

Module may define settings by implementing `ModuleSettings`.

Settings may be roaming (default) or local and browsable (default) or non-browsable, in any combination.
Typical examples:

- Browsable roaming settings [Settings.cs](Settings.cs)
- Non-browsable local settings [Workings.cs](Workings.cs)

Roaming settings are stored in `%FARPROFILE%\FarNet\<Module>`, local in `%FARLOCALPROFILE%\FarNet\<Module>`.

Browsable settings are shown by `F11 / FarNet / Settings`, non-browsable are excluded from the menu.

Sample PowerShellFar scripts dealing with settings:

- [Settings.far.ps1](Scripts/Settings.far.ps1)
- [Workings.far.ps1](Scripts/Workings.far.ps1)
- [MySettings.far.ps1](Scripts/MySettings.far.ps1)
