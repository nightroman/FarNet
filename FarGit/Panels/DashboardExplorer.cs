using FarNet;
using LibGit2Sharp;

namespace FarGit.Panels;

/// <summary>
/// Explorer for <see cref="DashboardPanel"/>.
/// Shows a repo-at-a-glance summary: branch, branches, status, stash, tags, remote, guide.
/// </summary>
public class DashboardExplorer(string gitDir)
	: BaseExplorer(gitDir, new Guid("e4f5a6b7-c8d9-4e0f-1a2b-3c4d5e6f7a8b"))
{
	public override Panel CreatePanel() => new DashboardPanel(this);

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		using var repo = new Repository(GitDir);

		// --- Branch row ---
		var head = repo.Head;
		var tip = head.Tip;
		var branchLabel = head.FriendlyName;

		if (head.IsTracking && head.TrackingDetails is { } td)
		{
			if (td.AheadBy  > 0) branchLabel += $"  +{td.AheadBy} to push";
			if (td.BehindBy > 0) branchLabel += $"  -{td.BehindBy} to pull";
		}

		var tipInfo = tip is not null
			? $"{tip.Sha[..7]}  {tip.MessageShort}"
			: "(no commits yet)";

		yield return new DashboardFile("Branch", branchLabel, tipInfo, DashboardSection.Branch);

		// --- Branches row ---
		var localCount  = repo.Branches.Count(b => !b.IsRemote);
		var remoteCount = repo.Branches.Count(b => b.IsRemote);
		var branchesValue = $"{localCount} local";
		var branchesDetail = remoteCount > 0
			? $"{remoteCount} remote-tracking  (Enter to manage)"
			: "Enter to create, switch, or merge branches";

		yield return new DashboardFile("Branches", branchesValue, branchesDetail, DashboardSection.Branches);

		// --- History row ---
		var commitCount = repo.Head.Tip is not null
			? repo.Head.Commits.Take(1001).Count()  // cap scan at 1001 to avoid huge repos hanging
			: 0;
		var historyValue  = commitCount > 1000 ? "1000+ commits" : $"{commitCount} commit{(commitCount == 1 ? "" : "s")}";
		var historyDetail = repo.Head.Tip is not null
			? $"Enter to browse timeline  ·  HEAD: {repo.Head.Tip.Sha[..7]}"
			: "no commits yet";

		yield return new DashboardFile("History", historyValue, historyDetail, DashboardSection.History);

		// --- Working tree status ---
		if (!repo.Info.IsBare)
		{
			var status = repo.RetrieveStatus(new StatusOptions { IncludeUntracked = true });

			var stagedCount    = status.Count(e => Lib.StagedSymbol(e.State)   != " ");
			var unstagedCount  = status.Count(e => Lib.UnstagedSymbol(e.State) != " ");
			var untrackedCount = status.Untracked.Count();

			yield return new DashboardFile(
				"Staged",
				stagedCount > 0 ? $"{stagedCount} file{(stagedCount == 1 ? "" : "s")}" : "nothing",
				stagedCount > 0 ? "ready to commit  (F7)" : "nothing staged — use F5 to stage all",
				DashboardSection.Staged);

			yield return new DashboardFile(
				"Unstaged",
				unstagedCount > 0 ? $"{unstagedCount} file{(unstagedCount == 1 ? "" : "s")}" : "nothing",
				unstagedCount > 0 ? "modified, not yet staged  (Enter to review)" : "working tree clean",
				DashboardSection.Unstaged);

			yield return new DashboardFile(
				"Untracked",
				untrackedCount > 0 ? $"{untrackedCount} file{(untrackedCount == 1 ? "" : "s")}" : "none",
				untrackedCount > 0 ? "new files not yet tracked  (Enter to review)" : "",
				DashboardSection.Untracked);
		}

		// --- Stash row ---
		var stashCount = repo.Stashes.Count();
		yield return new DashboardFile(
			"Stash",
			stashCount > 0 ? $"{stashCount} saved" : "empty",
			stashCount > 0 ? "temporarily saved states  (Enter to manage)" : "no stashes",
			DashboardSection.Stash);

		// --- Tags row ---
		var tagCount = repo.Tags.Count();
		yield return new DashboardFile(
			"Tags",
			tagCount > 0 ? $"{tagCount} tag{(tagCount == 1 ? "" : "s")}" : "none",
			tagCount > 0 ? "Enter to manage" : "no tags",
			DashboardSection.Tags);

		// --- Remote row ---
		var remotes = repo.Network.Remotes.ToList();
		var remoteValue  = remotes.Count > 0 ? remotes[0].Name : "none";
		var remoteDetail = remotes.Count > 0
			? $"{remotes[0].Url}  (Enter to fetch / pull / push)"
			: "no remotes configured — Enter to add one";

		yield return new DashboardFile("Remote", remoteValue, remoteDetail, DashboardSection.Remote);

		// --- Guide row ---
		yield return new DashboardFile(
			"Guide",
			"Reference",
			"Git concepts, commands, and workflows explained — Enter to open",
			DashboardSection.Guide);

		// Update panel title
		if (args.Panel is DashboardPanel panel && panel.Title is null)
		{
			var repoName = Path.GetFileName(
				Path.TrimEndingDirectorySeparator(repo.Info.WorkingDirectory ?? GitDir));
			panel.Title = $"FarGit  ·  {repoName}";
			panel.CurrentLocation = repo.Info.WorkingDirectory ?? GitDir;
		}
	}
}
