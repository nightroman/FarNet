
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System.Collections.Generic;
using FarNet;

namespace PowerShellFar.Commands
{
	sealed class GetFarFileCommand : BaseFileCmdlet
	{
		protected override void BeginProcessing()
		{
			IPanel panel = Passive ? Far.Net.Panel2 : Far.Net.Panel;

			// no panel?
			if (panel == null)
				return;

			// get path(s)
			IList<FarFile> files;
			if (All)
			{
				files = panel.ShownFiles;
			}
			else if (Selected)
			{
				files = panel.SelectedFiles;
			}
			else
			{
				WriteObject(panel.CurrentFile);
				return;
			}

			foreach(FarFile file in files)
				WriteObject(file);
		}
	}
}
