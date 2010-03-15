
Plugin   : FarNet
Version  : 4.3.11
Release  : 2010.03.15
Category : Development
Author   : Roman Kuzmin
E-mail   : nightroman@gmail.com
Source   : http://code.google.com/p/farnet/


	= DESCRIPTION =


.NET API and infrastructure for .NET modules in Far Manager. FarNet exposes the
Far Manager API in comfortable object oriented way. PowerShell scripting is
provided by the PowerShellFar module.


	= PREREQUISITES =


 - .NET Framework 2.0
 - Far Manager 2.0.1435
 - Microsoft Visual C++ 2008 SP1 Redistributable Package (*)

 (*) FarNet is built by Visual Studio 2008 SP1 and depends on VS runtime
 modules. You may have to google, download and install one of the packages:
 - Microsoft Visual C++ 2008 SP1 Redistributable Package (x86)
 - Microsoft Visual C++ 2008 SP1 Redistributable Package (x64)


	= INSTALLATION =


Copy to %FARHOME% keeping the same directory structure:
-- Far.exe.config
-- FarNet\FarNet.*
-- Plugins\FarNet\FarNetMan.* (*)
-- FarNet\Modules\* (sample modules; you may copy and build some)

(*) x64 installation:
-- Plugins\FarNet\FarNetMan.dll must be copied from Plugins.x64\FarNet

This is the default and recommended installation. Still, you can change
location of FarNet and Modules by changing the Far.exe.config configuration.

IMPORTANT: Far.exe.config cannot be moved or renamed. If this file is missed in
%FARHOME% or configured incorrectly then Far Manager fails to load the FarNet
plugin and shows only basic failure message with no details.


	= STRUCTURE =


.\
Far.exe.config - the configuration file

.\Plugins\FarNet\
FarNetMan.dll - Far Manager plugin, manager of .NET modules
FarNetMan.hlf - FarNet UI help

.\FarNet\
FarNet.dll - FarNet interfaces for .NET modules
FarNet.xml - FarNet API XML documentation
FarNet.*.dll - FarNet internal tools

.\FarNet\Modules\
Each module folder contains exactly one .cfg file (module manifest) or exactly
one .dll file (module assembly). The manifest file tells what should be loaded:
the first line is the .dll path and other optional lines are class names: all
or none should be specified.


	= LOADING MODULES FROM DISK =


*) For each folder in Modules: if a manifest *.cfg exists then only specified
assembly and classes are loaded, else all public not abstract BaseModuleEntry
descendant classes are loaded from a single *.dll file.

*) Not loaded folders: folders with names starting with '-' (-MyModule) and
folders with no *.dll and *.cfg files are ignored.

*) Assembly names must be unique among all modules or there can be problems.
Assembly names define kind of namespaces for information stored in the
registry. Directory names and assembly locations are not important.


	= LOADING MODULES FROM CACHE =


FarNet modules registry cache:
HKCU\Software\Far2\Plugins\FarNet\!Cache

If possible, FarNet caches information about assemblies and loads them only
when they are really invoked. In many cases when you change, rename or move
assemblies or classes FarNet updates the cache itself. But some changes are
too difficult to discover (e.g. changes in config files). In these cases you
have to remove the registry cache manually (ditto for other cache problems).


	= CONFIGURATION =


It is recommended to use default settings. The following environment variables
should be used only for advanced scenarious:

	set FarNet:FarManager:Modules=path
	-- Root of Far Manager FarNet modules. Default: %FARHOME%\FarNet\Modules

	set FarNet:FarManager:DisableGui=true|false
	--  Tells to disable special rare GUI features. Default: false


	= API DOCUMENTATION (.CHM) =


Download the latest version from: http://code.google.com/p/farnet/
It includes PowerShellFar API documentation.


	= API DOCUMENTATION (.XML) =


Included XML documentation is used by other development tools like Visual
Studio Object Browser, .NET Reflector and etc.


	= MODULES HELP =


You can add help for your modules. It works in dialogs, menus, input and message
boxes (see property HelpTopic) or by IFar.ShowHelp(). Unfortunately help can not
be automatically shown by F1 ShiftF2 because technically FarNet modules are not
plugins for Far Manager.


	= BUILDING SOURCES =


You can use msbuild.exe to build the sources:

	msbuild Build.proj

and then to install all the files to "C:\Program Files\Far":

	msbuild Build.proj /t:Install

or even both operations:

	msbuild Build.proj /t:Build;Install


	= PROBLEMS AND SOLUTIONS =


PROBLEM
x86 Far on x64 machines: in rare cases not trivial .NET modules cannot load
because x86 Far disables WOW64 redirection (normally needed for loading).

SOLUTION
Theoretically the best way to avoid this problem is to use x64 Far and FarNet
on x64 machines. Unfortunately this is not always possible in practice for a
few reasons. Then the following batch file can be used to start x86 Far:

	set PATH=%WINDIR%\syswow64;%PATH%
	"C:\Program Files\Far\Far.exe"

PROBLEM
After installation Far cannot load FarNet or FarNet cannot load its modules.

SOLUTION
Read installation steps in Readme.txt (FarNet and modules) carefully and ensure
that you do everything correctly. Often mistake: Far.exe.config is not copied
to the Far home directory.

PROBLEM
After updating of a FarNet module this module cannot start or works funny.

SOLUTION
Try again after removing of the FarNet module cache in the registry:
HKCU\Software\Far2\Plugins\FarNet\!Cache
