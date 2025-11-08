using FarNet;
using System.Management.Automation;

namespace PowerShellFar.Commands;

[Cmdlet("Open", "FarEditor", DefaultParameterSetName = PsnMain)]
sealed class OpenFarEditorCommand : NewFarEditorCommand
{
	[Parameter(ParameterSetName = PsnMain)]
	public SwitchParameter Modal { get; set; }

	[Parameter(ParameterSetName = "Detach", Mandatory = true)]
	public SwitchParameter Detach { get; set; }

	protected override void ProcessRecord()
	{
		if (Detach)
		{
			DoDetach();
			return;
		}

		var editor = CreateEditor();
		if (Modal)
			editor.Open(OpenMode.Modal);
		else
			editor.Open();
	}

	private static void DoDetach()
	{
		var editor = Far.Api.Editor;
		if (editor is null)
			return;

		var fileName = editor.FileName;
		var frame = editor.Frame;

		editor.Save();
		editor.Close();

		My.ProcessEx.StartFar($"""
			/e{frame.CaretLine + 1}:{frame.CaretColumn + 1} "{fileName}"
			""");
	}
}
