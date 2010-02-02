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
	/// Get-FarFile command.
	/// Gets panel file(s).
	/// </summary>
	/// <seealso cref="IAnyPanel.CurrentFile"/>
	/// <seealso cref="IAnyPanel.ShownFiles"/>
	/// <seealso cref="IAnyPanel.SelectedFiles"/>
	[Description("Gets panel file(s).")]
	public sealed class GetFarFileCommand : BaseFileCmdlet
	{
		///
		protected override void BeginProcessing()
		{
			IAnyPanel panel = Passive ? A.Far.Panel2 : A.Far.Panel;

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
