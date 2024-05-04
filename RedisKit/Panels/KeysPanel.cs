using FarNet;
using StackExchange.Redis;
using System;
using System.Linq;

namespace RedisKit;

class KeysPanel : BasePanel<KeysExplorer>
{
    public KeysPanel(KeysExplorer explorer) : base(explorer)
    {
        Title = $"Keys {explorer.Database.Multiplexer.Configuration}";
        SortMode = PanelSortMode.Name;
        ViewMode = 0;

        var co = new SetColumn { Kind = "O", Name = "T", Width = 1 };
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

    static string? InputNewKey(RedisKey key)
    {
        return Far.Api.Input(
            "New key name",
            "Key",
            $"Create new key from '{key}'",
            key);
    }

    static void CloneKey(ExplorerEventArgs args, RedisKey key, Action action)
    {
        var newName = InputNewKey(key);
        if (string.IsNullOrEmpty(newName))
        {
            args.Result = JobResult.Ignore;
            return;
        }

        args.Data = (key, newName);
        action();
    }

    public override void UICloneFile(CloneFileEventArgs args)
    {
        var key = (RedisKey)args.File.Data!;
        CloneKey(args, key, () => Explorer.CloneFile(args));
    }

    public override void UICreateFile(CreateFileEventArgs args)
    {
        //var branch = Repository.Head;
        //CloneBranch(args, branch, () => Explorer.CreateFile(args));
    }

    public override void UIDeleteFiles(DeleteFilesEventArgs args)
    {
        var text = $"Delete {args.Files.Count} keys:\n{string.Join("\n", args.Files.Select(x => x.Name))}";
        var op = MessageOptions.YesNo | MessageOptions.LeftAligned;
        if (args.Force)
            op |= MessageOptions.Warning;

        if (0 != Far.Api.Message(text, Host.MyName, op))
        {
            args.Result = JobResult.Ignore;
            return;
        }

        Explorer.DeleteFiles(args);
    }

    public override void UIRenameFile(RenameFileEventArgs args)
    {
        var newName = (Far.Api.Input("New key", "Key", "Rename key", args.File.Name) ?? string.Empty).Trim();
        if (newName.Length == 0)
        {
            args.Result = JobResult.Ignore;
            return;
        }

        args.Data = newName;
        Explorer.RenameFile(args);
    }

    public override bool UIKeyPressed(KeyInfo key)
    {
        switch (key.VirtualKeyCode)
        {
            // checkout cursor branch
            case KeyCode.Enter when key.IsShift():
                //CheckoutBranch();
                return true;
        }

        return base.UIKeyPressed(key);
    }
}
