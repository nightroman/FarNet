using FarNet;
using LibGit2Sharp;

namespace GitKit.Commands;

sealed class CheckoutCommand(CommandParameters parameters) : BaseCommand(parameters)
{
	readonly string? _checkoutBranchName = parameters.GetString(Param.Branch);

	public override void Invoke()
	{
		using var repo = new Repository(GitRoot);

		var checkoutBranchName = _checkoutBranchName ?? Far.Api.Input(
			"Branch name",
			"GitBranch",
			$"Checkout branch from {repo.Head.FriendlyName}",
			repo.Head.FriendlyName);

		if (checkoutBranchName is null)
			return;

		CheckoutBranch(repo, checkoutBranchName);
	}

	static void CheckoutBranch(Repository repo, string chekoutBranchName)
	{
		var branch = repo.Branches[chekoutBranchName];
		if (branch is null)
		{
			if (0 != Far.Api.Message(
				$"Create branch '{chekoutBranchName}' from '{repo.Head.FriendlyName}'?",
				Host.MyName,
				MessageOptions.YesNo))
				return;

			branch = repo.CreateBranch(chekoutBranchName, repo.Head.Tip);
		}

		if (!repo.Info.IsBare)
		{
			LibGit2Sharp.Commands.Checkout(repo, branch);
			Host.UpdatePanels();
		}
	}
}
