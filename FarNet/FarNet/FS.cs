
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.IO;

namespace FarNet;

// _221023_0659:
// Avoid `Path.Combine` traps: mind `\` and `/` and skip white space names.
// Otherwise the result path if either an item from root or the parent folder.

/// <summary>
/// <b>EXPERIMENTAL</b>
/// File system directory and file user context.
/// </summary>
/// <remarks>
/// <para>
/// This class simplifies getting current and selected file system paths in Far
/// Manager as input for module and script operations. This task is not that
/// simple: panels may be null (editor or viewer mode), panels may have no
/// files (quick view, info), panel items may define not existing paths.
/// </para>
/// <para>
/// "Path" methods return fully qualified file system paths, existing or not.
/// </para>
/// <para>
/// If a panel file name is a fully qualified path then it is used as the path,
/// the panel path is ignored.
/// </para>
/// <para>
/// If a panel file name is not a fully qualified path then the result path
/// is either combined with the fully qualified panel path or it is null.
/// See <see cref="IsExcludedPath"/> for details.
/// </para>
/// <para>
/// "Item", "File", and "Directory" methods return existing file system items.
/// They are created using "Path" methods, so <c>ToString()</c> returns paths.
/// These items may be safely used as paths where conversion to string works.
/// </para>
/// </remarks>
public class FSContext
{
	//_221023_0659
	/// <summary>
	/// Gets true if the path is excluded as ambiguous.
	/// </summary>
	/// <param name="path">The path to be checked.</param>
	/// <returns>True if the path is excluded.</returns>
	/// <remarks>
	/// <para>
	/// Excluded: null, empty, white space, .., and paths like \*, /*
	/// </para>
	/// <para>
	/// .. is excluded as ambiguous: it means the current panel path
	/// in Far Manager but when it is combined with the current path
	/// it gets the parent of the current, not the current.
	/// </para>
	/// <para>
	/// Paths starting with slashes are excluded as ambiguous. Native panels do
	/// not use such paths. Module panels should either use well defined paths
	/// or fully qualified paths.
	/// </para>
	/// </remarks>
	public static bool IsExcludedPath(string? path)
	{
		if (string.IsNullOrWhiteSpace(path))
			return true;

		if (path == "..")
			return true;

		if (path.StartsWith('\\') || path.StartsWith('/'))
			return true;

		return false;
	}

	/// <summary>
	/// Gets the cursor full path, existing or not, or null.
	/// </summary>
	public virtual string? CursorPath => null;

	/// <summary>
	/// Gets the selected paths, existing or not.
	/// </summary>
	/// <returns>The selected paths.</returns>
	public virtual string[] GetSelectedPaths()
	{
		var path = CursorPath;
		return path is null ? Array.Empty<string>() : new string[] { path };
	}

	/// <summary>
	/// Gets the cursor existing directory or null.
	/// </summary>
	public DirectoryInfo? CursorDirectory
	{
		get
		{
			var path = CursorPath;
			if (path is null)
				return null;

			try
			{
				var res = new DirectoryInfo(path);
				return res.Exists ? res : null;
			}
			catch
			{
				return null;
			}
		}
	}

	/// <summary>
	/// Gets the cursor existing file or null.
	/// </summary>
	public FileInfo? CursorFile
	{
		get
		{
			var path = CursorPath;
			if (path is null)
				return null;

			try
			{
				var res = new FileInfo(path);
				return res.Exists ? res : null;
			}
			catch
			{
				return null;
			}
		}
	}

	/// <summary>
	/// Gets the cursor existing directory or file or null.
	/// </summary>
	public FileSystemInfo? CursorItem
	{
		get
		{
			var path = CursorPath;
			if (path is null)
				return null;

			try
			{
				if (File.Exists(path))
					return new FileInfo(path);

				if (Directory.Exists(path))
					return new DirectoryInfo(path);

				return null;
			}
			catch
			{
				return null;
			}
		}
	}

