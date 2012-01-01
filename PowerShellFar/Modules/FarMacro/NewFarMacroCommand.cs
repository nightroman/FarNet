
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System.Management.Automation;
using FarNet;

namespace FarMacro
{
	[Cmdlet(VerbsCommon.New, BaseCmdlet.Noun)]
	public sealed class NewFarMacroCommand : BaseFarMacroCmdlet
	{
		protected override void BeginProcessing()
		{
			Macro macro = CreateMacro();
			WriteObject(macro);
		}
	}
}
