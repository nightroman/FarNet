using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace GitKit;

sealed class MyRepository : IDisposable
{
	static readonly Dictionary<string, Reference> s_references = new(StringComparer.OrdinalIgnoreCase);
	readonly Reference _reference;
	readonly string _key;

	public Repository Repository => _reference.Instance;

	public MyRepository(string path)
	{
		_key = Path.GetFullPath(Lib.GetGitRoot(path));

		if (s_references.TryGetValue(_key, out Reference? repoCount))
		{
			_reference = repoCount;
			++repoCount.RefCount;
		}
		else
		{
			_reference = new Reference(new Repository(_key));
			s_references.Add(_key, _reference);
		}
	}

	public void Dispose()
	{
		if (--_reference.RefCount == 0)
		{
			s_references.Remove(_key);
			_reference.Instance.Dispose();
		}
	}

	public void AddRef()
	{
		++_reference.RefCount;
	}

	class Reference
	{
		public readonly Repository Instance;
		public int RefCount = 1;

		public Reference(Repository repo)
		{
			Instance = repo;
		}
	}
}
