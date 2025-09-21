using FarNet;
using LibGit2Sharp;

namespace GitKit.Commands;

sealed class PullCommand(CommandParameters parameters) : BaseCommand(parameters)
{
	public override void Invoke()
	{
		using var repo = new Repository(GitDir);

		Pull(repo);
	}

	public static void Pull(Repository repo)
	{
		var fetch = new FetchOptions
		{
			CredentialsProvider = Host.GetCredentialsHandler()
		};

		var pull = new PullOptions
		{
			FetchOptions = fetch,
		};

		var sig = Lib.BuildSignature(repo);
		LibGit2Sharp.Commands.Pull(repo, sig, pull);

		Host.UpdatePanels();
	}
}
