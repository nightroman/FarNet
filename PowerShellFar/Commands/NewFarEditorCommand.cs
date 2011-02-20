
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.ComponentModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// New-FarEditor command.
	/// Creates an editor for other settings before opening.
	/// </summary>
	/// <seealso cref="IEditor"/>
	/// <seealso cref="IFar.CreateEditor"/>
	/// <seealso cref="StartFarEditorCommand"/>
	[Description("Creates an editor for other settings before opening.")]
	public class NewFarEditorCommand : BaseTextCmdlet
	{
		/// <summary>
		/// Line number to open the editor at. The first is 1.
		/// </summary>
		[Parameter(Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Line number to open the editor at. The first is 1.")]
		public int LineNumber
		{
			get { return _LineNumber; }
			set { _LineNumber = value; }
		}
		int _LineNumber;
		/// <summary>
		/// Character number in the line to open the editor at. The first is 1.
		/// </summary>
		[Parameter(Position = 2, HelpMessage = "Character number in the line to open the editor at. The first is 1.")]
		public int CharNumber
		{
			get { return _CharNumber; }
			set { _CharNumber = value; }
		}
		int _CharNumber;
		/// <summary>
		/// The host instance. See <see cref="IEditor.Host"/>.
		/// </summary>
		[Parameter(HelpMessage = "The host instance.")]
		public new PSObject Host { get; set; }
		internal IEditor CreateEditor()
		{
			IEditor editor = Far.Net.CreateEditor();
			editor.DeleteSource = DeleteSource;
			editor.DisableHistory = DisableHistory;
			editor.FileName = Path;
			editor.Host = Host;
			editor.Switching = Switching;
			editor.Title = Title;
			editor.GoTo(_CharNumber - 1, _LineNumber - 1);

			return editor;
		}
		///
		protected override void ProcessRecord()
		{
			WriteObject(CreateEditor());
		}
	}
}
