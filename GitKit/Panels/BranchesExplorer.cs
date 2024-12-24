using FarNet;
using GitKit.About;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GitKit.Panels;

class BranchesExplorer : BaseExplorer
{
	public static Guid MyTypeId = new("75a5d4a6-85b7-4bab-974c-f3a3eb21c992");

	public BranchesExplorer(string gitDir) : base(gitDir, MyTypeId)
	{
		CanCloneFile = true;
		CanCreateFile = true;
		CanDeleteFiles = true;
		CanRenameFile = true;
	}

	public override Panel CreatePanel()
	{
		return new BranchesPanel(this);
	}

	public override void EnterPanel(Panel panel)
	{
		using var repo = new Repository(GitDir);

		panel.PostName(repo.Head?.FriendlyName);
	}

	static char GetTipsMark(Commit tip1, Commit tip2)
	{
		if (tip1 is null || tip2 is null)
			return '?';

		if (tip1 == tip2)
			return '=';

		return tip1.Author.When < tip2.Author.When ? '<' : '>';
	}

	static string GetBranchMarks(Branch branch)
	{
		if (branch.IsRemote)
			return $"r ";

		var tracked = branch.TrackedBranch;
		char m1 = tracked is null ? ' ' : GetTipsMark(branch.Tip, tracked.Tip);
		char m2 = branch.IsCurrentRepositoryHead ? '*' : ' ';
		return $"{m1}{m2}";
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		using var repo = new Repository(GitDir);

		if (repo.Info.IsHeadDetached)
		{
			var branch = repo.Head;
			yield return new BranchFile(branch.FriendlyName, branch.Tip.MessageShort);
		}

		var branches = repo.Branches
			.OrderBy(x => x.IsRemote)
			.ThenBy(x => x.FriendlyName);

		foreach (var branch in branches)
		{
			yield return new BranchFile(branch.FriendlyName, branch.Tip.MessageShort, owner: GetBranchMarks(branch));
		}
	}

	public override Explorer? ExploreDirectory(ExploreDirectoryEventArgs args)
	{
		var branchName = args.File.Name;
		return new CommitsExplorer(GitDir, branchName, false);
	}

	void CloneBranch(ExplorerEventArgs args, bool checkout)
	{
		using var repo = new Repository(GitDir);

		var (branchName, newName) = ((string, string))args.Data!;
		var branch = repo.MyBranch(branchName);
		try
		{
			var newBranch = repo.CreateBranch(newName, branch.Tip);

			if (checkout && !repo.Info.IsBare)
				LibGit2Sharp.Commands.Checkout(repo, newBranch);

			args.PostName = newName;
		}
		catch (LibGit2SharpException ex)
		{
			throw new ModuleException(ex.Message);
		}
	}

	public override void CloneFile(CloneFileEventArgs args)
	{
		CloneBranch(args, false);
	}

	public override void CreateFile(CreateFileEventArgs args)
	{
		CloneBranch(args, true);
	}

	static void CannotDelete(DeleteFilesEventArgs args, FarFile file, string message)
	{
		args.Result = JobResult.Incomplete;
		args.FilesToStay.Add(file);
		if (0 == (args.Mode & ExplorerModes.Silent))
			Far.Api.Message(message, Host.MyName, MessageOptions.LeftAligned | MessageOptions.Warning);
	}

	void DeleteRemoteBranch(Branch branch)
	{
		using var repo = new Repository(GitDir);

		var op = new PushOptions
		{
			CredentialsProvider = Host.GetCredentialsHandler()
		};

		var remote = repo.Network.Remotes[branch.RemoteName];
		repo.Network.Push(remote, $":{branch.UpstreamBranchCanonicalName}", op);
	}

	public override void DeleteFiles(DeleteFilesEventArgs args)
	{
		using var repo = new Repository(GitDir);

		foreach (var file in args.Files)
		{
			var branchName = file.Name;
			var branch = repo.MyBranch(branchName);
			if (branch.IsCurrentRepositoryHead)
			{
				args.Result = JobResult.Incomplete;
				continue;
			}

			if (!args.Force)
			{
				if (branch.IsRemote)
				{
					CannotDelete(args, file, $"Use [ShiftDel] to delete remote branch '{branch.FriendlyName}'.");
					continue;
				}

				var another = Lib.GetBranchesContainingCommit(repo, branch.Tip).FirstOrDefault(another => another != branch);
				if (another is null)
				{
					CannotDelete(args, file, $"Use [ShiftDel] to delete branch '{branch.FriendlyName}' with unique commits.");
					continue;
				}
			}

			try
			{
				if (branch.IsRemote)
				{
					DeleteRemoteBranch(branch);
				}
				else
				{
					repo.Branches.Remove(branch);
				}
			}
			catch (Exception ex)
			{
				CannotDelete(args, file, $"Cannot delete branch '{branch.FriendlyName}': {ex.Message}");
			}
		}
	}

	public override void RenameFile(RenameFileEventArgs args)
	{
		using var repo = new Repository(GitDir);

		var branchName = args.File.Name;
		var newName = (string)args.Data!;
		repo.Branches.Rename(branchName, newName);
		args.PostName = newName;
	}
}
