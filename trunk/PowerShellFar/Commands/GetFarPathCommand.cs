/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
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
			IAnyPanel panel1 = Passive ? Far.Host.Panel2 : Far.Host.Panel;
			IAnyPanel panel2 = Mirror ? (Passive ? Far.Host.Panel : Far.Host.Panel2) : panel1;

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
