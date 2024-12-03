using FarNet;
using GitKit.Extras;
using LibGit2Sharp;
using System.Data.Common;

namespace GitKit.Commands;

sealed class PushCommand(DbConnectionStringBuilder parameters) : BaseCommand(parameters)
{
	public override void Invoke()
	{
		PushBranch(Repository, Repository.Head);
	}

	public static void PushBranch(Repository repo, Branch branch)
	{
		if (branch.IsRemote)
			throw new ModuleException("Cannot push remote branch.");

		var changes = Lib.GetChanges(repo);
		if (changes.Count > 0)
			throw new ModuleException($"Cannot push: {changes.Count} not committed changes.");

		if (0 != Far.Api.Message(
			$"Push branch '{branch.FriendlyName}'?",
			Host.MyName,
			MessageOptions.YesNo))
			return;

		var op = new PushOptions
		{
			CredentialsProvider = Host.GetCredentialsHandler()
		};

		if (branch.TrackedBranch is null)
		{
			var menu = Far.Api.CreateListMenu();
			menu.Title = "Select remote";
			menu.UsualMargins = true;
			foreach (var it in repo.Network.Remotes)
				menu.Add(it.Name).Data = it;

			if (!menu.Show() || menu.SelectedData is not Remote remote)
				return;

			branch = repo.Branches.Update(
				branch,
				b => b.Remote = remote.Name,
				b => b.UpstreamBranch = branch.CanonicalName);
		}

		repo.Network.Push(branch, op);

		Host.UpdatePanels();
	}
}
