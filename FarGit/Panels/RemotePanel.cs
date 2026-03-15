using FarNet;
using LibGit2Sharp;

namespace FarGit.Panels;

/// <summary>
/// Panel for managing git remotes and syncing with them.
/// Network operations are delegated to git.exe (Windows TLS + Credential Manager).
///
/// Key bindings:
///   F5  – Fetch (download changes without merging)
///   F6  – Pull  (download + merge into current branch)
///   F7  – Push  (upload local commits to remote)
///   F8  – Remove the selected remote
///   Ins – Add a new remote
/// </summary>
public class RemotePanel : BasePanel
{
	public RemotePanel(RemoteExplorer explorer) : base(explorer)
	{
		SortMode = PanelSortMode.Unsorted;

		var cn = new SetColumn { Kind = "N", Name = "Remote", Width = 12 };
		var co = new SetColumn { Kind = "O", Name = "URL" };

		var plan0 = new PanelPlan { Columns = [cn, co] };
		SetPlan(0, plan0);
		SetView(plan0);

		SetKeyBars([
			new KeyBar(KeyCode.F2, ControlKeyStates.None, "Menu",   "Panel actions"),
			new KeyBar(KeyCode.F5, ControlKeyStates.None, "Fetch",  "Download changes (no merge)"),
			new KeyBar(KeyCode.F6, ControlKeyStates.None, "Pull",   "Download + merge into current branch"),
			new KeyBar(KeyCode.F7, ControlKeyStates.None, "Push",   "Upload local commits"),
			new KeyBar(KeyCode.F8, ControlKeyStates.None, "Remove", "Remove this remote"),
		]);
	}

	protected override string HelpTopic => "remote-panel";

	RemoteFile? CurrentRemote => CurrentFile as RemoteFile;

	// ── Fetch ─────────────────────────────────────────────────────────────

	void Fetch()
	{
		var f = CurrentRemote;
		if (f is null) return;

		try
		{
			var output = Commands.RemoteOps.Fetch(GitDir, f.RemoteName);
			Update(true); Redraw();
			Far.Api.Message(output, $"Fetch — {f.RemoteName}");
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Pull ──────────────────────────────────────────────────────────────

	void Pull()
	{
		try
		{
			var output = Commands.RemoteOps.Pull(GitDir);
			Update(true); Redraw();
			Far.Api.Message(output, "Pull");
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Push ──────────────────────────────────────────────────────────────

	void PushToRemote()
	{
		var f = CurrentRemote;
		try
		{
			var output = Commands.RemoteOps.Push(GitDir, f?.RemoteName);
			Update(true); Redraw();
			Far.Api.Message(output, "Push");
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Add Remote ────────────────────────────────────────────────────────

	void AddRemote()
	{
		var name = Far.Api.Input("Remote name (e.g. 'origin'):", "FarGit-remote-name", "Add Remote");
		if (string.IsNullOrWhiteSpace(name)) return;

		var url = Far.Api.Input($"URL for '{name}':\n(e.g. https://github.com/user/repo.git)", "FarGit-remote-url", "Add Remote");
		if (string.IsNullOrWhiteSpace(url)) return;

		try
		{
			using var repo = UseRepository();
			repo.Network.Remotes.Add(name.Trim(), url.Trim());
			Update(true); Redraw();
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Remove Remote ─────────────────────────────────────────────────────

	void RemoveRemote()
	{
		var f = CurrentRemote;
		if (f is null) return;

		if (0 != Far.Api.Message(
			$"Remove remote '{f.RemoteName}' ({f.RemoteUrl})?\n\n" +
			"This removes the connection but does not delete any local commits.",
			Const.ModuleName, MessageOptions.YesNo))
			return;

		try
		{
			using var repo = UseRepository();
			repo.Network.Remotes.Remove(f.RemoteName);
			Update(true); Redraw();
		}
		catch (Exception ex)
		{
			Far.Api.Message(ex.Message, Const.ModuleName, MessageOptions.Warning);
		}
	}

	// ── Menu / key handling ───────────────────────────────────────────────

	internal override void AddMenu(IMenu menu)
	{
		menu.Add(Const.RemoteFetch, (_, _) => Fetch());
		menu.Add(Const.RemotePull,  (_, _) => Pull());
		menu.Add(Const.RemotePush,  (_, _) => PushToRemote());
		menu.Add(string.Empty).IsSeparator = true;
		menu.Add(Const.RemoteAdd,   (_, _) => AddRemote());
		menu.Add("&Remove remote",  (_, _) => RemoveRemote());
	}

	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			case KeyCode.F5 when key.Is():
				Fetch();
				return true;
			case KeyCode.F6 when key.Is():
				Pull();
				return true;
			case KeyCode.F7 when key.Is():
				PushToRemote();
				return true;
			case KeyCode.F8 when key.Is():
				RemoveRemote();
				return true;
			case KeyCode.Insert when key.Is():
				AddRemote();
				return true;
		}
		return base.UIKeyPressed(key);
	}
}
