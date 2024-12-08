using FarNet;
using RedisKit.UI;
using System.Linq;

namespace RedisKit.Panels;

class KeysPanel : BasePanel<KeysExplorer>
{
	public KeysPanel(KeysExplorer explorer) : base(explorer)
	{
		SortMode = PanelSortMode.Name;

		var co = new SetColumn { Kind = "O", Name = "Type", Width = 1 };
		var cn = new SetColumn { Kind = "N", Name = "Key" };
		var cm = new SetColumn { Kind = "DM", Name = "EOL" };

		var plan0 = new PanelPlan { Columns = [co, cn, cm] };
		SetPlan(0, plan0);

		SetView(plan0);
	}

	protected override string HelpTopic => "keys-panel";

	public override void UICloneFile(CloneFileEventArgs args)
	{
		if (args.File.IsDirectory)
		{
			args.Result = JobResult.Ignore;
			return;
		}

		var input = Explorer.GetNameInput(args.File);

		var ui = new InputBox2
		{
			Title = $"Clone '{args.File.Name}'",
			Text1 = input.Name,
			Text2 = input.Prefix,
			Prompt1 = "Key name",
			Prompt2 = "Prefix",
			History1 = Host.History.Key,
			History2 = Host.History.Prefix,
		};

		if (!ui.Show())
		{
			args.Result = JobResult.Ignore;
			return;
		}

		args.Data = new Files.ArgsDataName($"{ui.Text2}{ui.Text1}");
		Explorer.CloneFile(args);
	}

	public override void UICreateFile(CreateFileEventArgs args)
	{
		var ui = new InputBox2
		{
			Title = "Create String",
			Text2 = Explorer.Prefix,
			Prompt1 = "Key name",
			Prompt2 = "Prefix",
			History1 = Host.History.Key,
			History2 = Host.History.Prefix,
		};

		if (!ui.Show())
		{
			args.Result = JobResult.Ignore;
			return;
		}

		args.Data = new Files.ArgsDataName($"{ui.Text2}{ui.Text1}");
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
			message = $"Delete {args.Files.Count} key(s):";
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
		var input = Explorer.GetNameInput(args.File);

		var ui = new InputBox2
		{
			Text1 = input.Name,
			Text2 = input.Prefix,
			Prompt2 = "Prefix",
			History1 = Host.History.Key,
			History2 = Host.History.Prefix,
		};

		if (args.File.IsDirectory)
		{
			ui.Title = "Rename folder";
			ui.Prompt1 = "New folder";
		}
		else
		{
			ui.Title = "Rename key";
			ui.Prompt1 = "New key";
		}

		if (!ui.Show())
		{
			args.Result = JobResult.Ignore;
			return;
		}

		args.Data = new Files.ArgsDataName($"{ui.Text2}{ui.Text1}");
		Explorer.RenameFile(args);
	}
}
