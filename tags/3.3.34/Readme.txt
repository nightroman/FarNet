Plugin name  : FAR.NET
Category     : Development
Version      : 3.3.34
Release date : 2007.11.26
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


Copy to %FARHOME%:
- Far.exe.config
- Lib
- Plugins\FAR.NET
- Plugins.NET (with optional examples)

This is the default installation. You can move Lib or Plugins.NET; in this case
you have to update Far.exe.config accordingly.


	Structure


Plugins\Far.NET\
- FarNetPlugin.dll - FAR plugin and loader of FAR.NET plugins;
- FarNetPlugin_en.hlf - basic FAR.NET help;

Lib\
- FarNetIntf.dll - FarManager interfaces;
- FarNetIntf.xml - XML documentation;

Plugins.NET\
Each plugin folder contains one or more assemblies (.dll) and at most one
optional configuration file (.cfg). Each line of .cfg file is:
	<Assembly> <Class1> <Class2> ... <ClassN>
	where
	<Assembly> - relative path of a plugin assembly;
	<ClassX> - name of a class from this assembly.


	Loading Plugins


For each folder in Plugins.NET: if a file *.cfg exists then only specified
assemblies and their classes are loaded, else all non abstract BasePlugin
children are loaded from all DLLs in the plugin folder.


	CHM Documentation


Download the latest version from
http://code.google.com/p/farnet/


	XML Documentation


Included XML documentation is not perhaps a perfect form of documentation but it is
always up-to-date and practically very useful for development.

Visual Studio Object Browser automatically uses XML comments well enough.
Another good tool is Reflector for .NET (it is free). It displays documentation
in MSDN-like style, provides powerful navigation and search (including .NET and
any loaded .NET assemblies): http://www.aisto.com/roeder/dotnet/


	Plugins Help


You can add help for your plugins. It works in dialogs, menus, input and message
boxes (see property HelpTopic) or by IFar.ShowHelp(). Unfortunately help can not
be automatically shown by F1 ShiftF2 because technically FAR.NET plugins are not
visible to FAR.
