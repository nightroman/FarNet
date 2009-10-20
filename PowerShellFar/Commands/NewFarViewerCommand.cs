/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System.ComponentModel;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// New-FarViewer command.
	/// Creates a viewer for other settings before opening.
	/// </summary>
	/// <seealso cref="IViewer"/>
	/// <seealso cref="IFar.CreateViewer"/>
	[Description("Creates a viewer for other settings before opening.")]
	public class NewFarViewerCommand : BaseTextCmdlet
	{
		internal IViewer CreateViewer()
		{
			IViewer viewer = A.Far.CreateViewer();
			viewer.DeleteSource = DeleteSource;
			viewer.DisableHistory = DisableHistory;
			viewer.FileName = Path;
			viewer.Switching = Switching;
			viewer.Title = Title;
			return viewer;
		}

		///
		protected override void ProcessRecord()
		{
			if (Stop())
				return;
			WriteObject(CreateViewer());
		}
	}
}
