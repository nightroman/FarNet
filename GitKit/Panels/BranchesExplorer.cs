using FarNet;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GitKit;

class BranchesExplorer : BaseExplorer
{
	public static Guid MyTypeId = new("75a5d4a6-85b7-4bab-974c-f3a3eb21c992");

	public BranchesExplorer(Repository repository) : base(repository, MyTypeId)
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
		if (Repository.Info.IsHeadDetached)
		{
			var branch = Repository.Head;
			yield return new SetFile
			{
				Name = branch.FriendlyName,
				Description = branch.Tip.MessageShort,
				IsDirectory = true,
				Data = branch,
			};
		}

		var branches = Repository.Branches
			.OrderBy(x => x.IsRemote)
			.ThenBy(x => x.FriendlyName);

		foreach (var branch in branches)
		{
			yield return new SetFile
			{
				Name = branch.FriendlyName,
				Description = branch.Tip.MessageShort,
				Owner = GetBranchMarks(branch),
				IsDirectory = true,
				Data = branch,
			};
		}
	}

	public override Explorer? ExploreDirectory(ExploreDirectoryEventArgs args)
	{
		var branch = (Branch)args.File.Data!;
		return new CommitsExplorer(Repository, branch);
	}

	void CloneBranch(ExplorerEventArgs args, bool checkout)
	{
		var (branch, newName) = ((Branch, string))args.Data!;
		try
		{
			var newBranch = Repository.CreateBranch(newName, branch.Tip);

			if (checkout && !Repository.Info.IsBare)
				Commands.Checkout(Repository, newBranch);

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
		var op = new PushOptions
		{
			CredentialsProvider = Host.GetCredentialsHandler()
		};

		var remote = Repository.Network.Remotes[branch.RemoteName];
		Repository.Network.Push(remote, $":{branch.UpstreamBranchCanonicalName}", op);
	}

	public override void DeleteFiles(DeleteFilesEventArgs args)
	{
		foreach (var file in args.Files)
		{
			var branch = (Branch)file.Data!;

			if (!args.Force)
			{
				if (branch.IsRemote)
				{
					CannotDelete(args, file, $"Use [ShiftDel] to delete remote branch '{branch.FriendlyName}'.");
					continue;
				}

				var another = Lib.GetBranchesContainingCommit(Repository, branch.Tip).FirstOrDefault(another => another != branch);
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
					Repository.Branches.Remove(branch);
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
		var branch = (Branch)args.File.Data!;
		var newName = (string)args.Data!;
		Repository.Branches.Rename(branch, newName);
		args.PostName = newName;
	}
}
