
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;
using System.IO;

namespace FarNet.Tools;

/// <summary>
/// The file system explorer for FarNet.Explore, FarNet.PowerShellFar Search-FarFile, etc.
/// </summary>
public class FileSystemExplorer : Explorer
{
	/// <summary>
	/// Creates the explorer with the specified or current location.
	/// </summary>
	/// <param name="location">The explorer location. If it is omitted or empty, the current location is used.</param>
	public FileSystemExplorer(string? location = null) : base(new Guid("bc42b2b2-b735-498b-983f-646d1fd38d10"))
	{
		Location = string.IsNullOrEmpty(location) ? Far.Api.CurrentDirectory : location;
		CanExploreLocation = true;
	}

	/// <inheritdoc/>
	public override Panel CreatePanel()
	{
		Panel panel = new(this)
		{
			RealNames = true,
			RealNamesDeleteFiles = true,
			RealNamesExportFiles = true,
			RealNamesImportFiles = true,
			RealNamesMakeDirectory = true,
			Highlighting = PanelHighlighting.Full,
			DotsMode = PanelDotsMode.Dots,
		};
		return panel;
	}

	/// <inheritdoc/>
	public override void EnterPanel(Panel panel)
	{
		panel.CurrentLocation = Location;
		panel.Title = $"({Location})";
	}

	//! SCHB v2024.12.15.0 is confused by inheritdoc
	/// <summary>
	/// Returns the files.
	/// </summary>
	/// <param name="args">.</param>
	public override IList<FarFile> GetFiles(GetFilesEventArgs args)
	{
		string[] directories;
		try
		{
			directories = Directory.GetDirectories(Location);
		}
		catch
		{
			directories = [];
		}

		string[] files;
		try
		{
			files = Directory.GetFiles(Location);
		}
		catch
		{
			files = [];
		}

		List<FarFile> result = new(directories.Length + files.Length);

		try
		{
			foreach (var dir in directories)
			{
				DirectoryInfo f = new(dir);
				result.Add(new SetFile
				{
					Name = f.Name,
					Attributes = f.Attributes,
					CreationTime = f.CreationTime,
					LastWriteTime = f.LastWriteTime,
				});
			}
		}
		catch { }

		try
		{
			foreach (var file in files)
			{
				FileInfo f = new(file);
				result.Add(new SetFile
				{
					Name = f.Name,
					Attributes = f.Attributes,
					CreationTime = f.CreationTime,
					LastWriteTime = f.LastWriteTime,
					Length = f.Length,
				});
			}
		}
		catch { }

		return result;
	}

	/// <inheritdoc/>
	public override Explorer? ExploreLocation(ExploreLocationEventArgs args)
	{
		if (Path.IsPathRooted(args.Location))
			return new FileSystemExplorer(args.Location);
		else
			return new FileSystemExplorer(Path.Combine(Location, args.Location));
	}

	/// <inheritdoc/>
	public override Explorer? ExploreDirectory(ExploreDirectoryEventArgs args)
	{
		return new FileSystemExplorer(Path.Combine(Location, args.File.Name));
	}

	/// <inheritdoc/>
	public override Explorer? ExploreParent(ExploreParentEventArgs args)
	{
		var path = Path.GetDirectoryName(Location);
		return string.IsNullOrEmpty(path) ? null : new FileSystemExplorer(path);
	}

	/// <inheritdoc/>
	public override Explorer? ExploreRoot(ExploreRootEventArgs args)
	{
		var root = Path.GetPathRoot(Location);
		return root is null ? null : new FileSystemExplorer(root);
	}
}
