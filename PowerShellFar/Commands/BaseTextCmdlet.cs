/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Common features of cmdlets opening text files.
	/// </summary>
	public class BaseTextCmdlet : BaseCmdlet
	{
		internal const string _helpModal = "Open in modal mode. By default it is not but it depends on where it is opened.";

		///
		[Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The path of a file to be opened.")]
		[Alias("FilePath", "FileName")]
		public string Path
		{
			get { return _FilePath; }
			set { _FilePath = value; }
		}
		string _FilePath;

		///
		[Parameter(HelpMessage = "Window title. Default is the file path.")]
		public string Title
		{
			get { return _Title; }
			set { _Title = value; }
		}
		string _Title;

		///
		[Parameter(HelpMessage = "Tells when and how to delete the file when closed.")]
		public DeleteSource DeleteSource
		{
			get { return _DeleteSource; }
			set { _DeleteSource = value; }
		}
		DeleteSource _DeleteSource;

		///
		[Parameter(HelpMessage = "Don't add the file to the history.")]
		public SwitchParameter DisableHistory
		{
			get { return _DisableHistory; }
			set { _DisableHistory = value; }
		}
		SwitchParameter _DisableHistory;

		///
		[Parameter(HelpMessage = "Switching between editor and viewer.")]
		public Switching Switching
		{
			get { return _Switching; }
			set { _Switching = value; }
		}
		Switching _Switching;
	}
}
