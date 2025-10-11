﻿using FarNet;

namespace RedisKit.Panels;

class ListPanel : BasePanel<ListExplorer>
{
	public ListPanel(ListExplorer explorer) : base(explorer)
	{
		SortMode = PanelSortMode.Unsorted;

		var cs = new SetColumn { Kind = "S", Name = "#", Width = 5 };
		var cn = new SetColumn { Kind = "N", Name = "Item" };

		var plan0 = new PanelPlan { Columns = [cs, cn] };
		SetPlan(0, plan0);

		SetView(plan0);
	}

	protected override string HelpTopic => "list-panel";

	public override void UICloneFile(CloneFileEventArgs args)
	{
		var name = args.File.Name;
		var newName = Far.Api.Input("New item", "Item", $"Clone '{name}'", name);
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
		var newName = Far.Api.Input("New item", "Item", "Append new item");
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
		var text = $"Delete items ({args.Files.Count}):\n{string.Join("\n", args.Files.Select(x => x.Name))}";
		if (0 != Far.Api.Message(text, Host.MyName, MessageOptions.YesNo))
		{
			args.Result = JobResult.Ignore;
			return;
		}

		Explorer.DeleteFiles(args);
	}

	public override void UIRenameFile(RenameFileEventArgs args)
	{
		var newName = Far.Api.Input("New name", "Item", "Rename item", args.File.Name);
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
