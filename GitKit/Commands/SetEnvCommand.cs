using FarNet;
using GitKit.Commands;
using LibGit2Sharp;

namespace GitKit;

sealed class SetEnvCommand(CommandParameters parameters) : AbcCommand
{
	private readonly string _name = parameters.GetRequiredString(Param.Name);

	public override void Invoke()
	{
		string root = Repository.Discover(Far.Api.CurrentDirectory);

		string text;
		if (root is null)
		{
			text = "n/a";
		}
		else
		{
			using var repo = new Repository(root);

			text = repo.Head.FriendlyName;

			//! implemented as lazy call `git_diff_num_deltas`
			var count = repo.Diff.Compare<TreeChanges>().Count;
			if (count > 0)
				text += $" ({count})";
		}

		var text2 = Environment.GetEnvironmentVariable(_name);
		if (text != text2)
		{
			Environment.SetEnvironmentVariable(_name, text);
			Far.Api.UI.Redraw();
		}
	}
}
