
Plugin   : PowerShellFar
Category : Development
Version  : 2.2.10
Release  : 2009.11.02
Author   : Roman Kuzmin
Email    : nightroman@gmail.com
Sources  : C#, PowerShell
HomePage : http://code.google.com/p/farnet/


	= DESCRIPTION =


Implements Windows PowerShell host in Far Manager and user interface for
invoking commands and scripts. PowerShell code can access and control Far
functionality with FarNet object model. The package includes ready to use
scripts for various popular Far Manager tasks and PowerShell.hrc for
Colorer-take5.beta5 providing rich PowerShell syntax highlighting.

The latest versions of PowerShellFar, FarNet and documentation can be
downloaded from: http://code.google.com/p/farnet/


	= PREREQUISITES =


 - Far Manager 2.0.1192
 - Plugin FarNet 4.2.10 (*)
 - Windows PowerShell 2.0

 (*) see also FarNet prerequisites


	= INSTALLATION =


PowerShell:
Check\change PowerShell execution policy: run standard PowerShell.exe and type
Get-ExecutionPolicy - it should be RemoteSigned or Unrestricted. If it is not
then run Set-ExecutionPolicy with RemoteSigned or Unrestricted parameter.

Plugin:
Copy all files from "Plugin" to "Plugins.NET\PowerShellFar\" where "Plugins.NET"
is a folder for FarNet plugins. If FarNet is installed as recommended by
default then it is "%FARHOME%\Plugins.NET".

Bench:
You can put scripts anywhere: all together or not, but it is highly recommended
that all the used scripts are in the system %PATH% folders. For example, if you
use Bench scripts, include Bench directory into the system path.

PowerShell.hrc (for Colorer):
PowerShell syntax for Colorer. See installation instructions in the file.

RomanConsole.hrd (for Colorer):
Console palette with white background. PowerShell.hrc is actually designed with
this palette in use, other palettes can be less suitable or even practically
unusable. See installation instructions in the file.
