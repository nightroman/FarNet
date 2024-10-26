using GitKit.Extras;
using LibGit2Sharp;
using System.Data.Common;

namespace GitKit.Commands;

sealed class PullCommand(DbConnectionStringBuilder parameters) : BaseCommand(parameters)
{
	public override void Invoke()
	{
		Pull(Repository);
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
