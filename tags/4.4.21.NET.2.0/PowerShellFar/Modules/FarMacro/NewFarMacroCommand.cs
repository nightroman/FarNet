
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2011 Roman Kuzmin
*/

using System.ComponentModel;
using System.Management.Automation;
using FarNet;

namespace FarMacro
{
	[Cmdlet(VerbsCommon.New, BaseCmdlet.Noun)]
	[Description("Creates a new macro. Use Set-FarMacro or $Far.Macro.Install() to save it.")]
	public sealed class NewFarMacroCommand : BaseFarMacroCmdlet
	{
		protected override void BeginProcessing()
		{
			Macro macro = CreateMacro();
			WriteObject(macro);
		}
	}
}
