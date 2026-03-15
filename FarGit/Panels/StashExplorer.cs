using FarNet;
using LibGit2Sharp;

namespace FarGit.Panels;

/// <summary>
/// Explorer for <see cref="StashPanel"/>. Lists all stash entries.
/// </summary>
public class StashExplorer(string gitDir)
	: BaseExplorer(gitDir, new Guid("c2d3e4f5-a6b7-4c8d-9e0f-1a2b3c4d5e6f"))
{
	public override Panel CreatePanel() => new StashPanel(this);

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		using var repo = new Repository(GitDir);

		int index = 0;
		foreach (var stash in repo.Stashes)
		{
			yield return new StashFile(index, stash);
			index++;
		}

		if (args.Panel is StashPanel panel && panel.Title is null)
		{
			panel.Title = $"Git Stash — {repo.Info.WorkingDirectory}";
			panel.CurrentLocation = repo.Info.WorkingDirectory ?? GitDir;
		}
	}
}
