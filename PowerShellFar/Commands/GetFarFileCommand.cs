
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System.Collections.Generic;
using System.Management.Automation;

namespace PowerShellFar.Commands;

[OutputType(typeof(FarFile))]
sealed class GetFarFileCommand : BaseFileCmdlet
{
	protected override void BeginProcessing()
	{
		IPanel panel = Passive ? Far.Api.Panel2 : Far.Api.Panel;

		// no panel?
		if (panel is null)
			return;

		// get path(s)
		IList<FarFile> files;
		if (All)
		{
			files = panel.GetFiles();
		}
		else if (Selected)
		{
			files = panel.GetSelectedFiles();
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
