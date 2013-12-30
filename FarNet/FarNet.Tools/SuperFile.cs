
/*
FarNet.Tools library for FarNet
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using System.Collections.Generic;

namespace FarNet.Tools
{
	/// <summary>
	/// Super explorer/panel file.
	/// </summary>
	public class SuperFile : WrapFile
	{
		/// <summary>
		/// New super file with its explorer.
		/// </summary>
		/// <param name="explorer">The file's explorer.</param>
		/// <param name="file">The base file.</param>
		public SuperFile(Explorer explorer, FarFile file)
			: base(file)
		{
			if (explorer == null) throw new ArgumentNullException("explorer");
			_Explorer = explorer;
		}
		/// <summary>
		/// Gets the source explorer.
		/// </summary>
		public Explorer Explorer { get { return _Explorer; } }
		readonly Explorer _Explorer;
		/// <summary>
		/// Gets the source explorer location.
		/// </summary>
		public override string Owner { get { return Explorer.Location; } }
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
		internal static IEnumerable<FarFile> SuperFilesOfExplorerFiles(IEnumerable<SuperFile> xfiles, IList<FarFile> efiles, IEqualityComparer<FarFile> comparer)
		{
			if (efiles.Count > 0)
			{
				var xhash = HashSuperFiles(xfiles, comparer);
				foreach (var file in efiles)
				{
					//! try: if a module incorrectly gets alien files to stay then they are not hashed
					SuperFile xfile;
					if (xhash.TryGetValue(file, out xfile))
						yield return xfile;
				}
			}
		}
	}
}
