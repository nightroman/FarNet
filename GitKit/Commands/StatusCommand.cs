using FarNet;
using LibGit2Sharp;
using System.Linq;
using System;

namespace GitKit;

sealed class StatusCommand : BaseCommand
{
	public StatusCommand(Repository repo) : base(repo)
	{
	}

	void WriteChanges()
	{
		// see TreeChanges.DebuggerDisplay
		var changes = Lib.GetChanges(_repo);
		if (changes.Count > 0)
		{
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
	}

	public override void Invoke()
	{
		// tip is null: empty repository, fresh orphan branch ~ both "unborn"
		Commit? tip = _repo.Head.Tip;

		if (!_repo.Info.IsBare)
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
			Far.Api.UI.Write(_repo.Head.FriendlyName);
			if (_repo.Info.IsHeadUnborn)
				Far.Api.UI.Write(" (unborn)");
		}
		else
		{
			var tracking = _repo.Head.TrackingDetails;
			if (tracking is not null)
			{
				if (tracking.AheadBy > 0)
					Far.Api.UI.Write($"+{tracking.AheadBy} ", ConsoleColor.Green);

				if (tracking.BehindBy > 0)
					Far.Api.UI.Write($"-{tracking.BehindBy} ", ConsoleColor.Red);
			}

			bool comma = false;
			foreach (var branch in _repo.Branches.Where(x => x.Tip == tip).OrderBy(x => x.IsRemote))
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
