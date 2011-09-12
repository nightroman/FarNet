
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2011 Roman Kuzmin
*/

using System.Collections.Generic;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	sealed class GetFarItemCommand : BaseFileCmdlet
	{
		protected override void BeginProcessing()
		{
			IPanel panel = Passive ? Far.Net.Panel2 : Far.Net.Panel;
			if (panel == null)
				return;

			// case: PSF panel
			var ap = panel as AnyPanel;
			if (ap != null)
			{
				IEnumerable<PSObject> items;
				if (All)
				{
					items = ap.ShownItems;
				}
				else if (Selected)
				{
					items = ap.SelectedItems;
				}
				else
				{
					WriteObject(ap.CurrentItem);
					return;
				}

				foreach (PSObject o in items)
				{
					if (o != null)
						WriteObject(o);
				}

				return;
			}

			// get and convert paths to items
			IList<FarFile> filesToProcess;
			if (All)
			{
				filesToProcess = panel.ShownFiles;
			}
			else if (Selected)
			{
				filesToProcess = panel.SelectedFiles;
			}
			else
			{
				WriteObject(InvokeCommand.NewScriptBlock("Get-Item -LiteralPath $args[0] -Force -ErrorAction 0").Invoke(GetCurrentPath(panel, panel)), true);
				return;
			}

			//! Bug [_090116_085532]
			// Count is 0, e.g. for SelectedFiles when nothing is selected and the current item is dots;
			// in this case Get-Item -LiteralPath fails: cannot bind an empty array to LiteralPath.
			if (filesToProcess.Count > 0)
			{
				//! @($args[0])
				using(IEnumerator<string> it = new PathEnumerator(filesToProcess, panel.CurrentDirectory, panel.RealNames, false))
					WriteObject(InvokeCommand.NewScriptBlock("Get-Item -LiteralPath @($args[0]) -Force -ErrorAction 0").Invoke(it), true);
			}
		}
	}
}
