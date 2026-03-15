using FarNet;
using LibGit2Sharp;

namespace FarGit.Panels;

/// <summary>
/// Explorer for <see cref="StatusPanel"/>. Provides the working-tree status
/// (staged changes, unstaged changes, and untracked files) as panel entries.
/// </summary>
public class StatusExplorer(string gitDir)
	: BaseExplorer(gitDir, new Guid("b1c2d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"))
{
	public override Panel CreatePanel() => new StatusPanel(this);

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		using var repo = new Repository(GitDir);
		var status = repo.RetrieveStatus(new StatusOptions { IncludeUntracked = true });

		// Pre-collect each group so we can show counts in the section headers
		var staged = status
			.Select(e => (entry: e, sym: Lib.StagedSymbol(e.State)))
			.Where(x => x.sym != " ")
			.ToList();

		var unstaged = status
			.Select(e => (entry: e, sym: Lib.UnstagedSymbol(e.State)))
			.Where(x => x.sym != " ")
			.ToList();

		var untracked = status.Untracked.ToList();

		// 1. Staged section
		if (staged.Count > 0)
		{
			yield return SectionHeader($"  Staged ({staged.Count})  — F5 to unstage, F7 to commit");
			foreach (var (entry, sym) in staged)
				yield return new StatusFile(entry.FilePath, Const.CatStaged, sym, entry);
		}

		// 2. Unstaged section
		if (unstaged.Count > 0)
		{
			yield return SectionHeader($"  Unstaged ({unstaged.Count})  — F5 to stage");
			foreach (var (entry, sym) in unstaged)
				yield return new StatusFile(entry.FilePath, Const.CatUnstaged, sym, entry);
		}

		// 3. Untracked section
		if (untracked.Count > 0)
		{
			yield return SectionHeader($"  Untracked ({untracked.Count})  — F5 to start tracking");
			foreach (var entry in untracked)
				yield return new StatusFile(entry.FilePath, Const.CatUntracked, "?", entry);
		}

		if (staged.Count == 0 && unstaged.Count == 0 && untracked.Count == 0)
			yield return SectionHeader("  Working tree clean — nothing to commit");

		// Update panel title when first loaded
		if (args.Panel is StatusPanel panel && panel.Title is null)
		{
			var work = repo.Info.WorkingDirectory;
			var branch = repo.Head.FriendlyName;
			panel.Title = $"Git Status  [{branch}]  {work}";
			panel.CurrentLocation = work;
		}
	}

	/// <summary>Creates a non-interactive section header row.</summary>
	static SetFile SectionHeader(string text) => new()
	{
		Name = text,
		Attributes = FileAttributes.ReadOnly,
	};
}
