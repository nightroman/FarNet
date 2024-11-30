
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System.Collections.Generic;
using System.Management.Automation;

namespace PowerShellFar.Commands;

sealed class GetFarItemCommand : BaseFileCmdlet
{
	protected override void BeginProcessing()
	{
		var panel = Passive ? Far.Api.Panel2 : Far.Api.Panel;
		if (panel is null)
			return;

		// case: PSF panel
		if (panel is AnyPanel ap)
		{
			IEnumerable<PSObject?> items;
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

			foreach (var o in items)
			{
				if (o != null)
					WriteObject(o);
			}

			return;
		}

		// get and convert paths to items
		FarFile[] filesToProcess;
		if (All)
		{
			filesToProcess = panel.GetFiles();
		}
		else if (Selected)
		{
			filesToProcess = panel.GetSelectedFiles();
		}
		else
		{
			WriteObject(InvokeCommand.NewScriptBlock("Get-Item -LiteralPath $args[0] -Force -ErrorAction 0").Invoke(GetCurrentPath(panel, panel)), true);
			return;
		}

		//! Bug [_090116_085532]
		// Count is 0, e.g. when nothing is selected and the current item is dots;
		// then Get-Item -LiteralPath fails: cannot bind an empty array to LiteralPath.
		if (filesToProcess.Length > 0)
		{
			//! @($args[0])
			using IEnumerator<string> it = new PathEnumerator(filesToProcess, panel.CurrentDirectory, panel.RealNames, false);
			WriteObject(InvokeCommand.NewScriptBlock("Get-Item -LiteralPath @($args[0]) -Force -ErrorAction 0").Invoke(it), true);
		}
	}
}
