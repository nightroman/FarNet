
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System.Management.Automation;

namespace PowerShellFar.Commands;

[OutputType(typeof(IViewer))]
class NewFarViewerCommand : BaseTextCmdlet
{
	[Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
	[Alias("FilePath", "FileName")]
	public string Path { get; set; } = null!;

	internal IViewer CreateViewer()
	{
		var viewer = Far.Api.CreateViewer();
		viewer.DeleteSource = DeleteSource;
		viewer.DisableHistory = DisableHistory;
		viewer.FileName = GetUnresolvedProviderPathFromPSPath(Path);
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
