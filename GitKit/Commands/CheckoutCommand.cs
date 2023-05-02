using FarNet;
using LibGit2Sharp;
using System.Data.Common;

namespace GitKit;

sealed class CheckoutCommand : BaseCommand
{
	readonly string? _branchName;

	public CheckoutCommand(DbConnectionStringBuilder parameters) : base(parameters)
	{
		_branchName = parameters.GetString(Parameter.Branch);
	}

	public override void Invoke()
	{
		var branchName = _branchName ?? Far.Api.Input(
			"Branch name",
			"GitBranch",
			$"Checkout branch from {Repository.Head.FriendlyName}",
			Repository.Head.FriendlyName);

		if (branchName is null)
			return;

		CheckoutBranch(Repository, branchName);
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
