
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using FarNet;

namespace PowerShellFar.Commands
{
	class NewFarViewerCommand : BaseTextCmdlet
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
		protected override void ProcessRecord()
		{
			WriteObject(CreateViewer());
		}
	}
}
