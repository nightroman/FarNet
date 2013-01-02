
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

using System.Collections.Generic;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	sealed class GetFarPathCommand : BaseFileCmdlet
	{
		[Parameter(HelpMessage = "Join requested item names with the opposite panel path.")]
		public SwitchParameter Mirror { get; set; }
		protected override void BeginProcessing()
		{
			IPanel panel1 = Passive ? Far.Api.Panel2 : Far.Api.Panel;
			IPanel panel2 = Mirror ? (Passive ? Far.Api.Panel : Far.Api.Panel2) : panel1;

			// no panel?
			if (panel1 == null || panel2 == null)
				return;

			// get path(s)
			IEnumerator<string> it;
			if (All)
			{
				it = new PathEnumerator(panel1.ShownFiles, panel2.CurrentDirectory, panel1.RealNames, panel1 != panel2);
			}
			else if (Selected)
			{
				it = new PathEnumerator(panel1.SelectedFiles, panel2.CurrentDirectory, panel1.RealNames, panel1 != panel2);
			}
			else
			{
				WriteObject(GetCurrentPath(panel1, panel2));
				return;
			}
			using (it)
				while (it.MoveNext())
					WriteObject(it.Current);
		}
	}
}
