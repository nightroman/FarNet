Plugin name  : Far.NET3
Category     : Development
Version      : 3.3.33
Release date : 2007.11.19
Author       : Roman Kuzmin
Email        : nightroman@hotmail.com
HomePage     : http://nightroman.spaces.live.com
Sources      : C#, C++/CLI (at home page)


	Requirements


- .NET Frameworks 2.0
- Far Manager 1.71 (recommended 1.71.2192 and above)
* The plugin is built and tested on Far Manager 1.71.2287


	Description


Plugin for writing Far Manager plugins in any .NET language (even in PowerShell
with PowerShellFar plugin). It exposes significant part of Far API and also
extends it for comfortable use in object oriented way.
Home page: http://code.google.com/p/farnet/


	Installation


Copy to %FARHOME%: file "Far.exe.config", folders "Lib", "Plugins" and
optionally "Plugins.NET" (if you use something from there). This is default and
recommended installation. But you can change location of "Lib" and "Plugins.NET"
if you want; in this case you have to update "Far.exe.config" accordingly.


	Structure


- Plugins\Far.NET\FarNetPlugin.dll - standard Far plugin;
- Plugins\Far.NET\FarNetPlugin_en.hlf - help (for manually added plugin links);
- Lib - plugin assemblies and their resources;
- Lib\FarNetIntf.dll - FarManager interfaces;
- Lib\FarNetIntf.xml - XML documentation, e.g. for VS Object Browser;
- Plugins.NET - plugins folder. Each plugin is a folder with optional subfolders
Bin (assemblies) and Cfg (configuration files):
- Cfg\Plugin.cfg - configuration file. Each line is:
	<Assembly> <Class1> <Class2> ... <ClassN>
	where
	<Assembly> - name of an assembly from Bin;
	<ClassX> - name of a class in this assembly.


	Loading Plugins


- For each folder in Plugins.NET:
	- if "Cfg\Plugin.cfg" exists then load it according to the configuration;
	- else if "Bin" exists then load all *.dll from there;
	- else load all *.dll from the plugin folder.
- For each loaded *.dll find and create instances of all non abstract classes
implementing IPlugin interface.


	CHM Documentation


Download the latest version from
http://code.google.com/p/farnet/


	XML Documentation


Included XML documentation is not perhaps a perfect form of documentation but it is
always up-to-date and practically very useful for development.

Visual Studio Object Browser automatically uses XML comments well enough.
Another possible way is to use for example Reflector for .NET (it is free). It
displays documentation in MSDN-like style, provides powerful navigation and
search (including .NET and any loaded .NET assemblies).
(Reflector for .NET: http://www.aisto.com/roeder/dotnet/)


	Plugins Help


You can add help for your plugins. It works in dialogs, menus, input and message
boxes (see property HelpTopic) or by IFar.ShowHelp(). Unfortunately help can not
be automatically shown by ShiftF2 because technically Far.NET plugins are not
visible for FAR, it sees only Far.NET. But you can add a link to your plugin
help file to "FarNetPlugin_en.hlf" manually as it is done for PowerShellFar.
