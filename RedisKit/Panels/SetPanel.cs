using FarNet;
using System.Linq;

namespace RedisKit.Panels;

class SetPanel : BasePanel<SetExplorer>
{
	public SetPanel(SetExplorer explorer) : base(explorer)
	{
		SortMode = PanelSortMode.Name;
		ViewMode = 0;

		var cn = new SetColumn { Kind = "N", Name = "Member" };

		var plan0 = new PanelPlan { Columns = [cn] };
		SetPlan(0, plan0);

		var plan9 = plan0.Clone();
		plan9.IsFullScreen = true;
		SetPlan((PanelViewMode)9, plan9);
	}

	protected override string HelpTopic => "set-panel";

	public override void UICloneFile(CloneFileEventArgs args)
	{
		var name = args.File.Name;
		var newName = Far.Api.Input("New member", "Member", $"Clone '{name}'", name);
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
		var newName = Far.Api.Input("New member", "Member", "Create set member");
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
		var text = $"Delete members ({args.Files.Count}):\n{string.Join("\n", args.Files.Select(x => x.Name))}";
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
		var newName = Far.Api.Input("New name", "Member", "Rename member", args.File.Name);
		if (newName is null)
		{
			args.Result = JobResult.Ignore;
			return;
		}

		args.Data = newName;
		Explorer.RenameFile(args);
	}

	public override void UISetText(SetTextEventArgs args)
	{
		base.UISetText(args);
		Update(true);
	}
}
