
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System;
using System.IO;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	sealed class NewFarFileCommand : BaseCmdlet
	{
		#region [ Any parameter set ]
		[Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
		public string Description { get; set; }
		[Parameter]
		public string Owner { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		[Parameter]
		public System.Collections.ICollection Columns { get; set; }
		[Parameter]
		public object Data { get; set; }
		#endregion
		#region [ Name parameter set ]
		/// <summary>
		/// See <see cref="FarFile.Name"/>.
		/// </summary>
		[Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Name")]
		[AllowEmptyString]
		public string Name { get; set; }
		[Parameter(ParameterSetName = "Name")]
		public long Length { get; set; }
		[Parameter(ParameterSetName = "Name")]
		public DateTime CreationTime { get; set; }
		[Parameter(ParameterSetName = "Name")]
		public DateTime LastAccessTime { get; set; }
		[Parameter(ParameterSetName = "Name")]
		public DateTime LastWriteTime { get; set; }
		[Parameter(ParameterSetName = "Name")]
		public FileAttributes Attributes { get; set; }
		#endregion
		#region [ File parameter set ]
		[Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ParameterSetName = "File")]
		public FileSystemInfo File { get; set; }
		[Parameter(ParameterSetName = "File")]
		public SwitchParameter FullName { get; set; }
		#endregion
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
