using FarNet;
using LibGit2Sharp;
using System.Linq;
using System;
using System.Data.Common;

namespace GitKit;

sealed class StatusCommand : BaseCommand
{
	readonly bool _showFiles;

	public StatusCommand(DbConnectionStringBuilder parameters) : base(parameters)
	{
		_showFiles = parameters.GetBool("ShowFiles");
	}

	void WriteChanges()
	{
		// see TreeChanges.DebuggerDisplay
		var changes = Lib.GetChanges(Repository);
		if (changes.Count == 0)
			return;

		if (_showFiles)
		{
			Far.Api.UI.WriteLine($"Changes in {Repository.Info.WorkingDirectory}", ConsoleColor.White);
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
		// tip is null: empty repository, fresh orphan branch ~ both "unborn"
		Commit? tip = Repository.Head.Tip;

		if (!Repository.Info.IsBare)
			WriteChanges();

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
			Far.Api.UI.Write(Repository.Head.FriendlyName);
			if (Repository.Info.IsHeadUnborn)
				Far.Api.UI.Write(" (unborn)");
		}
		else
		{
			var tracking = Repository.Head.TrackingDetails;
			if (tracking is not null)
			{
				if (tracking.AheadBy > 0)
					Far.Api.UI.Write($"+{tracking.AheadBy} ", ConsoleColor.Green);

				if (tracking.BehindBy > 0)
					Far.Api.UI.Write($"-{tracking.BehindBy} ", ConsoleColor.Red);
			}

			var branches = Repository.Branches
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
