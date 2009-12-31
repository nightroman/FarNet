
Plugin   : FarNet
Version  : 4.2.20
Release  : 2009.12.31
Category : Development
Author   : Roman Kuzmin
E-mail   : nightroman@gmail.com
Source   : http://code.google.com/p/farnet/


	= DESCRIPTION =


Plugin for writing Far Manager plugins in .NET languages and even PowerShell
scripting with PowerShellFar plugin. It exposes FarNet object model covering
and extending Far Manager native API in comfortable object oriented way.
Home page: http://code.google.com/p/farnet/


	= PREREQUISITES =


 - .NET Framework 2.0
 - Far Manager 2.0.1304
 - Microsoft Visual C++ 2008 SP1 Redistributable Package (*)

 (*) FarNet is built by Visual Studio 2008 SP1 and depends on VS runtime
 modules. You may have to google, download and install one of the packages:
 - Microsoft Visual C++ 2008 SP1 Redistributable Package (x86)
 - Microsoft Visual C++ 2008 SP1 Redistributable Package (x64)


	= INSTALLATION =


Copy to %FARHOME%:
- Far.exe.config
- Lib
- Plugins\FarNet (*)
- Plugins.NET (.NET plugins; you may copy and build optional samples)

(*) x64 installation:
- Plugins\FarNet\FarNetMan.dll should be copied from Plugins.x64\FarNet

This is the default installation (recommended). You can move Lib or\and
Plugins.NET, in this case you have to update Far.exe.config accordingly.

IMPORTANT: Far.exe.config cannot be moved or renamed. If this file is missed in
%FARHOME% or configured incorrectly then Far Manager fails to load FarNet and
shows only basic failure message with no details.


	= STRUCTURE =


.\
Far.exe.config - configuration file

.\Lib\
FarNet.dll - FarNet interfaces
FarNet.xml - XML documentation

.\Plugins\FarNet\
FarNetMan.dll - Far plugin, manager of FarNet plugins
FarNetMan.hlf - FarNet help

.\Plugins.NET\
Each plugin folder contains one or more assemblies (.dll) and at most one
optional configuration file (.cfg). Each line of .cfg file is:
	<Assembly> <Class1> <Class2> ... <ClassN>
	where
	<Assembly> - relative path of a plugin assembly;
	<ClassX> - name of a plugin class to be loaded and connected.


	= LOADING PLUGINS FROM DISK =


*) For each folder in Plugins.NET: if a file *.cfg exists then only specified
assemblies and their classes are loaded, else all non abstract BasePlugin
children are loaded from all DLLs in the plugin folder.
*) Excluded folders: folders "-*", e.g. "-MyPlugin", are not loaded.
*) Plugin assembly names must be unique among all plugins otherwise there can be
both .NET and FarNet problems. An assembly name defines kind of namespace for
information stored in the registry. Directory names and assembly locations are
not important.


	= LOADING PLUGINS FROM CACHE =


FarNet plugin info registry cache:
HKCU\Software\Far2\Plugins\FarNet\<cache>

If possible, FarNet caches information about assemblies and loads them only
when they are really invoked. In many cases when you change, rename or move
assemblies or classes FarNet updates the cache itself. But some changes are
too difficult to discover (e.g. changes in config files), in these cases you
have to remove the registry cache manually (ditto for other cache problems).


	= API DOCUMENTATION (.CHM) =


Download the latest version from: http://code.google.com/p/farnet/
It includes PowerShellFar API documentation.


	= API DOCUMENTATION (.XML) =


Included XML documentation is used by other development tools like Visual
Studio Object Browser, .NET Reflector and etc.


	= PLUGINS HELP =


You can add help for your plugins. It works in dialogs, menus, input and message
boxes (see property HelpTopic) or by IFar.ShowHelp(). Unfortunately help can not
be automatically shown by F1 ShiftF2 because technically FarNet plugins are not
visible to Far Manager.


	= BUILDING SOURCES =


You can use msbuild.exe to build the sources:

	msbuild Build-FarNetDev.proj

and then to install all the files to "C:\Program Files\Far":

	msbuild Build-FarNetDev.proj /t:Install

or even both operations:

	msbuild Build-FarNetDev.proj /t:Build;Install


	= PROBLEMS AND SOLUTIONS =


PROBLEM
x86 Far on x64 machines: in rare cases not trivial .NET plugins cannot load
because x86 Far disables WOW64 redirection (normally needed for loading).
SOLUTION
Theoretically the best way to avoid this problem is to use x64 Far and FarNet
on x64 machines. Unfortunately it is not always possible in practice: plugins
may not have x64 versions or x64 Far may have not yet resolved problems. Then
the following batch file can be used to start x86 Far:

	set PATH=%WINDIR%\syswow64;%PATH%
	"c:\program files\Far\Far.exe"

PROBLEM
After installation Far cannot load FarNet or FarNet cannot load .NET plugins.
SOLUTION
Read installation steps in Readme.txt (FarNet and plugins) carefully and ensure
that you do everything correctly. Often mistake: Far.exe.config is not copied
to the Far home directory.
