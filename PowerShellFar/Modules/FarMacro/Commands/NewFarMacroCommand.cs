/*
PowerShellFar plugin for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.ComponentModel;
using System.Management.Automation;
using FarNet;

namespace FarMacro
{
	/// <summary>
	/// New-FarMacro command.
	/// Creates a new macro instance.
	/// </summary>
	[Cmdlet(VerbsCommon.New, BaseCmdlet.Noun)]
	[Description("Creates a new macro instance.")]
	public sealed class NewFarMacroCommand : BaseFarMacroCmdlet
	{
		///
		protected override void BeginProcessing()
		{
			Macro macro = CreateMacro();
			WriteObject(macro);
		}
	}
}
