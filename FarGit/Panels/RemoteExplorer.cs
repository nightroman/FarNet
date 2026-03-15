using FarNet;
using LibGit2Sharp;

namespace FarGit.Panels;

/// <summary>
/// Explorer for <see cref="RemotePanel"/>.
/// Lists all configured remotes for the repository.
/// </summary>
public class RemoteExplorer(string gitDir)
	: BaseExplorer(gitDir, new Guid("b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e"))
{
	public override Panel CreatePanel() => new RemotePanel(this);

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		using var repo = new Repository(GitDir);
		var remotes = repo.Network.Remotes.ToList();

		foreach (var r in remotes)
			yield return new RemoteFile(r);

		if (remotes.Count == 0)
			yield return new SetFile { Name = "  No remotes configured — press F7 to add one", Attributes = FileAttributes.ReadOnly };

		if (args.Panel is RemotePanel panel && panel.Title is null)
		{
			panel.Title = $"Remotes  [{repo.Head.FriendlyName}]  {repo.Info.WorkingDirectory}";
			panel.CurrentLocation = repo.Info.WorkingDirectory ?? GitDir;
		}
	}
}
