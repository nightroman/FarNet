/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.ComponentModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// New-FarFile command.
	/// Creates a panel file.
	/// </summary>
	/// <seealso cref="FarFile"/>
	[Description("Creates a panel file.")]
	public sealed class NewFarFileCommand : BaseCmdlet
	{
		///
		[Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, HelpMessage = "Sets FarFile.Name")]
		[AllowEmptyString]
		public string Name { get; set; }

		///
		[Parameter(Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Sets FarFile.Description")]
		public string Description { get; set; }

		///
		[Parameter(HelpMessage = "Sets FarFile.AlternateName")]
		public string AlternateName { get; set; }

		///
		[Parameter(HelpMessage = "Sets FarFile.Owner")]
		public string Owner { get; set; }

		///
		[Parameter(HelpMessage = "Sets FarFile.Length")]
		public long Length { get; set; }

		///
		[Parameter(HelpMessage = "Sets FarFile.Data")]
		public object Data { get; set; }

		///
		[Parameter(HelpMessage = "Sets FarFile.CreationTime")]
		public DateTime CreationTime { get; set; }

		///
		[Parameter(HelpMessage = "Sets FarFile.LastAccessTime")]
		public DateTime LastAccessTime { get; set; }

		///
		[Parameter(HelpMessage = "Sets FarFile.LastWriteTime")]
		public DateTime LastWriteTime { get; set; }

		///
		[Parameter(HelpMessage = "Sets FarFile.Columns")]
		public System.Collections.ICollection Columns { get; set; }

		///
		protected override void ProcessRecord()
		{
			SetFile file = new SetFile();

			file.Name = Name;
			file.Description = Description;
			file.AlternateName = AlternateName;
			file.Owner = Owner;
			file.Length = Length;
			file.Data = Data;

			file.CreationTime = CreationTime;
			file.LastAccessTime = LastAccessTime;
			file.LastWriteTime = LastWriteTime;

			file.Columns = Columns;

			WriteObject(file);
		}
	}
}