	/// <summary>
	/// Gets the selected existing directories.
	/// </summary>
	/// <returns>The existing directories.</returns>
	public DirectoryInfo[] GetSelectedDirectories()
	{
		var paths = GetSelectedPaths();
		var res = new List<DirectoryInfo>(paths.Length);

		foreach (var path in paths)
		{
			try
			{
				if (Directory.Exists(path))
					res.Add(new DirectoryInfo(path));
			}
			catch
			{
			}
		}

		return res.ToArray();
	}

	/// <summary>
	/// Gets the selected existing files.
	/// </summary>
	/// <returns>The existing files.</returns>
	public FileInfo[] GetSelectedFiles()
	{
		var paths = GetSelectedPaths();
		var res = new List<FileInfo>(paths.Length);

		foreach (var path in paths)
		{
			try
			{
				if (File.Exists(path))
					res.Add(new FileInfo(path));
			}
			catch
			{
			}
		}

		return res.ToArray();
	}

	/// <summary>
	/// Gets the selected existing directory and file items.
	/// </summary>
	/// <returns>The existing directory and file items.</returns>
	public FileSystemInfo[] GetSelectedItems()
	{
		var paths = GetSelectedPaths();
		var res = new List<FileSystemInfo>(paths.Length);

		foreach (var path in paths)
		{
			try
			{
				if (File.Exists(path))
					res.Add(new FileInfo(path));
				else if (Directory.Exists(path))
					res.Add(new DirectoryInfo(path));
			}
			catch
			{
			}
		}

		return res.ToArray();
	}
}

/// <summary>
/// <b>EXPERIMENTAL</b>
/// See <see cref="FSContext"/>.
/// </summary>
public class FSContextSingle : FSContext
{
	readonly string? _path;

	/// <summary>
	/// Instance with the specified item path.
	/// </summary>
	/// <param name="path">The path.</param>
	public FSContextSingle(string? path)
	{
		_path = path;
	}

	/// <inheritdoc/>
	public override string? CursorPath
	{
		get
		{
			try
			{
				return !string.IsNullOrWhiteSpace(_path) && Path.IsPathFullyQualified(_path) ? _path : null;
			}
			catch
			{
				return null;
			}
		}
	}
}

/// <summary>
/// <b>EXPERIMENTAL</b>
/// See <see cref="FSContext"/>.
/// </summary>
public class FSContextPanel : FSContext
{
	readonly IPanel? _panel;

	/// <summary>
	/// Instance with the specified panel.
	/// </summary>
	/// <param name="panel">The panel.</param>
	public FSContextPanel(IPanel? panel)
	{
		_panel = panel;
	}

	/// <inheritdoc/>
	public override string? CursorPath
	{
		get
		{
			if (_panel is null || _panel.Kind != PanelKind.File)
				return null;

			var file = _panel.CurrentFile;
			if (file is null)
				return null;

			if (string.IsNullOrWhiteSpace(file.Name))
				return null;

			try
			{
				if (Path.IsPathFullyQualified(file.Name))
					return file.Name;

				var path = _panel.CurrentDirectory;
				if (Path.IsPathFullyQualified(path) && !IsExcludedPath(file.Name))
					return Path.Combine(path, file.Name);

				return null;
			}
			catch
			{
				return null;
			}
		}
	}

	/// <inheritdoc/>
	public override string[] GetSelectedPaths()
	{
		if (_panel is null || _panel.Kind != PanelKind.File)
			return Array.Empty<string>();

		var files = _panel.GetSelectedFiles();
		if (files.Length == 0)
			return Array.Empty<string>();

		var res = new List<string>(files.Length);
		var location = new Lazy<string?>(() =>
		{
			var location = _panel.CurrentDirectory;
			return string.IsNullOrWhiteSpace(location) || !Path.IsPathFullyQualified(location) ? null : location;
		});

		foreach (var file in files)
		{
			if (string.IsNullOrWhiteSpace(file.Name))
				continue;

			try
			{
				if (Path.IsPathFullyQualified(file.Name))
				{
					res.Add(file.Name);
					continue;
				}

				var path = location.Value;
				if (path is null)
					continue;

				if (IsExcludedPath(file.Name))
					continue;

				res.Add(Path.Combine(path, file.Name));
			}
			catch
			{
			}
		}

		return res.ToArray();
	}
}
