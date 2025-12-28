using FarNet;
using LibGit2Sharp;

namespace GitKit.Commands;

sealed class StatusCommand(CommandParameters parameters) : BaseCommand(parameters)
{
	readonly bool _showFiles = parameters.GetBool("ShowFiles");

	static void WriteChanges(Repository repo, bool showFiles)
	{
		// see TreeChanges.DebuggerDisplay
		var changes = Lib.GetChanges(repo);
		var count = changes.Count;
		if (count == 0)
			return;

		if (showFiles)
		{
			Far.Api.UI.WriteLine($"Changes in {repo.Info.WorkingDirectory}", ConsoleColor.White);
			foreach (var change in changes)
				Far.Api.UI.WriteLine($"  {change.Status}:\t{change.Path}");
		}

		Far.Api.UI.Write($"({count}) ", ConsoleColor.Red);
	}

	public override void Invoke()
	{
		using var repo = new Repository(GitDir);
		var head = repo.Head;

		// tip is null: empty repository, fresh orphan branch ~ both "unborn"
		Commit? tip = head.Tip;

		if (!repo.Info.IsBare)
			WriteChanges(repo, _showFiles);

		var settings = Settings.Default.GetData();
		if (tip is not null)
		{
			Far.Api.UI.Write(tip.Sha[0..settings.ShaPrefixLength], ConsoleColor.DarkYellow);
			Far.Api.UI.Write(" ");
		}

		Far.Api.UI.Write("(");
		Far.Api.UI.Write("HEAD -> ", ConsoleColor.Cyan);

		if (tip is null)
		{
			Far.Api.UI.Write(head.FriendlyName);
			if (repo.Info.IsHeadUnborn)
				Far.Api.UI.Write(" (unborn)");
		}
		else
		{
			if (head.IsTracking && head.TrackingDetails is { } tracking)
			{
				if (tracking.AheadBy is int n1 && n1 > 0)
					Far.Api.UI.Write($"+{n1} ", ConsoleColor.Green);

				if (tracking.BehindBy is int n2 && n2 > 0)
					Far.Api.UI.Write($"-{n2} ", ConsoleColor.Red);
			}

			var branches = repo.Branches
				.Where(x => x.Tip == tip)
				.OrderBy(x => x.IsCurrentRepositoryHead ? 0 : 1)
				.ThenBy(x => x.IsRemote ? 1 : 0)
				.ThenBy(x => x.FriendlyName);

			bool comma = false;
			foreach (var branch in branches)
			{
				if (comma)
					Far.Api.UI.Write(", ");

				comma = true;
				Far.Api.UI.Write(branch.FriendlyName, branch.IsRemote ? ConsoleColor.Red : branch.IsCurrentRepositoryHead ? ConsoleColor.Green : ConsoleColor.Gray);
			}
		}

		Far.Api.UI.Write(")");

		if (tip is not null)
			Far.Api.UI.Write($" {tip.MessageShort}");

		Far.Api.UI.WriteLine();
	}
}
