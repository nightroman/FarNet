
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2011 Roman Kuzmin
*/

using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Common features of cmdlets opening text files.
	/// </summary>
	class BaseTextCmdlet : BaseCmdlet
	{
		[Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
		[Alias("FilePath", "FileName")]
		public string Path { get; set; }
		[Parameter()]
		public string Title { get; set; }
		[Parameter()]
		public DeleteSource DeleteSource { get; set; }
		[Parameter()]
		public SwitchParameter DisableHistory { get; set; }
		[Parameter()]
		public Switching Switching { get; set; }
	}
}
