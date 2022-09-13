
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System.Collections.Generic;
using System.Management.Automation;

namespace PowerShellFar.Commands;

[OutputType(typeof(string))]
sealed class GetFarPathCommand : BaseFileCmdlet
{
	[Parameter(HelpMessage = "Join requested item names with the opposite panel path.")]
	public SwitchParameter Mirror { get; set; }

	protected override void BeginProcessing()
	{
		IPanel panel1 = Passive ? Far.Api.Panel2 : Far.Api.Panel;
		IPanel panel2 = Mirror ? (Passive ? Far.Api.Panel : Far.Api.Panel2) : panel1;

		// no panel?
		if (panel1 is null || panel2 is null)
			return;

		// get path(s)
		IEnumerator<string> it;
		if (All)
		{
			it = new PathEnumerator(panel1.GetFiles(), panel2.CurrentDirectory, panel1.RealNames, panel1 != panel2);
		}
		else if (Selected)
		{
			it = new PathEnumerator(panel1.GetSelectedFiles(), panel2.CurrentDirectory, panel1.RealNames, panel1 != panel2);
		}
		else
		{
			WriteObject(GetCurrentPath(panel1, panel2));
			return;
		}
		using (it)
		{
			while (it.MoveNext())
				WriteObject(it.Current);
		}
	}
}
