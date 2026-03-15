using FarNet;
using LibGit2Sharp;

namespace FarGit.Commands;

/// <summary>
/// Shows a modal input dialog to collect a commit message, then commits immediately on OK.
/// For amend, the current commit message is pre-filled.
/// </summary>
static class Commit
{
	public static void Open(string gitDir, bool amend)
	{
		using var repo = new Repository(gitDir);

		// Guard: nothing staged (skip for amend — amend can be message-only)
		if (!amend)
		{
			var staged = repo.Diff.Compare<TreeChanges>(repo.Head.Tip?.Tree, DiffTargets.Index);
			if (!staged.Any())
			{
				Far.Api.Message(
					"Nothing staged to commit.\n\nUse F5 on the Dashboard or the Status panel to stage files first.",
					Const.ModuleName, MessageOptions.Warning);
				return;
			}
		}

		// Guard: git identity
		var sig = Lib.BuildSignature(repo);
		if (sig is null)
		{
			Far.Api.Message(
				"Git user identity is not configured.\n\n" +
				"Run these commands in a terminal:\n\n" +
				"  git config --global user.name \"Your Name\"\n" +
				"  git config --global user.email \"you@example.com\"",
				Const.ModuleName, MessageOptions.Warning);
			return;
		}

		var title  = amend ? "Amend Last Commit" : $"Commit on '{repo.Head.FriendlyName}'";
		var prompt = amend
			? "Edit commit message:"
			: "Commit message:";
		var history = "FarGit-commit";
		var prefill = amend ? repo.Head.Tip?.MessageShort?.TrimEnd() ?? "" : "";

		var message = Far.Api.Input(prompt, history, title, prefill);
		if (string.IsNullOrWhiteSpace(message)) return;

		try
		{
			var options = new CommitOptions { AmendPreviousCommit = amend };
			var commit  = repo.Commit(message.Trim(), sig, sig, options);

			RefreshPanel(Far.Api.Panel);
			RefreshPanel(Far.Api.Panel2);

			// Check whether a remote exists so we can offer to push
			bool hasRemote = repo.Network.Remotes.Any();
			var sha7 = commit.Sha[..7];
			if (hasRemote && 0 == Far.Api.Message(
				$"Committed {sha7} on '{repo.Head.FriendlyName}'.\n\nPush to remote now?",
				Const.ModuleName, MessageOptions.YesNo))
			{
				RemoteOps.Push(gitDir);
			}
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	static void RefreshPanel(IPanel? panel)
	{
		if (panel is Panels.AbcPanel)
		{
			panel.Update(true);
			panel.Redraw();
		}
	}
}
