# FarNet.Demo module

## Main features

This modules demonstrates some FarNet module features:

- Module host [DemoHost.cs](DemoHost.cs) called on every module load.
- Module command [DemoCommand.cs](DemoCommand.cs) for commands with prefixes.
- Module tool [DemoTool.cs](DemoTool.cs) for menu items in the Far plugin menu.
- Explorer [DemoExplorer.cs] for module panels.
- Localisation using .restext files.
- Settings using XML files.
- Module help files.

## Module settings

Module may implement settings by deriving from and implementing `ModuleSettings` class.

Settings may be roaming (default) or local and browsable (default) or non-browsable, in any combination.
Here are the two most typical examples:

- Browsable roaming settings [Settings.cs](Settings.cs)
- Non-browsable local settings [Workings.cs](Workings.cs)

Roaming settings are stored in `%FARPROFILE%\FarNet\<Module>`, local in `%FARLOCALPROFILE%\FarNet\<Module>`.

Browsable settings are shown in the `[F11]` \ `FarNet` \ `Settings` menu, non-browsable are excluded.
