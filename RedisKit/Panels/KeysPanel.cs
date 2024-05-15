using FarNet;
using System.Linq;

namespace RedisKit;

class KeysPanel : BasePanel<KeysExplorer>
{
	public KeysPanel(KeysExplorer explorer) : base(explorer)
	{
		SortMode = PanelSortMode.Name;
		ViewMode = 0;

		var co = new SetColumn { Kind = "O", Name = "Type", Width = 1 };
		var cn = new SetColumn { Kind = "N", Name = "Key" };
		var cm = new SetColumn { Kind = "DM", Name = "EOL" };

		var plan0 = new PanelPlan { Columns = [co, cn, cm] };
		SetPlan(0, plan0);

		var plan9 = plan0.Clone();
		plan9.IsFullScreen = true;
		SetPlan((PanelViewMode)9, plan9);
	}

	protected override string HelpTopic => "keys-panel";

	internal override void AddMenu(IMenu menu)
	{
	}

	public override void UICloneFile(CloneFileEventArgs args)
	{
		var name = args.File.Name;
		var newName = Far.Api.Input("New key name", "Key", $"Clone '{name}'", name);
		if (newName is null)
		{
			args.Result = JobResult.Ignore;
			return;
		}

		args.Data = newName;
		Explorer.CloneFile(args);
	}

	public override void UICreateFile(CreateFileEventArgs args)
	{
		var newName = Far.Api.Input("New key name", "Key", $"New String", null);
		if (newName is null)
		{
			args.Result = JobResult.Ignore;
			return;
		}

		args.Data = newName;
		Explorer.CreateFile(args);
	}

	public override void UIDeleteFiles(DeleteFilesEventArgs args)
	{
		var text = $"Delete keys ({args.Files.Count}):\n{string.Join("\n", args.Files.Select(x => x.Name))}";
		var op = MessageOptions.YesNo | MessageOptions.LeftAligned;
		if (0 != Far.Api.Message(text, Host.MyName, op))
		{
			args.Result = JobResult.Ignore;
			return;
		}

		Explorer.DeleteFiles(args);
	}

	public override void UIRenameFile(RenameFileEventArgs args)
	{
		var newName = Far.Api.Input("New name", "Key", "Rename key", args.File.Name);
		if (newName is null)
		{
			args.Result = JobResult.Ignore;
			return;
		}

		args.Data = newName;
		Explorer.RenameFile(args);
	}

	void DoEnter()
	{
		var file = CurrentFile;
		if (file is null)
			return;

		var args = new ExploreDirectoryEventArgs( ExplorerModes.None, file);
		var explorer2 = Explorer.ExploreDirectory(args);
		if (explorer2 is null)
			return;

		explorer2.OpenPanelChild(this);
	}

	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			case KeyCode.Enter when key.Is():
				DoEnter();
				return true;
		}

		return base.UIKeyPressed(key);
	}
}
