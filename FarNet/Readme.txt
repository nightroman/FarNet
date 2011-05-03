
Plugin   : FarNet
Version  : 4.4.11
Release  : 2011-05-02
Category : Development
Author   : Roman Kuzmin
E-mail   : nightroman@gmail.com
Source   : http://code.google.com/p/farnet/


= DESCRIPTION =


FarNet provides the .NET API for Far Manager and the runtime infrastructure for
.NET modules. The API is exposed in comfortable object oriented way and most of
tedious programming job is done internally. User modules normally contain only
tiny pieces of boilerplate framework code.


= PREREQUISITES =


 - .NET Framework 2.0+
 - Far Manager 2.0.1807
 - Microsoft Visual C++ 2008 SP1 Redistributable Package (*)

 (*) FarNet is built by Visual Studio 2008 SP1 and depends on VS runtime
 modules. You may have to google, download and install one of the packages:
 - Microsoft Visual C++ 2008 SP1 Redistributable Package (x86)
 - Microsoft Visual C++ 2008 SP1 Redistributable Package (x64)


= INSTALLATION =


The file Install.txt shows what has to be copied to %FARHOME% keeping the same
directory structure (see (*) about x64).

(*) x64 installation:
-- Plugins\FarNet\FarNetMan.dll has to be copied from Plugins.x64\FarNet

Optional:
-- FarNet\Modules\* (sample modules; you may copy and build some)

Far.exe.config allows to change the proposed locations of FarNet and Modules
but this scenario is not tested on development.

IMPORTANT: Far.exe.config cannot be moved or renamed. If this file is missed in
%FARHOME% or configured incorrectly then Far Manager fails to load the FarNet
plugin and shows only basic failure message with no details.


= FILE STRUCTURE =


.\
Far.exe.config - the configuration file

.\Plugins\FarNet\
FarNetMan.dll - Far Manager plugin and FarNet module manager
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
assembly and classes are loaded, else all public not abstract BaseModuleItem
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


= MODULE COMMANDS IN MACROS =


If a FarNet module provides commands invoked by prefixes then these commands
can be called from macros by CallPlugin(). The first argument is the FarNet
system ID: 0xcd. The second argument is the module prefix, colon and command.
For asynchronous steps and jobs the argument should start with one and two
colons respectively.

Create and use in macros the macro constant FarNet:

	use the PowerShellFar command:
		>: $Far.Macro.InstallConstant('FarNet', 0xcd)

	or use the .reg file:
		Windows Registry Editor Version 5.00
		[HKEY_CURRENT_USER\Software\Far2\KeyMacros\Consts]
		"FarNet"=dword:000000cd

SYNTAX AND DETAILS

	Synchronous command:

		CallPlugin(FarNet, "Prefix:Command")
			- It is called from all macro areas
			- It can but should not use modal UI
			- It cannot open panels

	Asynchronous step (IFar.PostStep)

		CallPlugin(FarNet, ":Prefix:Command")
			- It is called from areas where the plugin menu is available
			- FarNet itself must have a hotkey in the plugin menu
			- It can use modal UI as usual
			- It can open panels

	Asynchronous job (IFar.PostJob)

		CallPlugin(FarNet, "::Prefix:Command")
			- It is called from all macro areas
			- It can use modal UI as usual
			- It cannot open panels

	- Synchronous calls are for simple actions with no or tiny interaction;
	- An asynchronous CallPlugin normally should be the last macro command;
	- Asynchronous jobs are good for starting modal dialogs, editors, etc.;
	- Asynchronous steps are used to open any UI including module panels.

CAUTION

It is recommended to use asynchronous steps, not jobs. Steps are basically the
same as calls from the plugin menu, this is the most stable approach. Jobs may
reveal scenarios not quite expected by Far Manager. The CallPlugin feature is
powerful but still experimental, it can be even replaced in the future.

EXAMPLES

	Synchronous (RightControl and PowerShellFar commands):

		CallPlugin(FarNet, "RightControl:step-left")
		CallPlugin(FarNet, ">: Menu-Favorites-.ps1")

	Asynchronous step (PowerShellFar command opens the panel):

		CallPlugin(FarNet, ":>: Get-Process | Out-FarPanel")

	Asynchronous job (PowerShellFar command opens the dialog):

		CallPlugin(FarNet, "::>: $Psf.InvokeInputCode()")

KNOWN ISSUE AND WORKAROUND

An asynchronous step opens the plugin menu internally. This menu is actually
shown for a moment if a macro has its option "Disable screen output" checked.
The workaround is very funny: uncheck this macro option, i.e. allow screen
output. The effect is the opposite: the unwanted menu is not shown anymore.


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
x86 Far on x64 machines: in rare cases not trivial .NET modules cannot be
loaded because x86 Far disables WOW64 redirection.

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
