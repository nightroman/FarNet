using LibGit2Sharp;
using System.Data.Common;

namespace GitKit;

sealed class PullCommand : BaseCommand
{
	public PullCommand(DbConnectionStringBuilder parameters) : base(parameters)
	{
	}

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
		Commands.Pull(repo, sig, pull);

		Host.UpdatePanels();
	}
}
