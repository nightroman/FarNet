using FarNet;
using LibGit2Sharp;
using System;
using System.Data.Common;

namespace GitKit;

[ModuleCommand(Name = "GitKit", Prefix = "gk", Id = "15a36561-bf47-47a5-ae43-9729eda272a3")]
public class Command : ModuleCommand
{
	static DbConnectionStringBuilder ParseParameters(string text)
	{
		try
		{
			return new DbConnectionStringBuilder { ConnectionString = text };
		}
		catch (Exception ex)
		{
			throw new ModuleException($"Use semicolon separated key=value pairs. Error: {ex.Message}");
		}
	}

	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		var parameters = ParseParameters(e.Command);

		if (parameters.Count == 0)
		{
			Host.Instance.ShowHelpTopic("contents");
			return;
		}

		string path;
		if (parameters.TryGetValue("repo", out object? value))
			path = Environment.ExpandEnvironmentVariables((string)value);
		else
			path = Far.Api.CurrentDirectory;

		var repo = new Repository(Host.GetGitRoot(path));

		if (parameters.TryGetValue("panel", out value))
		{
			switch (value)
			{
				case "branches":
					new BranchesExplorer(repo).CreatePanel().Open();
					return;

				case "commits":
					new CommitsExplorer(repo, repo.Head).CreatePanel().Open();
					return;

				case "changes":
					TreeChanges changes() => repo.Diff.Compare<TreeChanges>(repo.Head.Tip.Tree, DiffTargets.Index | DiffTargets.WorkingDirectory);
					new ChangesExplorer(repo, changes).CreatePanel().Open();
					return;

				default:
					throw new ModuleException($"Unknown panel `{value}`.");
			}
		}

		throw new ModuleException($"Unknown parameters in {e.Prefix}:{e.Command}");
	}
}
