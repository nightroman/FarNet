
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace FarNet;

/// <summary>
/// Abstract panel item representing native files and directories, plugin panel items, and module panel items.
/// </summary>
/// <remarks>
/// Modules may implement derived classes in order to represent their panel
/// items effectively. Alternatively, they may use <see cref="SetFile"/>,
/// the simple property set.
/// <para>
/// Although this class is abstract all its virtual properties are defined,
/// they get default values and throw <c>NotImplementedException</c> on
/// setting. Thus, derived classes do not have to implement every property.
/// At least <see cref="Name"/> has to be defined in order to be shown in
/// a panel, other properties are implemented when needed.
/// </para>
/// </remarks>
public abstract class FarFile : IXmlInfo
{
	/// <summary>
	/// File name.
	/// </summary>
	public virtual string Name
	{
		get => string.Empty;
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// Description.
	/// </summary>
	public virtual string? Description
	{
		get => null;
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// Owner.
	/// </summary>
	public virtual string? Owner
	{
		get => null;
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// User data. Only for <see cref="Panel"/>.
	/// </summary>
	public virtual object? Data
	{
		get => null;  //??? _090610_071700
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// Creation time.
	/// </summary>
	public virtual DateTime CreationTime
	{
		get => default;
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// Last access time.
	/// </summary>
	public virtual DateTime LastAccessTime
	{
		get => default;
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// Last write time.
	/// </summary>
	public virtual DateTime LastWriteTime
	{
		get => default;
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// File length.
	/// </summary>
	public virtual long Length
	{
		get => 0;
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// Custom columns. See <see cref="PanelPlan"/>.
	/// </summary>
	public virtual ICollection? Columns
	{
		get => null;
		set => throw new NotImplementedException();
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
		get => 0;
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// Read only attribute.
	/// See <see cref="Attributes"/>.
	/// </summary>
	public bool IsReadOnly
	{
		get => (Attributes & FileAttributes.ReadOnly) != 0;
		set => Attributes = value ? (Attributes | FileAttributes.ReadOnly) : (Attributes & ~FileAttributes.ReadOnly);
	}

	/// <summary>
	/// Hidden attribute.
	/// See <see cref="Attributes"/>.
	/// </summary>
	public bool IsHidden
	{
		get => (Attributes & FileAttributes.Hidden) != 0;
		set => Attributes = value ? (Attributes | FileAttributes.Hidden) : (Attributes & ~FileAttributes.Hidden);
	}

	/// <summary>
	/// System attribute.
	/// See <see cref="Attributes"/>.
	/// </summary>
	public bool IsSystem
	{
		get => (Attributes & FileAttributes.System) != 0;
		set => Attributes = value ? (Attributes | FileAttributes.System) : (Attributes & ~FileAttributes.System);
	}

	/// <summary>
	/// Directory attribute.
	/// See <see cref="Attributes"/>.
	/// </summary>
	public bool IsDirectory
	{
		get => (Attributes & FileAttributes.Directory) != 0;
		set => Attributes = value ? (Attributes | FileAttributes.Directory) : (Attributes & ~FileAttributes.Directory);
	}

	/// <summary>
	/// Archive attribute.
	/// See <see cref="Attributes"/>.
	/// </summary>
	public bool IsArchive
	{
		get => (Attributes & FileAttributes.Archive) != 0;
		set => Attributes = value ? (Attributes | FileAttributes.Archive) : (Attributes & ~FileAttributes.Archive);
	}

	/// <summary>
	/// Reparse point attribute.
	/// See <see cref="Attributes"/>.
	/// </summary>
	public bool IsReparsePoint
	{
		get => (Attributes & FileAttributes.ReparsePoint) != 0;
		set => Attributes = value ? (Attributes | FileAttributes.ReparsePoint) : (Attributes & ~FileAttributes.ReparsePoint);
	}

	/// <summary>
	/// Compressed attribute.
	/// See <see cref="Attributes"/>.
	/// </summary>
	public bool IsCompressed
	{
		get => (Attributes & FileAttributes.Compressed) != 0;
		set => Attributes = value ? (Attributes | FileAttributes.Compressed) : (Attributes & ~FileAttributes.Compressed);
	}

	/// <summary>
	/// Encrypted attribute.
	/// See <see cref="Attributes"/>.
	/// </summary>
	public bool IsEncrypted
	{
		get => (Attributes & FileAttributes.Encrypted) != 0;
		set => Attributes = value ? (Attributes | FileAttributes.Encrypted) : (Attributes & ~FileAttributes.Encrypted);
	}

	/// <summary>
	/// Returns the file name.
	/// </summary>
	public sealed override string ToString()
	{
		return Name;
	}

	/// <summary>
	/// INTERNAL
	/// </summary>
	public virtual string XmlNodeName()
	{
		return IsDirectory ? "Directory" : "File";
	}

	static ReadOnlyCollection<XmlAttributeInfo>? _attrs;
	static ReadOnlyCollection<XmlAttributeInfo> XmlAttr()
	{
		if (_attrs != null)
			return _attrs;

		var attrs = new XmlAttributeInfo[]
		{
			new XmlAttributeInfo("Name", (object file) => ((FarFile)file).Name),
			new XmlAttributeInfo("Description", (object file) => ((FarFile)file).Description),
			new XmlAttributeInfo("Owner", (object file) => ((FarFile)file).Owner),
			new XmlAttributeInfo("Length", (object file) => ((FarFile)file).Length),
			new XmlAttributeInfo("CreationTime", (object file) => ((FarFile)file).CreationTime),
			new XmlAttributeInfo("LastAccessTime", (object file) => ((FarFile)file).LastAccessTime),
			new XmlAttributeInfo("LastWriteTime", (object file) => ((FarFile)file).LastWriteTime),
			new XmlAttributeInfo("ReadOnly", (object file) => ((FarFile)file).IsReadOnly),
			new XmlAttributeInfo("Hidden", (object file) => ((FarFile)file).IsHidden),
			new XmlAttributeInfo("System", (object file) => ((FarFile)file).IsSystem),
			new XmlAttributeInfo("Archive", (object file) => ((FarFile)file).IsArchive),
			new XmlAttributeInfo("Compressed", (object file) => ((FarFile)file).IsCompressed),
			new XmlAttributeInfo("ReparsePoint", (object file) => ((FarFile)file).IsReparsePoint),
		};

		_attrs = new ReadOnlyCollection<XmlAttributeInfo>(attrs);
		return _attrs;
	}

	/// <summary>
	/// INTERNAL
	/// </summary>
	public virtual IList<XmlAttributeInfo> XmlAttributes()
	{
		return XmlAttr();
	}
}
