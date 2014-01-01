
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

using System;
using System.ComponentModel;
using System.Management.Automation;

namespace FarDescription
{
	[Cmdlet(VerbsData.Update, Res.Noun)]
	public sealed class UpdateFarDescriptionCommand : PSCmdlet
	{
		[Parameter(Position = 0)]
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
