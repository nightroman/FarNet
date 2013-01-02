
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	class NewFarViewerCommand : BaseTextCmdlet
	{
		[Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
		[Alias("FilePath", "FileName")]
		public string Path { get; set; }
		internal IViewer CreateViewer()
		{
			IViewer viewer = Far.Api.CreateViewer();
			viewer.DeleteSource = DeleteSource;
			viewer.DisableHistory = DisableHistory;
			viewer.FileName = Path;
			viewer.Switching = Switching;
			viewer.Title = Title;
			if (CodePage >= 0)
				viewer.CodePage = CodePage;
			return viewer;
		}
		protected override void ProcessRecord()
		{
			WriteObject(CreateViewer());
		}
	}
}
