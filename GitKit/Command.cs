using FarNet;
using LibGit2Sharp;
using System;
using System.Data.Common;

namespace GitKit;

[ModuleCommand(Name = "GitKit", Prefix = "gk", Id = "15a36561-bf47-47a5-ae43-9729eda272a3")]
public class Command : ModuleCommand
{
	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		var parameters = Parameters.Parse(e.Command);

		if (parameters.Count == 0)
		{
			Host.Instance.ShowHelpTopic("contents");
			return;
		}

		var path = parameters.GetValue("repo");
		if (path is null)
			path = Far.Api.CurrentDirectory;
		else
			path = Environment.ExpandEnvironmentVariables(path);

		var repo = new Repository(Lib.GetGitRoot(path));

		var panel = parameters.GetValue("panel");
		if (panel is not null)
		{
			switch (panel)
			{
				case "branches":
					parameters.AssertNone();
					new BranchesExplorer(repo).CreatePanel().Open();
					return;

				case "commits":
					parameters.AssertNone();
					new CommitsExplorer(repo, repo.Head).CreatePanel().Open();
					return;

				case "changes":
					parameters.AssertNone();
					TreeChanges changes() => repo.Diff.Compare<TreeChanges>(repo.Head.Tip.Tree, DiffTargets.Index | DiffTargets.WorkingDirectory);
					new ChangesExplorer(repo, changes).CreatePanel().Open();
					return;

				default:
					throw new ModuleException($"Unknown panel `{panel}`.");
			}
		}

		parameters.AssertNone();
	}
}
