/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Get-FarPath command.
	/// Gets panel item paths.
	/// </summary>
	[Description("Gets panel item paths.")]
	public sealed class GetFarPathCommand : BaseFileCmdlet
	{
		/// <summary>
		/// Join requested item names with the opposite panel path.
		/// </summary>
		[Parameter(HelpMessage = "Join requested item names with the opposite panel path.")]
		public SwitchParameter Mirror { get; set; }

		///
		protected override void BeginProcessing()
		{
			IPanel panel1 = Passive ? A.Far.Panel2 : A.Far.Panel;
			IPanel panel2 = Mirror ? (Passive ? A.Far.Panel : A.Far.Panel2) : panel1;

			// no panel?
			if (panel1 == null || panel2 == null)
				return;

			// get path(s)
			IEnumerator<string> it;
			if (All)
			{
				it = new PathEnumerator(panel1.ShownFiles, panel2.Path, panel1.RealNames, panel1 != panel2);
			}
			else if (Selected)
			{
				it = new PathEnumerator(panel1.SelectedFiles, panel2.Path, panel1.RealNames, panel1 != panel2);
			}
			else
			{
				WriteObject(GetCurrentPath(panel1, panel2));
				return;
			}
			while (it.MoveNext())
				WriteObject(it.Current);
		}

	}
}
