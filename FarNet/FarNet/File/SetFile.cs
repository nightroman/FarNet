
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;
using System.IO;

namespace FarNet;

/// <summary>
/// Straightforward implementation of <see cref="FarFile"/> ready to use by module panels.
/// </summary>
/// <remarks>
/// It is just a set of properties where any property can be set. In most
/// cases panels may use this class for their items. In some cases they may
/// implement custom classes derived from <see cref="FarFile"/> in order to
/// represent data more effectively (using less memory or working faster).
/// </remarks>
public sealed class SetFile : FarFile
{
	/// <summary>
	/// Creates an empty file data object.
	/// </summary>
	public SetFile()
	{
	}

	/// <summary>
	/// Creates file data snapshot of <see cref="FarFile"/>.
	/// </summary>
	/// <param name="file">Any panel file.</param>
	public SetFile(FarFile file)
	{
		ArgumentNullException.ThrowIfNull(file);

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
	/// Creates file data snapshot of <see cref="FileSystemInfo"/>.
	/// </summary>
	/// <param name="info">File system item info (<see cref="FileInfo"/> or <see cref="DirectoryInfo"/>).</param>
	/// <param name="fullName">Use the full name (path) as the panel file name.</param>
	public SetFile(FileSystemInfo info, bool fullName)
	{
		ArgumentNullException.ThrowIfNull(info);

		Name = fullName ? info.FullName : info.Name;
		CreationTime = info.CreationTime;
		LastAccessTime = info.LastAccessTime;
		LastWriteTime = info.LastWriteTime;
		Attributes = info.Attributes;

		if ((Attributes & FileAttributes.Directory) == 0)
			Length = ((FileInfo)info).Length;
	}

	/// <inheritdoc/>
	public override string Name
	{
		get => _name ?? string.Empty;
		set => _name = value;
	}
	string? _name;

	/// <inheritdoc/>
	public override string? Description { get; set; }

	/// <inheritdoc/>
	public override string? Owner { get; set; }

	/// <inheritdoc/>
	public override DateTime CreationTime { get; set; }

	/// <inheritdoc/>
	public override DateTime LastAccessTime { get; set; }

	/// <inheritdoc/>
	public override DateTime LastWriteTime { get; set; }

	/// <inheritdoc/>
	public override long Length { get; set; }

	/// <inheritdoc/>
	public override object? Data { get; set; }

	/// <inheritdoc/>
	public override FileAttributes Attributes { get; set; }

	/// <inheritdoc/>
	public override ICollection? Columns { get; set; }
}
