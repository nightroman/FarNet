/*
FarNet plugin for Far Manager
Copyright (c) 2005-2009 FarNet Team
*/

using System;
using System.Collections;
using System.IO;

namespace FarNet
{
	/// <summary>
	/// "Abstract" <see cref="IPanel"/>'s item representing a file, a directory or a plugin item.
	/// </summary>
	/// <remarks>
	/// Plugin panels may implement derived classes (at least <see cref="Name"/> has to be defined)
	/// or they may just use the ready straightforward implementation <see cref="SetFile"/>.
	/// </remarks>
	public class FarFile
	{
		/// <summary>
		/// File name.
		/// </summary>
		public virtual string Name
		{
			get { return null; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Description.
		/// </summary>
		public virtual string Description
		{
			get { return null; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Owner.
		/// </summary>
		public virtual string Owner
		{
			get { return null; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Alternate name, can be used as a file system name.
		/// </summary>
		/// <seealso cref="IPluginPanelInfo.AutoAlternateNames"/>
		public virtual string AlternateName
		{
			get { return null; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// User data. Only for <see cref="IPluginPanel"/>.
		/// </summary>
		public virtual object Data
		{
			get { return null; } //??? _090610_071700
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Creation time.
		/// </summary>
		public virtual DateTime CreationTime
		{
			get { return new DateTime(); }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Last access time.
		/// </summary>
		public virtual DateTime LastAccessTime
		{
			get { return new DateTime(); }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Last access time.
		/// </summary>
		public virtual DateTime LastWriteTime
		{
			get { return new DateTime(); }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// File length.
		/// </summary>
		public virtual long Length
		{
			get { return 0; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Custom columns. See <see cref="PanelModeInfo"/>.
		/// </summary>
		public virtual ICollection Columns
		{
			get { return null; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// File attributes. All <c>Is*</c> properties are based on this value.
		/// </summary>
		/// <remarks>
		/// Derived class may override this property and cannot override <c>Is*</c>.
		/// All <c>Is*</c> properties are completely mapped to this value.
		/// </remarks>
		public virtual FileAttributes Attributes
		{
			get { return 0; }
			set { throw new NotSupportedException(); }
		}
		/// <summary>
		/// Read only attribute.
		/// See <see cref="Attributes"/>.
		/// </summary>
		public bool IsReadOnly
		{
			get { return (Attributes & FileAttributes.ReadOnly) != 0; }
			set { Attributes = value ? (Attributes | FileAttributes.ReadOnly) : (Attributes & ~FileAttributes.ReadOnly); }
		}
		/// <summary>
		/// Hidden attribute.
		/// See <see cref="Attributes"/>.
		/// </summary>
		public bool IsHidden
		{
			get { return (Attributes & FileAttributes.Hidden) != 0; }
			set { Attributes = value ? (Attributes | FileAttributes.Hidden) : (Attributes & ~FileAttributes.Hidden); }
		}
		/// <summary>
		/// System attribute.
		/// See <see cref="Attributes"/>.
		/// </summary>
		public bool IsSystem
		{
			get { return (Attributes & FileAttributes.System) != 0; }
			set { Attributes = value ? (Attributes | FileAttributes.System) : (Attributes & ~FileAttributes.System); }
		}
		/// <summary>
		/// Directory attribute.
		/// See <see cref="Attributes"/>.
		/// </summary>
		public bool IsDirectory
		{
			get { return (Attributes & FileAttributes.Directory) != 0; }
			set { Attributes = value ? (Attributes | FileAttributes.Directory) : (Attributes & ~FileAttributes.Directory); }
		}
		/// <summary>
		/// Archive attribute.
		/// See <see cref="Attributes"/>.
		/// </summary>
		public bool IsArchive
		{
			get { return (Attributes & FileAttributes.Archive) != 0; }
			set { Attributes = value ? (Attributes | FileAttributes.Archive) : (Attributes & ~FileAttributes.Archive); }
		}
		/// <summary>
		/// Reparse point attribute.
		/// See <see cref="Attributes"/>.
		/// </summary>
		public bool IsReparsePoint
		{
			get { return (Attributes & FileAttributes.ReparsePoint) != 0; }
			set { Attributes = value ? (Attributes | FileAttributes.ReparsePoint) : (Attributes & ~FileAttributes.ReparsePoint); }
		}
		/// <summary>
		/// Compressed attribute.
		/// See <see cref="Attributes"/>.
		/// </summary>
		public bool IsCompressed
		{
			get { return (Attributes & FileAttributes.Compressed) != 0; }
			set { Attributes = value ? (Attributes | FileAttributes.Compressed) : (Attributes & ~FileAttributes.Compressed); }
		}
		/// <summary>
		/// Encrypted attribute.
		/// See <see cref="Attributes"/>.
		/// </summary>
		public bool IsEncrypted
		{
			get { return (Attributes & FileAttributes.Encrypted) != 0; }
			set { Attributes = value ? (Attributes | FileAttributes.Encrypted) : (Attributes & ~FileAttributes.Encrypted); }
		}
		///
		public override string ToString()
		{
			return Name;
		}
	}

	/// <summary>
	/// Straightforward implementation of <see cref="FarFile"/> ready to use by <see cref="IPluginPanel"/> panels.
	/// </summary>
	/// <remarks>
	/// It is just a set of properties and any property can be set.
	/// In most cases plugin panels should just use this class for their items.
	/// In some cases they may use custom implementations of <see cref="FarFile"/>.
	/// </remarks>
	public sealed class SetFile : FarFile
	{
		/// <summary>
		/// Creates an empty file data object.
		/// </summary>
		public SetFile()
		{ }
		/// <summary>
		/// Creates file data snapshot from a <see cref="FarFile"/> object.
		/// </summary>
		/// <param name="file">Any panel file which data are taken.</param>
		public SetFile(FarFile file)
		{
			if (file == null)
				throw new ArgumentNullException("file");

			AlternateName = file.AlternateName;
			Attributes = file.Attributes;
			CreationTime = file.CreationTime;
			Data = file.Data;
			Description = file.Description;
			LastAccessTime = file.LastAccessTime;
			LastWriteTime = file.LastWriteTime;
			Length = file.Length;
			Name = file.Name;
			Owner = file.Owner;
		}
		/// <summary>
		/// Creates file data snapshot from a <see cref="FileSystemInfo"/> object.
		/// </summary>
		/// <param name="info">File system item info (normally <see cref="FileInfo"/> or <see cref="DirectoryInfo"/>).</param>
		/// <param name="fullName">Use the full name (path) as the name.</param>
		public SetFile(FileSystemInfo info, bool fullName)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			Name = fullName ? info.FullName : info.Name;
			CreationTime = info.CreationTime;
			LastAccessTime = info.LastAccessTime;
			LastWriteTime = info.LastWriteTime;
			Attributes = info.Attributes;

			if ((Attributes & FileAttributes.Directory) == 0)
				Length = ((FileInfo)info).Length;
		}
		/// <summary>
		/// File name.
		/// </summary>
		public override string Name { get; set; }
		/// <summary>
		/// Description.
		/// </summary>
		public override string Description { get; set; }
		/// <summary>
		/// Owner.
		/// </summary>
		public override string Owner { get; set; }
		/// <summary>
		/// Alternate name, can be used as a file system name.
		/// </summary>
		/// <seealso cref="IPluginPanelInfo.AutoAlternateNames"/>
		public override string AlternateName { get; set; }
		/// <summary>
		/// Creation time.
		/// </summary>
		public override DateTime CreationTime { get; set; }
		/// <summary>
		/// Last access time.
		/// </summary>
		public override DateTime LastAccessTime { get; set; }
		/// <summary>
		/// Last access time.
		/// </summary>
		public override DateTime LastWriteTime { get; set; }
		/// <summary>
		/// File length.
		/// </summary>
		public override long Length { get; set; }
		/// <summary>
		/// User data. Only for <see cref="IPluginPanel"/>.
		/// </summary>
		public override object Data { get; set; }
		/// <summary>
		/// File attributes.
		/// </summary>
		public override FileAttributes Attributes { get; set; }
		/// <summary>
		/// Custom columns. See <see cref="PanelModeInfo"/>.
		/// </summary>
		public override ICollection Columns { get; set; }
	}
}
