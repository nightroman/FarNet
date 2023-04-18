using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitKit;

/// <summary>
/// Creates and disposes repository instances.
/// </summary>
public static class RepositoryFactory
{
	static readonly LinkedList<Reference> s_references = new();

	class Reference
	{
		public readonly Repository Instance;
		public readonly string Path;
		public int RefCount = 1;

		public Reference(string path)
		{
			Instance = new Repository(path);
			Path = path;
		}
	}

	/// <summary>
	/// Gets or creates the only repository instance for the specified path.
	/// The caller must call <see cref="Release"/>.
	/// </summary>
	public static Repository Instance(string path)
	{
		path = Path.GetFullPath(Lib.GetGitRoot(path));

		var reference = s_references.FirstOrDefault(x => x.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
		if (reference is null)
		{
			reference = new Reference(path);
			s_references.AddFirst(reference);
		}
		else
		{
			++reference.RefCount;
		}

		return reference.Instance;
	}

	/// <summary>
	/// Increments the reference count of this instance.
	/// The caller must call <see cref="Release"/>.
	/// </summary>
	public static void AddRef(this Repository instance)
	{
		var reference = s_references.FirstOrDefault(x => ReferenceEquals(x.Instance, instance)) ?? throw new InvalidOperationException();
		++reference.RefCount;
	}

	/// <summary>
	/// Decrements the reference count of this instance and disposes no longer referenced.
	/// It is called after <see cref="Instance"/> or <see cref="AddRef"/>.
	/// </summary>
	public static void Release(this Repository instance)
	{
		var reference = s_references.FirstOrDefault(x => ReferenceEquals(x.Instance, instance)) ?? throw new InvalidOperationException();
		if (--reference.RefCount == 0)
		{
			s_references.Remove(reference);
			reference.Instance.Dispose();
		}
	}

	/// <summary>
	/// Forcedly disposes and removes all repository references.
	/// </summary>
	public static void Clear()
	{
		foreach(var reference in s_references)
			reference.Instance.Dispose();

		s_references.Clear();
	}
}
