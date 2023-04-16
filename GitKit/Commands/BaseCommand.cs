using LibGit2Sharp;

namespace GitKit;

abstract class BaseCommand : AnyCommand
{
	protected readonly Repository _repo;

	public BaseCommand(Repository repo)
	{
		_repo = repo;
	}
}
