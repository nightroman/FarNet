using FarNet;
using LibGit2Sharp;

namespace FarGit.Panels;

/// <summary>
/// Explorer for <see cref="TagPanel"/>. Lists all tags.
/// </summary>
public class TagExplorer(string gitDir)
	: BaseExplorer(gitDir, new Guid("d3e4f5a6-b7c8-4d9e-0f1a-2b3c4d5e6f7a"))
{
	public override Panel CreatePanel() => new TagPanel(this);

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		using var repo = new Repository(GitDir);

		foreach (var tag in repo.Tags.OrderByDescending(t => GetWhen(t)))
		{
			var (desc, when) = BuildDescription(tag, repo);
			yield return new TagFile(tag.FriendlyName, desc, when, tag);
		}

		if (args.Panel is TagPanel panel && panel.Title is null)
		{
			panel.Title = $"Git Tags — {repo.Info.WorkingDirectory}";
			panel.CurrentLocation = repo.Info.WorkingDirectory ?? GitDir;
		}
	}

	static (string desc, DateTime when) BuildDescription(Tag tag, Repository repo)
	{
		// Annotated tag → use tagger info
		if (tag.IsAnnotated && tag.Annotation is { } ann)
		{
			var when = ann.Tagger.When.LocalDateTime;
			var sha = ann.Target.Sha[..7];
			var msg = ann.Message.Split('\n', 2)[0].Trim();
			return ($"{sha} {msg}", when);
		}

		// Lightweight tag → peel to commit
		var commit = repo.Lookup<Commit>(tag.Target.Id);
		if (commit is not null)
			return ($"{commit.Sha[..7]} {commit.MessageShort}", commit.Author.When.LocalDateTime);

		return (tag.Target.Sha[..7], DateTime.MinValue);
	}

	static DateTime GetWhen(Tag tag)
	{
		if (tag.IsAnnotated && tag.Annotation is { } ann)
			return ann.Tagger.When.LocalDateTime;
		return DateTime.MinValue;
	}
}
