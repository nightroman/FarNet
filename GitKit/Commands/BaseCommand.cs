using LibGit2Sharp;

namespace GitKit;

abstract class BaseCommand : AnyCommand
{
	protected Repository Repository { get; }

	public BaseCommand(Repository repo)
	{
		Repository = repo;
	}
}
