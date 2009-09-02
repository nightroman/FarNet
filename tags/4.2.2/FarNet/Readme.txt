Plugin   : FarNet
Category : Development
Version  : 4.2.0
Release  : 2009.09.02
Author   : Roman Kuzmin
Email    : nightroman@gmail.com
Sources  : C#, C++/CLI
HomePage : http://code.google.com/p/farnet/


	DESCRIPTION


Plugin for writing Far Manager plugins in any .NET language (even in PowerShell
with PowerShellFar plugin). It exposes significant part of FAR API and also
extends it for comfortable use in object oriented way.
Home page: http://code.google.com/p/farnet/


	PREREQUISITES


 - .NET Framework 2.0
 - Far Manager 2.0.1100
 - Microsoft Visual C++ 2008 SP1 Redistributable Package (*)

 (*) FarNet is built by Visual Studio 2008 SP1 and depends on VS runtime
 modules. You may have to google, download and install one of the packages:
 - Microsoft Visual C++ 2008 SP1 Redistributable Package (x86)
 - Microsoft Visual C++ 2008 SP1 Redistributable Package (x64)


	INSTALLATION

Copy to %FARHOME%:
- Far.exe.config
- Lib
- Plugins\FarNet (*)
- Plugins.NET (for .NET plugins, contains optional examples)

(*) x64 installation:
- the file FarNetMan.dll should be copied from Plugins.x64\FarNet

This is the default installation (recommended). You can move Lib or\and
Plugins.NET, in this case you have to update Far.exe.config accordingly.


	STRUCTURE


Plugins\FarNet\
- FarNetMan.dll - FAR plugin, manager of FarNet .NET plugins
- FarNetMan.hlf - FarNet help

Lib\
- FarNet.dll - FarNet interfaces
- FarNet.xml - XML documentation

Plugins.NET\
Each plugin folder contains one or more assemblies (.dll) and at most one
optional configuration file (.cfg). Each line of .cfg file is:
	<Assembly> <Class1> <Class2> ... <ClassN>
	where
	<Assembly> - relative path of a plugin assembly;
	<ClassX> - name of a class from this assembly.


	LOADING PLUGINS FROM DISK


*) For each folder in Plugins.NET: if a file *.cfg exists then only specified
assemblies and their classes are loaded, else all non abstract BasePlugin
children are loaded from all DLLs in the plugin folder.
*) Excluded folders: folders "-*", e.g. "-MyPlugin", are not loaded.
*) Plugin assembly names must be unique among all plugins otherwise there can be
both .NET and FarNet problems. An assembly name defines kind of namespace for
information stored in the registry. Directory names and assembly locations are
not important.


	LOADING PLUGINS FROM CACHE


FarNet plugin info registry cache:
HKCU\Software\Far2\Plugins\FarNet\<cache>

If possible, FarNet caches information about assemblies and loads them only
when they are really invoked. In many cases when you change, rename or move
assemblies or classes FarNet updates the cache itself. But some changes are
too difficult to discover (e.g. changes in config files), in these cases you
have to remove the registry cache manually (ditto for other cache problems).


	CHM DOCUMENTATION


Download the latest version from:
http://code.google.com/p/farnet/


	XML DOCUMENTATION


Included XML documentation is used by other development tools like Visual
Studio Object Browser, .NET Reflector and etc.


	PLUGINS HELP


You can add help for your plugins. It works in dialogs, menus, input and message
boxes (see property HelpTopic) or by IFar.ShowHelp(). Unfortunately help can not
be automatically shown by F1 ShiftF2 because technically FarNet plugins are not
visible to FAR.


	BUILDING SOURCES


- If you build Debug remove the line "#include <Test1.h>" from "stdafx.h".
- You can use msbuild.exe to build the sources:

	msbuild Build-FarNetDev.proj

and then to install all the files to "C:\Program Files\Far":

	msbuild Build-FarNetDev.proj /t:Install

or even both operations:

	msbuild Build-FarNetDev.proj /t:Build;Install
