using FarNet;
using GitKit.Commands;
using LibGit2Sharp;

namespace GitKit;

sealed class SetEnvCommand(CommandParameters parameters) : AbcCommand
{
	private readonly string _name = parameters.GetRequiredString(Param.Name);

	public override void Invoke()
	{
		// one-symbol rule
		var old = Environment.GetEnvironmentVariable(_name);
		if (old?.Length == 1 && !char.IsLetterOrDigit(old[0]))
			return;

		string root = Repository.Discover(Far.Api.CurrentDirectory);

		string text;
		if (root is null)
		{
			text = "n/a";
		}
		else
		{
			using var repo = new Repository(root);
			var head = repo.Head;

			text = head.FriendlyName;

			// add tracking
			if (head.IsTracking && head.TrackingDetails is { } tracking)
			{
				int n1 = tracking.AheadBy.GetValueOrDefault();
				int n2 = tracking.BehindBy.GetValueOrDefault();

				if (n1 > 0)
					text += $" +{n1}";

				if (n2 > 0)
					text += $" -{n2}";

				if (n1 + n2 == 0)
					text += " =";
			}

			// add changes (lazy call `git_diff_num_deltas`)
			var count = repo.Diff.Compare<TreeChanges>().Count;
			if (count > 0)
				text += $" ({count})";
		}

		if (text != old)
		{
			Environment.SetEnvironmentVariable(_name, text);
			Far.Api.UI.Redraw();
		}
	}
}
