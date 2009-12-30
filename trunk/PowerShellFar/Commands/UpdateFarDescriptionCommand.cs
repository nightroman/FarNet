/*
PowerShellFar plugin for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.ComponentModel;
using System.Management.Automation;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Update-FarDescription command.
	/// Updates Far description file for a directory.
	/// </summary>
	[Description("Updates Far description file for a directory.")]
	public sealed class UpdateFarDescriptionCommand : BaseCmdlet
	{
		///
		[Parameter(Position = 0, HelpMessage = "Directory path.")]
		[AllowEmptyString]
		public string Path { get; set; }

		///
		protected override void BeginProcessing()
		{
			if (Stop())
				return;

			if (Path == null)
				Path = Environment.CurrentDirectory;

			bool exists;
			Description.UpdateDescriptionFile(Description.GetDescriptionFile(Path, out exists));
		}
	}
}
