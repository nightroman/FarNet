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

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		var isHeadDetached = Repository.Info.IsHeadDetached;

		return Repository.Branches
			.Where(x => isHeadDetached || x.FriendlyName != "origin/HEAD")
			.OrderBy(x => x.IsRemote)
			.ThenBy(x => x.FriendlyName)
			.Select(x => new SetFile
			{
				Name = x.FriendlyName,
				Description = x.Tip.MessageShort,
				Owner = x.IsCurrentRepositoryHead ? "*" : null,
				IsDirectory = true,
				Data = x,
			});
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
			Far.Api.Message(message, "GitKit: cannot delete", MessageOptions.LeftAligned | MessageOptions.Warning);
	}

	public override void DeleteFiles(DeleteFilesEventArgs args)
	{
		foreach (var file in args.Files)
		{
			var branch = (Branch)file.Data!;

			if (branch.IsRemote)
			{
				CannotDelete(args, file, $"Remote branch '{branch.FriendlyName}' is not yet supported.");
				continue;
			}

			if (!args.Force)
			{
				var another = Lib.GetBranchesContainingCommit(Repository, branch.Tip).FirstOrDefault(another => another != branch);
				if (another is null)
				{
					CannotDelete(args, file, $"Use [ShiftDel] to delete branch '{branch.FriendlyName}' and its unique local commit(s).");
					continue;
				}
			}

			try
			{
				Repository.Branches.Remove(branch);
			}
			catch (Exception ex)
			{
				CannotDelete(args, file, ex.Message);
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
