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

	public override void CreateFile(CreateFileEventArgs args)
	{
		try
		{
			var newName = (string)args.Data!;
			var branch = Repository.CreateBranch(newName);
			Commands.Checkout(Repository, branch);
			args.PostName = newName;
		}
		catch (LibGit2SharpException ex)
		{
			throw new ModuleException(ex.Message);
		}
	}

	public override void DeleteFiles(DeleteFilesEventArgs args)
	{
		foreach (var file in args.Files)
		{
			var branch = (Branch)file.Data!;
			try
			{
				Repository.Branches.Remove(branch);
			}
			catch(Exception ex)
			{
				args.Result = JobResult.Incomplete;
				args.FilesToStay.Add(file);
				if (0 == (args.Mode & ExplorerModes.Silent))
					Far.Api.Message(ex.Message, "Error", MessageOptions.LeftAligned | MessageOptions.Warning);
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
