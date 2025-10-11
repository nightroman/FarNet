using FarNet;

namespace RedisKit.Panels;

class HashPanel : BasePanel<HashExplorer>
{
	public HashPanel(HashExplorer explorer) : base(explorer)
	{
		SortMode = PanelSortMode.Name;

		var cn = new SetColumn { Kind = "N", Name = "Field" };
		var cz = new SetColumn { Kind = "Z", Name = "Value" };

		PanelPlan plan0;
		if (Explorer.Eol)
		{
			var cm = new SetColumn { Kind = "DM", Name = "EOL" };
			plan0 = new PanelPlan { Columns = [cn, cz, cm] };
		}
		else
		{
			plan0 = new PanelPlan { Columns = [cn, cz] };
		}

		SetPlan(0, plan0);

		SetView(plan0);
	}

	protected override string HelpTopic => "hash-panel";

	public override void UICloneFile(CloneFileEventArgs args)
	{
		var name = args.File.Name;
		var newName = Far.Api.Input("New field name", "Field", $"Clone '{name}'", name);
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
		var newName = Far.Api.Input("New field name", "Field", "Create hash field");
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
		if (0 != Far.Api.Message(text, Host.MyName, MessageOptions.YesNo))
		{
			args.Result = JobResult.Ignore;
			return;
		}

		Explorer.DeleteFiles(args);
	}

	public override void UIRenameFile(RenameFileEventArgs args)
	{
		var newName = Far.Api.Input("New name", "Field", "Rename field", args.File.Name);
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
		if (args.Result != JobResult.Done)
			return;

		Update(true);
	}
}
