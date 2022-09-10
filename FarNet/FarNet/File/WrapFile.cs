
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;
using System.IO;

namespace FarNet;

/// <summary>
/// The base class for a file which wraps another file.
/// </summary>
public class WrapFile : FarFile
{
	/// <summary>
	/// New file which wraps another file.
	/// </summary>
	/// <param name="file">The base file.</param>
	public WrapFile(FarFile file)
	{
		File = file ?? throw new ArgumentNullException("file");
	}

	/// <summary>
	/// Gets the base file.
	/// </summary>
	public FarFile File { get; }

	/// <inheritdoc/>
	public override string Name => File.Name;

	/// <inheritdoc/>
	public override string? Description => File.Description;

	/// <inheritdoc/>
	public override string? Owner => File.Owner;

	/// <inheritdoc/>
	public override object? Data => File.Data;

	/// <inheritdoc/>
	public override DateTime CreationTime => File.CreationTime;

	/// <inheritdoc/>
	public override DateTime LastAccessTime => File.LastAccessTime;

	/// <inheritdoc/>
	public override DateTime LastWriteTime => File.LastWriteTime;

	/// <inheritdoc/>
	public override long Length => File.Length;

	/// <inheritdoc/>
	public override ICollection? Columns => File.Columns;

	/// <inheritdoc/>
	public override FileAttributes Attributes => File.Attributes;
}
