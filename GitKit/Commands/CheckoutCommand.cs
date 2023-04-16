using FarNet;
using LibGit2Sharp;

namespace GitKit;

sealed class CheckoutCommand : BaseCommand
{
	readonly string _branchName;

	public CheckoutCommand(Repository repo, string value) : base(repo)
	{
		_branchName = value;
	}

	public override void Invoke()
	{
		var branch = _repo.Branches[_branchName];
		if (branch is null)
		{
			if (0 != Far.Api.Message(
				$"Create branch '{_branchName}' from '{_repo.Head.FriendlyName}'?",
				Host.MyName,
				MessageOptions.YesNo))
				return;

			branch = _repo.CreateBranch(_branchName, _repo.Head.Tip);
		}

		Commands.Checkout(_repo, branch);
	}
}
