
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2011 Roman Kuzmin
*/

using System.Collections.Generic;
using System.Management.Automation;
using FarNet;

namespace FarMacro
{
	[Cmdlet(VerbsCommon.Set, BaseCmdlet.Noun)]
	public sealed class SetFarMacroCommand : BaseFarMacroCmdlet
	{
		[Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Macros")]
		public Macro InputObject { get; set; }
		protected override void ProcessRecord()
		{
			Macro macro = InputObject == null ? CreateMacro() : InputObject;
			Far.Net.Macro.Install(macro);
		}
	}
}