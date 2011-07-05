
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
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
			IViewer viewer = Far.Net.CreateViewer();
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
			WriteObject(CreateViewer());
		}
	}
}
