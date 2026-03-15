using FarNet;
using LibGit2Sharp;

namespace FarGit.Panels;

/// <summary>
/// Panel for browsing and managing git tags.
///
/// Key bindings:
///   Enter        – show the tagged commit as a diff patch
///   Ins / C      – create a new lightweight tag on HEAD
///   Del          – delete the selected tag(s)
/// </summary>
public class TagPanel : BasePanel
{
	public TagPanel(TagExplorer explorer) : base(explorer)
	{
		SortMode = PanelSortMode.Unsorted;

		var cd = new SetColumn { Kind = "D", Name = "Date", Width = 12 };
		var cn = new SetColumn { Kind = "N", Name = "Tag" };
		var cz = new SetColumn { Kind = "Z", Name = "Target", Width = 52 };

		var plan0 = new PanelPlan { Columns = [cd, cn, cz] };
		SetPlan(0, plan0);
		SetView(plan0);

		SetKeyBars([
			new KeyBar(KeyCode.F2, ControlKeyStates.None, "Menu",   "Panel actions"),
			new KeyBar(KeyCode.F3, ControlKeyStates.None, "Show",   "View tagged commit diff"),
			new KeyBar(KeyCode.F7, ControlKeyStates.None, "Create", "Create new tag on HEAD"),
			new KeyBar(KeyCode.F8, ControlKeyStates.None, "Delete", "Delete tag"),
		]);
	}

	protected override string HelpTopic => "tag-panel";

	// --- Actions -----------------------------------------------------------

	TagFile? CurrentTag => CurrentFile as TagFile;

	void ShowTaggedCommit()
	{
		var f = CurrentTag;
		if (f is null) return;

		using var repo = UseRepository();

		// Peel to commit
		var target = f.Tag.PeeledTarget ?? f.Tag.Target;
		var commit = repo.Lookup<Commit>(target.Id);
		if (commit is null)
		{
			Far.Api.Message("Cannot resolve tag target to a commit.", Const.ModuleName);
			return;
		}

		var parent = commit.Parents.FirstOrDefault();
		var patch = repo.Diff.Compare<Patch>(parent?.Tree, commit.Tree);
		var text = patch.Content.Replace("\uFEFF", string.Empty);

		var tmp = Path.ChangeExtension(Path.GetTempFileName(), ".diff");
		File.WriteAllText(tmp, text, System.Text.Encoding.UTF8);

		var viewer = Far.Api.CreateViewer();
		viewer.FileName = tmp;
		viewer.Title = $"Tag {f.Name}: {commit.MessageShort}";
		viewer.DeleteSource = DeleteSource.File;
		viewer.Open();
	}

	void CreateTag()
	{
		var name = Far.Api.Input("Tag name", "GitTagName", "Create tag on HEAD");
		if (string.IsNullOrWhiteSpace(name)) return;

		using var repo = UseRepository();

		if (repo.Tags[name] is not null)
		{
			Far.Api.Message($"Tag '{name}' already exists.", Const.ModuleName, MessageOptions.Warning);
			return;
		}

		repo.ApplyTag(name);

		PostName(name);
		Update(true);
		Redraw();
	}

	void DeleteTag(IList<FarFile> files)
	{
		var names = string.Join("\n", files.Select(f => f.Name));
		if (0 != Far.Api.Message($"Delete tag(s):\n{names}", Const.ModuleName, MessageOptions.YesNo))
			return;

		using var repo = UseRepository();
		foreach (var f in files)
			repo.Tags.Remove(f.Name);

		Update(true);
		Redraw();
	}

	// --- Menu / key handling -----------------------------------------------

	internal override void AddMenu(IMenu menu)
	{
		menu.Add(Const.TagShow,   (_, _) => ShowTaggedCommit());
		menu.Add(string.Empty).IsSeparator = true;
		menu.Add(Const.TagCreate, (_, _) => CreateTag());
		if (CurrentFile is not null)
		{
			menu.Add(string.Empty).IsSeparator = true;
			menu.Add(Const.TagDelete, (_, _) => DeleteTag([CurrentFile]));
		}
	}

	public override void UIOpenFile(FarFile file)
	{
		ShowTaggedCommit();
	}

	public override void UIDeleteFiles(DeleteFilesEventArgs args)
	{
		DeleteTag(args.Files);
	}

	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			case KeyCode.F3 when key.Is():
				ShowTaggedCommit();
				return true;

			case KeyCode.F7 when key.Is():
				CreateTag();
				return true;

			case KeyCode.F8 when key.Is():
				if (CurrentFile is not null) DeleteTag([CurrentFile]);
				return true;

			// Legacy aliases
			case KeyCode.C when key.Is():
			case KeyCode.Insert when key.Is():
				CreateTag();
				return true;
		}

		return base.UIKeyPressed(key);
	}
}
