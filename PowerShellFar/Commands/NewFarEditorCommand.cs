using FarNet;
using System.Management.Automation;

namespace PowerShellFar.Commands;

[OutputType(typeof(IEditor))]
class NewFarEditorCommand : BaseTextCmdlet
{
	[Parameter(ParameterSetName = PsnMain, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
	[Alias("FilePath", "FileName")]
	public string? Path { get; set; }

	[Parameter(ParameterSetName = PsnMain, Position = 1, ValueFromPipelineByPropertyName = true)]
	public int LineNumber { get; set; }

	[Parameter(ParameterSetName = PsnMain, Position = 2)]
	public int CharNumber { get; set; }

	[Parameter(ParameterSetName = PsnMain)]
	public new PSObject? Host { get; set; }

	[Parameter(ParameterSetName = PsnMain)]
	public SwitchParameter IsLocked { set => _IsLocked = value; }
	SwitchParameter? _IsLocked;

	internal IEditor CreateEditor()
	{
		var editor = Far.Api.CreateEditor();
		editor.DeleteSource = DeleteSource;
		editor.DisableHistory = DisableHistory;
		editor.Host = Host;
		editor.Switching = Switching;
		editor.Title = Title;
		editor.GoTo(CharNumber - 1, LineNumber - 1);
		if (!string.IsNullOrEmpty(Path))
			editor.FileName = GetUnresolvedProviderPathFromPSPath(Path);
		if (CodePage >= 0)
			editor.CodePage = CodePage;
		if (_IsLocked.HasValue)
			editor.IsLocked = _IsLocked.Value;

		return editor;
	}

	protected override void ProcessRecord()
	{
		WriteObject(CreateEditor());
	}
}
