using FarNet;
using GitKit.About;
using LibGit2Sharp;
using System;
using System.Linq;

namespace GitKit.Commands;

sealed class StatusCommand(CommandParameters parameters) : BaseCommand(parameters)
{
	readonly bool _showFiles = parameters.GetBool("ShowFiles");

	static void WriteChanges(Repository repo, bool showFiles)
	{
		// see TreeChanges.DebuggerDisplay
		var changes = Lib.GetChanges(repo);
		if (changes.Count == 0)
			return;

		if (showFiles)
		{
			Far.Api.UI.WriteLine($"Changes in {repo.Info.WorkingDirectory}", ConsoleColor.White);
			foreach (var change in changes)
				Far.Api.UI.WriteLine($"  {change.Status}:\t{change.Path}");
		}

		int n;

		n = changes.Added.Count();
		if (n > 0)
			Far.Api.UI.Write($"a{n} ", ConsoleColor.Red);

		n = changes.Modified.Count();
		if (n > 0)
			Far.Api.UI.Write($"m{n} ", ConsoleColor.Red);

		n = changes.Deleted.Count();
		if (n > 0)
			Far.Api.UI.Write($"d{n} ", ConsoleColor.Red);

		n = changes.TypeChanged.Count();
		if (n > 0)
			Far.Api.UI.Write($"t{n} ", ConsoleColor.Red);

		n = changes.Renamed.Count();
		if (n > 0)
			Far.Api.UI.Write($"r{n} ", ConsoleColor.Red);

		n = changes.Copied.Count();
		if (n > 0)
			Far.Api.UI.Write($"c{n} ", ConsoleColor.Red);

		//! sign of changes, just in case if none of the above
		Far.Api.UI.Write("- ");
	}

	public override void Invoke()
	{
		using var repo = new Repository(GitDir);

		// tip is null: empty repository, fresh orphan branch ~ both "unborn"
		Commit? tip = repo.Head.Tip;

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
			Far.Api.UI.Write(repo.Head.FriendlyName);
			if (repo.Info.IsHeadUnborn)
				Far.Api.UI.Write(" (unborn)");
		}
		else
		{
			var tracking = repo.Head.TrackingDetails;
			if (tracking is not null)
			{
				if (tracking.AheadBy > 0)
					Far.Api.UI.Write($"+{tracking.AheadBy} ", ConsoleColor.Green);

				if (tracking.BehindBy > 0)
					Far.Api.UI.Write($"-{tracking.BehindBy} ", ConsoleColor.Red);
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
