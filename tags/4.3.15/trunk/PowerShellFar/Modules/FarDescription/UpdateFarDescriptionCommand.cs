
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.ComponentModel;
using System.Management.Automation;

namespace FarDescription
{
	[Cmdlet(VerbsData.Update, Res.Noun)]
	[Description("Updates the description file in the directory. If the file becomes empty then it is removed.")]
	public sealed class UpdateFarDescriptionCommand : PSCmdlet
	{
		[Parameter(Position = 0, HelpMessage = "The directory path where the description file is. Default = empty = the process current directory.")]
		[AllowEmptyString]
		public string Path { get; set; }

		protected override void BeginProcessing()
		{
			if (Path == null)
				Path = Environment.CurrentDirectory;

			bool exists;
			Description.UpdateDescriptionFile(Description.GetDescriptionFile(Path, out exists));
		}
	}
}
