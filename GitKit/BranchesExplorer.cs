using FarNet;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GitKit;

class BranchesExplorer : BaseExplorer
{
	const string MarkCurrentBranch = "*";
	const string MarkRemoteBranch = "r";

	public static Guid MyTypeId = new("75a5d4a6-85b7-4bab-974c-f3a3eb21c992");

	public BranchesExplorer(MyRepository repository) : base(repository, MyTypeId)
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

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		return Repository.Branches
			.OrderBy(x => x.IsRemote)
			.ThenBy(x => x.FriendlyName)
			.Select(x => new SetFile
			{
				Name = x.FriendlyName,
				Description = x.Tip.MessageShort,
				Owner = x.IsCurrentRepositoryHead ? MarkCurrentBranch : x.IsRemote ? MarkRemoteBranch : null,
				IsDirectory = true,
				Data = x,
			});
	}

	public override Explorer? ExploreDirectory(ExploreDirectoryEventArgs args)
	{
		var branch = (Branch)args.File.Data!;
		return new CommitsExplorer(MyRepository, branch);
	}

	void CloneBranch(ExplorerEventArgs args, bool checkout)
	{
		var (branch, newName) = ((Branch, string))args.Data!;
		try
		{
			var newBranch = Repository.CreateBranch(newName, branch.Tip);
			if (checkout)
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
		Host.InvokeGit(
			$"push {branch.RemoteName} --delete {branch.UpstreamBranchCanonicalName}",
			Repository.Info.WorkingDirectory);
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
					CannotDelete(args, file, $"Use [ShiftDel] to delete branch '{branch.FriendlyName}', it has unique commits.");
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
