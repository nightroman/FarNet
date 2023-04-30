using LibGit2Sharp;
using System.Data.Common;

namespace GitKit;

abstract class BaseCommand : AnyCommand
{
	readonly RepositoryReference _reference;
	protected Repository Repository { get; }

	protected BaseCommand(DbConnectionStringBuilder parameters)
	{
		_reference = RepositoryReference.GetReference(Host.GetFullPath(parameters.GetValue("repo")));
		Repository = _reference.Instance;
	}

	protected BaseCommand(string path)
	{
		_reference = RepositoryReference.GetReference(path);
		Repository = _reference.Instance;
	}

	protected override void Dispose(bool disposing)
	{
		_reference.Dispose();
	}
}
