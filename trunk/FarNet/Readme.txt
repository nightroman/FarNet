
Plugin   : FarNet
Version  : 5.0.14
Release  : 2012-02-19
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


 - Far Manager 3.0.2468
 - .NET Framework 3.5 or 4.0
 - Microsoft Visual C++ 2008 SP1 Redistributable Package (*)

 (*) Part of FarNet is built by Visual Studio 2008 SP1 and depends on its
 runtime modules. You may have to download and install one of the packages:
 - Microsoft Visual C++ 2008 SP1 Redistributable Package (x86)
 - Microsoft Visual C++ 2008 SP1 Redistributable Package (x64)


= INSTALLATION =


The file Install.txt shows what has to be copied to %FARHOME% keeping the same
directory structure (see (*) about x64).

(*) x64 installation:
-- Plugins\FarNet\FarNetMan.dll has to be copied from Plugins.x64\FarNet

Optional:
-- FarNet\Modules\* (sample modules, just sources)

Far.exe.config can change the default FarNet directory but this scenario is not
recommended and not tested.

IMPORTANT: Far.exe.config cannot be moved or renamed. If this file is missing
in %FARHOME% or configured incorrectly then Far Manager fails to load the
FarNet plugin and shows only basic failure message with no details.


= FILE STRUCTURE =


%FARHOME%
Far.exe.config - the configuration file

%FARHOME%\Plugins\FarNet
FarNetMan.dll - Far Manager plugin and module manager
FarNetMan.hlf - FarNet UI help

%FARHOME%\FarNet
FarNet.dll, FarNet.xml - FarNet API for .NET modules (and XML API comments).
FarNet.Tools.dll, FarNet.Tools.xml - FarNet module tools library.
FarNet.Settings.dll, FarNet.Settings.xml - FarNet module settings library.
FarNet.*.dll - other FarNet libraries used internally by the core.

%FARHOME%\FarNet\Modules
Module root directory. Each child directory is normally a module directory.


= LOADING MODULES FROM DISK =


*) For each directory "ModuleName" in Modules the core looks for and loads the
module assembly "ModuleName\ModuleName.dll". When the assembly is loaded the
core looks for the special module classes in it. The module directory may
contain other files including other assemblies, the core ignores them.

*) Unique module names (module directory and base assembly names) define
namespaces for various module information. These names should be chosen
carefully and should not change.


= LOADING MODULES FROM CACHE =


If a module is not preloadable then on its first loading the core caches its
meta data like menu item titles, command prefixes, supported file masks, and
etc. Next time the module data are read from the cache and the module is not
loaded when the core starts. It is loaded only when it starts to work.

Normally the core discovers module changes and updates the cache itself. In
rare cases the cache may have to be removed manually and the core restarted.
Note that data of removed modules are removed from the cache automatically.

The module cache file by default:
%LOCALAPPDATA%\Far Manager\Profile\FarNet\Cache.binary


= CONFIGURATION =


It is recommended to use the default settings. The following environment
variables should be used only in special cases:

	set FarNet:FarManager:Modules=<path>
	-- Root module directory. Default: %FARHOME%\FarNet\Modules

	set FarNet:FarManager:DisableGui=<true|false>
	--  Tells to disable special GUI features. Default: false


= MODULE COMMANDS IN MACROS =


If a FarNet module provides commands invoked by prefixes then these commands
can be called from macros by CallPlugin(). The first argument is FarNet GUID.
The second argument is the module prefix, colon, and command. For asynchronous
steps and jobs the argument should start with one and two colons respectively.
FarNet GUID is "10435532-9BB3-487B-A045-B0E6ECAAB6BC".

SYNTAX AND DETAILS

	Synchronous command:

		CallPlugin("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "Prefix:Command")
			- It is called from all macro areas
			- It can but should not use modal UI
			- It cannot open panels

	Asynchronous step (IFar.PostStep)

		CallPlugin("10435532-9BB3-487B-A045-B0E6ECAAB6BC", ":Prefix:Command")
			- It is called from areas with the plugin menu
			- It can use modal UI as usual
			- It can open panels

	Asynchronous job (IFar.PostJob)

		CallPlugin("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "::Prefix:Command")
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

		CallPlugin("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "RightControl:step-left")
		CallPlugin("10435532-9BB3-487B-A045-B0E6ECAAB6BC", ">: Menu-Favorites-.ps1")

	Asynchronous step (PowerShellFar command opens the panel):

		CallPlugin("10435532-9BB3-487B-A045-B0E6ECAAB6BC", ":>: Get-Process | Out-FarPanel")

	Asynchronous job (PowerShellFar command opens the dialog):

		CallPlugin("10435532-9BB3-487B-A045-B0E6ECAAB6BC", "::>: $Psf.InvokeInputCode()")

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

---

PROBLEM
After installation Far cannot load FarNet or FarNet cannot load its modules.

SOLUTION
Read installation steps in Readme.txt (FarNet and modules) carefully and ensure
that you do everything correctly. Often mistake: Far.exe.config is not copied
to the Far home directory.

---

PROBLEM
After updating of a FarNet module this module cannot start or works funny.

SOLUTION
Try again after removing the FarNet module cache:
%LOCALAPPDATA%\Far Manager\Profile\FarNet\cache.binary
