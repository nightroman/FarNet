
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	class NewFarEditorCommand : BaseTextCmdlet
	{
		[Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
		[Alias("FilePath", "FileName")]
		public string Path { get; set; }
		[Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
		public int LineNumber { get; set; }
		[Parameter(Position = 2)]
		public int CharNumber { get; set; }
		[Parameter]
		public new PSObject Host { get; set; }
		[Parameter]
		public SwitchParameter IsLocked { get { return _IsLocked.GetValueOrDefault(); } set { _IsLocked = value; } }
		SwitchParameter? _IsLocked;
		internal IEditor CreateEditor()
		{
			IEditor editor = Far.Api.CreateEditor();
			editor.DeleteSource = DeleteSource;
			editor.DisableHistory = DisableHistory;
			editor.FileName = Path;
			editor.Host = Host;
			editor.Switching = Switching;
			editor.Title = Title;
			editor.GoTo(CharNumber - 1, LineNumber - 1);
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
}
