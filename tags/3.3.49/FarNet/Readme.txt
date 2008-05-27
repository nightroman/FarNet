Plugin   : FAR.NET
Category : Development
Version  : 3.3.49
Release  : 2008.05.18
Author   : Roman Kuzmin
Email    : nightroman@hotmail.com
Sources  : C#, C++/CLI (see "Build sources")
HomePage : http://code.google.com/p/farnet/


	Requirements


- .NET Framework 2.0
- FAR Manager 1.71.2358.


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


Plugins\FAR.NET\
- FarNetPlugin.dll - FAR plugin and loader of FAR.NET plugins;
- FarNetPlugin.hlf - FAR.NET help;

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


	Loading plugins from disk


*) For each folder in Plugins.NET: if a file *.cfg exists then only specified
assemblies and their classes are loaded, else all non abstract BasePlugin
children are loaded from all DLLs in the plugin folder.
*) Excluded folders: folders "-*", e.g. "-MyPlugin", are not loaded.
*) Plugin assembly names must be unique among all plugins otherwise there can be
both .NET and FAR.NET problems. An assembly name defines kind of namespace for
information stored in the registry. Directory names and assembly locations are
not important.


	Loading plugins from cache


FAR.NET plugin info registry cache:
HKCU\Software\Far\Plugins\FAR.NET\<cache>

If possible, FAR.NET caches information about assemblies and loads them only
when they are really invoked. In many cases when you change, rename or move
assemblies or classes FAR.NET updates the cache itself. But some changes are
too difficult for that (e.g. changes in config files), in these cases you have
to remove the registry cache manually (ditto for other cache problems).


	CHM documentation


Download the latest version from:
http://code.google.com/p/farnet/


	XML documentation


Included XML documentation is not perhaps a perfect form of documentation but it is
always up-to-date and practically very useful for development.

Visual Studio Object Browser automatically uses XML comments well enough.
Another good tool is Reflector for .NET (it is free). It displays documentation
in MSDN-like style, provides powerful navigation and search (including .NET and
any loaded .NET assemblies): http://www.aisto.com/roeder/dotnet/


	Plugins help


You can add help for your plugins. It works in dialogs, menus, input and message
boxes (see property HelpTopic) or by IFar.ShowHelp(). Unfortunately help can not
be automatically shown by F1 ShiftF2 because technically FAR.NET plugins are not
visible to FAR.


	Build sources


- If you build Debug remove the line "#include <Test1.h>" from "stdafx.h".
- You can use msbuild.exe to build the sources:

	msbuild Build-FarNet.proj

and then to install all the files to "C:\Program Files\Far":

	msbuild Build-FarNet.proj /t:Install

or even both operations:

	msbuild Build-FarNet.proj /t:Build;Install
