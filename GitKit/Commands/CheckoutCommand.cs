using FarNet;
using LibGit2Sharp;
using System.Data.Common;

namespace GitKit;

sealed class CheckoutCommand : BaseCommand
{
	readonly string? _branchName;

	public CheckoutCommand(Repository repo, DbConnectionStringBuilder parameters) : base(repo)
	{
		_branchName = parameters.GetValue("Branch");
	}

	public override void Invoke()
	{
		var branchName = _branchName ?? Far.Api.Input(
			"Branch name",
			"GitBranch",
			$"Checkout branch from {_repo.Head.FriendlyName}",
			_repo.Head.FriendlyName);

		if (branchName is null)
			return;

		CheckoutBranch(_repo, branchName);
	}

	static void CheckoutBranch(Repository repo, string branchName)
	{
		var branch = repo.Branches[branchName];
		if (branch is null)
		{
			if (0 != Far.Api.Message(
				$"Create branch '{branchName}' from '{repo.Head.FriendlyName}'?",
				Host.MyName,
				MessageOptions.YesNo))
				return;

			branch = repo.CreateBranch(branchName, repo.Head.Tip);
		}

		if (!repo.Info.IsBare)
		{
			Commands.Checkout(repo, branch);
			Host.UpdatePanels();
		}
	}
}
