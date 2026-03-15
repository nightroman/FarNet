using FarNet;
using LibGit2Sharp;

namespace FarGit.Panels;

/// <summary>
/// Panel for managing git stashes.
///
/// Key bindings:
///   Enter        – show stash contents (changed files)
///   Ins / A      – create a new stash
///   P            – pop stash (apply + drop)
///   Del          – drop stash
/// </summary>
public class StashPanel : BasePanel
{
	public StashPanel(StashExplorer explorer) : base(explorer)
	{
		SortMode = PanelSortMode.Unsorted;

		var co = new SetColumn { Kind = "O", Name = "Ref", Width = 12 };
		var cd = new SetColumn { Kind = "D", Name = "Date", Width = 12 };
		var cn = new SetColumn { Kind = "N", Name = "Message" };

		var plan0 = new PanelPlan { Columns = [co, cd, cn] };
		SetPlan(0, plan0);
		SetView(plan0);

		SetKeyBars([
			new KeyBar(KeyCode.F2, ControlKeyStates.None, "Menu",   "Panel actions"),
			new KeyBar(KeyCode.F3, ControlKeyStates.None, "Show",   "View stash contents"),
			new KeyBar(KeyCode.F5, ControlKeyStates.None, "Apply",  "Apply stash (keep it)"),
			new KeyBar(KeyCode.F6, ControlKeyStates.None, "Pop",    "Apply and drop stash"),
			new KeyBar(KeyCode.F7, ControlKeyStates.None, "Create", "Save current changes as stash"),
			new KeyBar(KeyCode.F8, ControlKeyStates.None, "Drop",   "Delete stash"),
		]);
	}

	protected override string HelpTopic => "stash-panel";

	// --- Actions -----------------------------------------------------------

	StashFile? CurrentStash => CurrentFile as StashFile;

	void ApplyStash()
	{
		var f = CurrentStash;
		if (f is null) return;

		using var repo = UseRepository();
		var result = repo.Stashes.Apply(f.Index);
		if (result == StashApplyStatus.Conflicts)
			Far.Api.Message("Stash applied with conflicts. Resolve them before continuing.", Const.ModuleName, MessageOptions.Warning);
		else
			Far.Api.Message($"Applied {f.Owner}", Const.ModuleName);

		Update(true);
		Redraw();
	}

	void PopStash()
	{
		var f = CurrentStash;
		if (f is null) return;

		if (0 != Far.Api.Message($"Pop (apply + drop) {f.Owner}?", Const.ModuleName, MessageOptions.YesNo))
			return;

		using var repo = UseRepository();
		var result = repo.Stashes.Pop(f.Index);
		if (result == StashApplyStatus.Conflicts)
			Far.Api.Message("Stash applied with conflicts.", Const.ModuleName, MessageOptions.Warning);

		Update(true);
		Redraw();
	}

	void DropStash()
	{
		var f = CurrentStash;
		if (f is null) return;

		if (0 != Far.Api.Message($"Drop {f.Owner}: {f.Name}?", Const.ModuleName, MessageOptions.YesNo))
			return;

		using var repo = UseRepository();
		repo.Stashes.Remove(f.Index);

		Update(true);
		Redraw();
	}

	void CreateStash()
	{
		var message = Far.Api.Input("Stash message (optional)", "GitStashMessage", "Create stash") ?? string.Empty;

		using var repo = UseRepository();
		var sig = Lib.BuildSignature(repo);
		repo.Stashes.Add(sig, message.Length > 0 ? message : null, StashModifiers.Default);

		Update(true);
		Redraw();
	}

	void ShowContents()
	{
		var f = CurrentStash;
		if (f is null) return;

		// The stash work-tree commit stores the changed files;
		// open a viewer showing the stash diff patch.
		using var repo = UseRepository();
		var stash = f.Stash;
		var parent = stash.WorkTree.Parents.FirstOrDefault();
		if (parent is null) return;

		var patch = repo.Diff.Compare<Patch>(parent.Tree, stash.WorkTree.Tree);
		var text = patch.Content.Replace("\uFEFF", string.Empty);

		var tmp = Path.ChangeExtension(Path.GetTempFileName(), ".diff");
		File.WriteAllText(tmp, text, System.Text.Encoding.UTF8);

		var viewer = Far.Api.CreateViewer();
		viewer.FileName = tmp;
		viewer.Title = $"{f.Owner}: {f.Name}";
		viewer.DeleteSource = DeleteSource.File;
		viewer.Open();
	}

	// --- Menu / key handling -----------------------------------------------

	internal override void AddMenu(IMenu menu)
	{
		menu.Add(Const.StashShow,   (_, _) => ShowContents());
		menu.Add(string.Empty).IsSeparator = true;
		menu.Add(Const.StashApply,  (_, _) => ApplyStash());
		menu.Add(Const.StashPop,    (_, _) => PopStash());
		menu.Add(string.Empty).IsSeparator = true;
		menu.Add(Const.StashCreate, (_, _) => CreateStash());
		menu.Add(Const.StashDrop,   (_, _) => DropStash());
	}

	public override void UIOpenFile(FarFile file)
	{
		ShowContents();
	}

	public override void UIDeleteFiles(DeleteFilesEventArgs args)
	{
		var names = string.Join("\n", args.Files.Select(f => f.Owner));
		if (0 != Far.Api.Message($"Drop stashes ({args.Files.Count}):\n{names}", Const.ModuleName, MessageOptions.YesNo))
		{
			args.Result = JobResult.Ignore;
			return;
		}

		// Drop from highest index to lowest so indices stay valid
		using var repo = UseRepository();
		foreach (var f in args.Files.Cast<StashFile>().OrderByDescending(x => x.Index))
			repo.Stashes.Remove(f.Index);
	}

	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			case KeyCode.F3 when key.Is():
				ShowContents();
				return true;

			case KeyCode.F5 when key.Is():
				ApplyStash();
				return true;

			case KeyCode.F6 when key.Is():
				PopStash();
				return true;

			case KeyCode.F7 when key.Is():
				CreateStash();
				return true;

			case KeyCode.F8 when key.Is():
				DropStash();
				return true;

			// Legacy aliases
			case KeyCode.A when key.Is():
			case KeyCode.Insert when key.Is():
				CreateStash();
				return true;

			case KeyCode.P when key.Is():
				PopStash();
				return true;
		}

		return base.UIKeyPressed(key);
	}
}
