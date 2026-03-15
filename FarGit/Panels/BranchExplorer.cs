using FarNet;
using LibGit2Sharp;

namespace FarGit.Panels;

/// <summary>
/// Explorer for <see cref="BranchPanel"/>.
/// Lists local branches (current first) then remote-tracking branches.
/// </summary>
public class BranchExplorer(string gitDir)
	: BaseExplorer(gitDir, new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"))
{
	public override Panel CreatePanel() => new BranchPanel(this);

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		using var repo = new Repository(GitDir);

		var local = repo.Branches
			.Where(b => !b.IsRemote)
			.OrderByDescending(b => b.IsCurrentRepositoryHead)
			.ThenBy(b => b.FriendlyName)
			.ToList();

		var remote = repo.Branches
			.Where(b => b.IsRemote)
			.OrderBy(b => b.FriendlyName)
			.ToList();

		yield return SectionHeader($"  Local branches ({local.Count})  — Enter to switch, F7 to create, F5 to merge, F8 to delete");
		foreach (var b in local)
			yield return new BranchFile(b);

		if (remote.Count > 0)
		{
			yield return SectionHeader($"  Remote tracking ({remote.Count})");
			foreach (var b in remote)
				yield return new BranchFile(b);
		}

		if (args.Panel is BranchPanel panel && panel.Title is null)
		{
			panel.Title = $"Branches  [{repo.Head.FriendlyName}]  {repo.Info.WorkingDirectory}";
			panel.CurrentLocation = repo.Info.WorkingDirectory ?? GitDir;
		}
	}

	static SetFile SectionHeader(string text) => new()
	{
		Name = text,
		Attributes = FileAttributes.ReadOnly,
	};
}
