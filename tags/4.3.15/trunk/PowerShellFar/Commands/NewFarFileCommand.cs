
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// New-FarFile command.
	/// Creates a panel file.
	/// </summary>
	/// <seealso cref="FarFile"/>
	[Description("Creates a panel file (custom or from a file system info.")]
	public sealed class NewFarFileCommand : BaseCmdlet
	{
		#region Any parameter set
		/// <summary>
		/// See <see cref="FarFile.Description"/>.
		/// </summary>
		[Parameter(Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Sets FarFile.Description")]
		public string Description { get; set; }
		/// <summary>
		/// See <see cref="FarFile.Owner"/>.
		/// </summary>
		[Parameter(HelpMessage = "Sets FarFile.Owner")]
		public string Owner { get; set; }
		/// <summary>
		/// See <see cref="FarFile.Columns"/>.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		[Parameter(HelpMessage = "Sets FarFile.Columns")]
		public System.Collections.ICollection Columns { get; set; }
		/// <summary>
		/// See <see cref="FarFile.Data"/>.
		/// </summary>
		[Parameter(HelpMessage = "Sets FarFile.Data")]
		public object Data { get; set; }
		#endregion
		#region Name parameter set
		/// <summary>
		/// See <see cref="FarFile.Name"/>.
		/// </summary>
		[Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, HelpMessage = "Sets FarFile.Name", ParameterSetName = "Name")]
		[AllowEmptyString]
		public string Name { get; set; }
		/// <summary>
		/// See <see cref="FarFile.Length"/>.
		/// </summary>
		[Parameter(HelpMessage = "Sets FarFile.Length", ParameterSetName = "Name")]
		public long Length { get; set; }
		/// <summary>
		/// See <see cref="FarFile.CreationTime"/>.
		/// </summary>
		[Parameter(HelpMessage = "Sets FarFile.CreationTime", ParameterSetName = "Name")]
		public DateTime CreationTime { get; set; }
		/// <summary>
		/// See <see cref="FarFile.LastAccessTime"/>.
		/// </summary>
		[Parameter(HelpMessage = "Sets FarFile.LastAccessTime", ParameterSetName = "Name")]
		public DateTime LastAccessTime { get; set; }
		/// <summary>
		/// See <see cref="FarFile.LastWriteTime"/>.
		/// </summary>
		[Parameter(HelpMessage = "Sets FarFile.LastWriteTime", ParameterSetName = "Name")]
		public DateTime LastWriteTime { get; set; }
		/// <summary>
		/// See <see cref="FarFile.Attributes"/>.
		/// </summary>
		[Parameter(HelpMessage = "Sets FarFile.Attributes", ParameterSetName = "Name")]
		public FileAttributes Attributes { get; set; }
		#endregion
		#region File parameter set
		/// <summary>
		/// File system info (file or directory).
		/// </summary>
		[Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, HelpMessage = "File system info.", ParameterSetName = "File")]
		public FileSystemInfo File { get; set; }
		/// <summary>
		/// Tells to use the full name for a file system item.
		/// </summary>
		[Parameter(HelpMessage = "Tells to use the full name for a file system item.", ParameterSetName = "File")]
		public SwitchParameter FullName { get; set; }
		#endregion
		///
		protected override void ProcessRecord()
		{
			// system file
			if (File != null)
			{
				var ff = new SetFile(File, FullName);

				ff.Description = Description;
				ff.Owner = Owner;
				ff.Columns = Columns;
				if (Data == null)
					ff.Data = File;
				else
					ff.Data = Data;

				WriteObject(ff);
				return;
			}

			// user file
			WriteObject(new SetFile()
			{
				Name = Name,
				Description = Description,
				Owner = Owner,
				Length = Length,
				CreationTime = CreationTime,
				LastAccessTime = LastAccessTime,
				LastWriteTime = LastWriteTime,
				Attributes = Attributes,
				Columns = Columns,
				Data = Data,
			});
		}
	}
}
