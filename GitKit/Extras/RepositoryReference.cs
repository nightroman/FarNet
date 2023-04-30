using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitKit;

/// <summary>
/// Maintains repository instances and reference counting.
/// </summary>
public sealed class RepositoryReference : IDisposable
{
	const string UnknownInstance = "This repository instance is unknown.";
	static readonly LinkedList<RepositoryReference> s_references = new();
	int _refCount = 1;

	/// <summary>
	/// Normalized git directory path used as the key.
	/// </summary>
	public string Directory { get; }

	/// <summary>
	/// The only repository instance.
	/// </summary>
	public Repository Instance { get; }

	/// <summary>
	/// Gets existing or creates new repository reference.
	/// Designed for auto disposal by using.
	/// </summary>
	public static RepositoryReference GetReference(string path)
	{
		path = Path.GetFullPath(Lib.GetGitRoot(path));

		var reference = s_references.FirstOrDefault(x => x.Directory.Equals(path, StringComparison.OrdinalIgnoreCase));
		if (reference is null)
		{
			reference = new RepositoryReference(path);
		}
		else
		{
			++reference._refCount;
		}

		return reference;
	}

	RepositoryReference(string directory)
	{
		Instance = new Repository(directory);
		Directory = directory;

		s_references.AddFirst(this);
	}

	/// <summary>
	/// Designed for auto disposal by using.
	/// Do not dispose more than once.
	/// </summary>
	public void Dispose()
	{
		if (--_refCount == 0)
		{
			s_references.Remove(this);
			Instance.Dispose();
		}
	}

	/// <summary>
	/// Forcedly disposes and removes all repository references.
	/// </summary>
	public static void Clear()
	{
		foreach (var reference in s_references)
			reference.Instance.Dispose();

		s_references.Clear();
	}

	/// <summary>
	/// Gets or creates the only repository instance for the specified path.
	/// The caller must call <see cref="Release"/>.
	/// </summary>
	public static Repository GetRepository(string path)
	{
		return GetReference(path).Instance;
	}

	/// <summary>
	/// Increments reference count of this instance.
	/// The caller must call <see cref="Release"/>.
	/// </summary>
	public static void AddRef(Repository instance)
	{
		var reference = s_references.FirstOrDefault(x => ReferenceEquals(x.Instance, instance)) ?? throw new InvalidOperationException(UnknownInstance);
		++reference._refCount;
	}

	/// <summary>
	/// Decrements reference count of this instance and disposes not referenced.
	/// It must pair with <see cref="GetRepository"/> or <see cref="AddRef"/>.
	/// </summary>
	public static void Release(Repository instance)
	{
		var reference = s_references.FirstOrDefault(x => ReferenceEquals(x.Instance, instance)) ?? throw new InvalidOperationException(UnknownInstance);
		reference.Dispose();
	}
}
