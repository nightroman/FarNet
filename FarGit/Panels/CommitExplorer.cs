using FarNet;
using LibGit2Sharp;

namespace FarGit.Panels;

/// <summary>
/// Explorer for <see cref="CommitPanel"/>.
/// Shows the commit timeline for the current branch, with branch/tag labels on each commit.
/// </summary>
public class CommitExplorer(string gitDir)
	: BaseExplorer(gitDir, new Guid("d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a"))
{
	const int MaxCommits = 500;

	public override Panel CreatePanel() => new CommitPanel(this);

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		using var repo = new Repository(GitDir);

		if (repo.Head.Tip is null)
		{
			yield return new SetFile { Name = "  (no commits yet)", Attributes = FileAttributes.ReadOnly };
			yield break;
		}

		// Build a map: commit sha → label strings (branches, tags, HEAD)
		var labels = BuildLabelMap(repo);

		int count = 0;
		foreach (var commit in repo.Head.Commits)
		{
			if (count++ >= MaxCommits) break;
			labels.TryGetValue(commit.Sha, out var label);
			yield return new CommitFile(commit, label ?? string.Empty);
		}

		if (args.Panel is CommitPanel panel && panel.Title is null)
		{
			panel.Title = $"History  [{repo.Head.FriendlyName}]  {repo.Info.WorkingDirectory}";
			panel.CurrentLocation = repo.Info.WorkingDirectory ?? GitDir;
		}
	}

	/// <summary>
	/// Builds commit-sha → label string map for all refs (HEAD, branches, tags).
	/// Example label: "HEAD → main, origin/main, v1.0"
	/// </summary>
	static Dictionary<string, string> BuildLabelMap(Repository repo)
	{
		var map = new Dictionary<string, List<string>>();

		void Add(string sha, string label)
		{
			if (!map.TryGetValue(sha, out var list))
				map[sha] = list = [];
			list.Add(label);
		}

		// HEAD pointer
		var headTip = repo.Head.Tip;
		if (headTip is not null)
			Add(headTip.Sha, $"HEAD → {repo.Head.FriendlyName}");

		// All branches (local + remote), skip the HEAD branch (already added)
		foreach (var b in repo.Branches)
		{
			if (b.Tip is null) continue;
			if (b.IsCurrentRepositoryHead) continue; // HEAD already labeled
			Add(b.Tip.Sha, b.FriendlyName);
		}

		// All tags
		foreach (var t in repo.Tags)
		{
			var sha = (t.PeeledTarget as Commit)?.Sha ?? (t.Target as Commit)?.Sha;
			if (sha is not null)
				Add(sha, t.FriendlyName);
		}

		return map.ToDictionary(kv => kv.Key, kv => string.Join(", ", kv.Value));
	}
}
