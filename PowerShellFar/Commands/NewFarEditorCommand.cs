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
		///
		[Parameter(Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Line number to open the editor at. The first is 1.")]
		public int LineNumber
		{
			get { return _LineNumber; }
			set { _LineNumber = value; }
		}
		int _LineNumber;

		///
		[Parameter(Position = 2, HelpMessage = "Character number in the line to open the editor at. The first is 1.")]
		public int CharNumber
		{
			get { return _CharNumber; }
			set { _CharNumber = value; }
		}
		int _CharNumber;

		///
		[Parameter(HelpMessage = "Any user data not used internally.")]
		public object Data
		{
			get { return _Data; }
			set { _Data = value; }
		}
		object _Data;

		///
		internal IEditor CreateEditor()
		{
			IEditor editor = A.Far.CreateEditor();
			editor.Data = _Data;
			editor.DeleteSource = DeleteSource;
			editor.DisableHistory = DisableHistory;
			editor.FileName = Path;
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
