
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;

namespace FarNet.Tools;

/// <summary>
/// Super explorer/panel file.
/// </summary>
/// <param name="explorer">The file's explorer.</param>
/// <param name="file">The base file.</param>
public class SuperFile(Explorer explorer, FarFile file) : WrapFile(file)
{
	/// <summary>
	/// Gets the source explorer.
	/// </summary>
	public Explorer Explorer => _Explorer;

	readonly Explorer _Explorer = explorer ?? throw new ArgumentNullException(nameof(explorer));

	/// <summary>
	/// Gets the source explorer location.
	/// </summary>
	public override string Owner => Explorer.Location;

	internal static Dictionary<FarFile, SuperFile> HashSuperFiles(IEnumerable<SuperFile> files, IEqualityComparer<FarFile> comparer)
	{
		var hash = new Dictionary<FarFile, SuperFile>(comparer);
		foreach (var file in files)
		{
			try { hash.Add(file.File, file); }
			catch (ArgumentException) { }
		}
		return hash;
	}

	internal static IEnumerable<SuperFile> SuperFilesOfExplorerFiles(IEnumerable<SuperFile> xfiles, IList<FarFile> efiles, IEqualityComparer<FarFile> comparer)
	{
		if (efiles.Count > 0)
		{
			var xhash = HashSuperFiles(xfiles, comparer);
			foreach (var file in efiles)
			{
				//! try: if a module incorrectly gets alien files to stay then they are not hashed
				if (xhash.TryGetValue(file, out SuperFile? xfile))
					yield return xfile;
			}
		}
	}
}
