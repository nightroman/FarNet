
Module   : FarNet.PowerShellFar
Version  : 4.3.7
Release  : 2010.03.01
Category : Scripting
Author   : Roman Kuzmin
E-mail   : nightroman@gmail.com
Source   : http://code.google.com/p/farnet/


	= DESCRIPTION =


Implements Windows PowerShell host in Far Manager, user interface for invoking
commands and scripts, and exposes underlying FarNet object model. The package
includes scripts for many popular Far Manager tasks and PowerShell.hrc for
Colorer providing rich PowerShell syntax highlighting.

PowerShellFar, FarNet and documentation can be updated by the included script
Update-PowerShellFar.ps1 or downloaded manually from:
http://code.google.com/p/farnet/


	= PREREQUISITES =


 - Far Manager 2.0.1428
 - Plugin FarNet 4.3.7
 - Windows PowerShell 2.0


	= INSTALLATION =


*) Configure PowerShell:
Check\change PowerShell execution policy: run standard PowerShell.exe and type
Get-ExecutionPolicy - it should be RemoteSigned or Unrestricted. If it is not
then run Set-ExecutionPolicy with RemoteSigned or Unrestricted parameter.

*) Install the module:
Copy FarNet\Modules\PowerShellFar to %FARHOME%\FarNet\Modules\PowerShellFar.
The location can be changed but it is recommended to use exactly this way:
at least you can use Update-PowerShellFar.ps1 for updates.

*) Recommended: Install and configure Bench scripts:
You can put scripts anywhere: all together or not, but it is highly recommended
that all the used scripts are in the system %PATH% folders. For example, if you
use Bench scripts then include Bench path into the system path.

*) Recommended: Install PowerShell.hrc for Colorer:
PowerShell syntax for Colorer. See installation instructions in the file.

*) Optionally: Install RomanConsole.hrd for Colorer:
Console palette with white background. PowerShell.hrc is actually designed with
this palette in use, other palettes may be less suitable or even practically
unusable. See installation instructions in the file.


	= PROBLEMS AND SOLUTIONS =


PROBLEM
x86 Far on x64 machines: in rare cases PowerShellFar cannot load PowerShell
core because x86 Far disables WOW64 redirection.

SOLUTION
Theoretically the best way to avoid this problem is to use x64 Far and FarNet
on x64 machines. Unfortunately it is not always possible in practice: plugins
may not have x64 versions or x64 Far may have not yet resolved problems. Then
the following batch file can be used to start x86 Far:

	set PATH=%WINDIR%\syswow64;%PATH%
	"C:\Program Files\Far\Far.exe"

PROBLEM
After installation Far cannot load FarNet or FarNet cannot load PowerShellFar.

SOLUTION
Read installation steps in Readme.txt (FarNet and PowerShellFar) carefully and
ensure that you do everything correctly. Often mistake: Far.exe.config is not
copied to the Far home directory.
