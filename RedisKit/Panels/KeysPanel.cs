using FarNet;
using System.Linq;

namespace RedisKit.Panels;

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

	public override void UICloneFile(CloneFileEventArgs args)
	{
		if (args.File.IsDirectory)
		{
			args.Result = JobResult.Ignore;
			return;
		}

		var name = args.File.Name;
		var newName = Far.Api.Input("New key name", "Key", $"Clone '{name}'", name);
		if (newName is null)
		{
			args.Result = JobResult.Ignore;
			return;
		}

		args.Data = new Files.ArgsDataName(newName);
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

		args.Data = new Files.ArgsDataName(newName);
		Explorer.CreateFile(args);
	}

	public override void UIDeleteFiles(DeleteFilesEventArgs args)
	{
		string message;
		if (Explorer.Colon is { })
		{
			var n1 = args.Files.Count(x => x.IsDirectory);
			var n2 = args.Files.Count(x => !x.IsDirectory);
			message = $"Delete {n1} folder(s), {n2} key(s):";
		}
		else
		{
			message = $"Delete {args.Files.Count}) key(s):";
		}

		var text = $"{message}\n{string.Join("\n", args.Files.Select(x => x.Name))}";
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
		if (args.File.IsDirectory)
			return;

		var newName = args.File.IsDirectory ?
			Far.Api.Input("New folder name", "Folder", "Rename folder", args.File.DataFolder().Prefix) :
			Far.Api.Input("New key name", "Key", "Rename key", args.File.Name);

		if (newName is null)
		{
			args.Result = JobResult.Ignore;
			return;
		}

		args.Data = new Files.ArgsDataName(newName);
		Explorer.RenameFile(args);
	}
}
