
Module   : FarNet.PowerShellFar
Version  : 5.0.0
Release  : 2012-01-08
Category : Scripting
Author   : Roman Kuzmin
E-mail   : nightroman@gmail.com
Source   : http://code.google.com/p/farnet/


= DESCRIPTION =


One of the most useful FarNet modules is PowerShellFar. It combines the rich
console based user interface of Far Manager with full power of Windows
PowerShell perfectly integrated into this original text friendly environment.

It implements the Windows PowerShell host in Far Manager, exposes the FarNet
object model, and provides several ways for invoking commands and viewing the
results. The package includes cmdlets, modules and scripts for many popular Far
Manager tasks and PowerShell.hrc for Colorer providing rich PowerShell syntax
highlighting.

PowerShellFar, FarNet and documentation can be updated by the included script
Update-FarNet.ps1 or downloaded manually from:
http://code.google.com/p/farnet/


= PREREQUISITES =


 - Far Manager 3.0.0
 - Plugin FarNet 5.0.0
 - Windows PowerShell V2, V3


= INSTALLATION =


*) Configure PowerShell:
Set execution policy: start PowerShell.exe and type Get-ExecutionPolicy. If it
is not Bypass, Unrestricted, or RemoteSigned then invoke Set-ExecutionPolicy
with one of these values. Bypass is the fastest but the least secure.

On x64 machines Set-ExecutionPolicy should be set twice: for x86 and x64. Use
the Windows PowerShell start menu in order to start both consoles and set the
policy in both.

If you are not administrator use the parameter -Scope CurrentUser, for example:
>: Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

*) Install the module:
Copy FarNet\Modules\PowerShellFar to %FARHOME%\FarNet\Modules\PowerShellFar.
The location and name can be changed but it is recommended to use this way:
some tools assume this path by default, e.g. Update-FarNet.ps1.

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
on x64 machines. Unfortunately this is not always possible in practice for a
few reasons. Then the following batch file can be used to start x86 Far:

	set PATH=%WINDIR%\syswow64;%PATH%
	"C:\Program Files\Far\Far.exe"

PROBLEM
After installation Far cannot load FarNet or FarNet cannot load PowerShellFar.

SOLUTION
Read installation steps in Readme.txt (FarNet and PowerShellFar) carefully and
ensure that you do everything correctly. Often mistake: Far.exe.config is not
copied to the Far home directory.
