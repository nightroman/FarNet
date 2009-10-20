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
	/// Get-FarFile command.
	/// Gets panel file(s).
	/// </summary>
	/// <seealso cref="IPanel.CurrentFile"/>
	/// <seealso cref="IPanel.ShownFiles"/>
	/// <seealso cref="IPanel.SelectedFiles"/>
	[Description("Gets panel file(s).")]
	public sealed class GetFarFileCommand : BaseFileCmdlet
	{
		///
		protected override void BeginProcessing()
		{
			IPanel panel = Passive ? A.Far.Panel2 : A.Far.Panel;

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
