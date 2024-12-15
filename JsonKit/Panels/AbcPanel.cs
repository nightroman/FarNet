using FarNet;

namespace JsonKit.Panels;

abstract class AbcPanel(AbcExplorer explorer) : Panel(explorer)
{
	protected abstract string HelpTopic { get; }

	internal AbcExplorer MyExplorer => (AbcExplorer)Explorer;

	protected void SetView(PanelPlan plan0)
	{
		ViewMode = Far.Api.Panel is AbcPanel panel && 9 == (int)panel.ViewMode ? (PanelViewMode)9 : 0;

		var plan9 = plan0.Clone();
		plan9.IsFullScreen = true;
		SetPlan((PanelViewMode)9, plan9);
	}

	void ShowHelp()
	{
		Host.Instance.ShowHelpTopic(HelpTopic);
	}

	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			case KeyCode.F1 when key.Is():
				ShowHelp();
				return true;

			case KeyCode.A when key.IsCtrl():
				return true;

			//! Mantis#2635 Ignore if auto-completion menu is opened
			case KeyCode.S when key.IsCtrl() && Far.Api.Window.Kind != WindowKind.Menu:
				SaveData();
				return true;
		}

		return base.UIKeyPressed(key);
	}

	public override void UIDeleteFiles(DeleteFilesEventArgs args)
	{
		var text = args.Force ?
			$"Set nulls ({args.Files.Count})?" :
			$"Remove items ({args.Files.Count})?";

		if (0 != Far.Api.Message(text, Host.MyName, MessageOptions.YesNo))
		{
			args.Result = JobResult.Ignore;
			return;
		}

		Explorer.DeleteFiles(args);
	}

	public override void UISetText(SetTextEventArgs args)
	{
		base.UISetText(args);
		Update(true);
	}

	protected override bool CanClose()
	{
		var explorer = MyExplorer;

		// has no file? nothing to save
		if (explorer.Args.FilePath is null)
			return true;

		// has parent? parent will save
		if (explorer.Args.Parent is { })
			return true;

		// no changes?
		if (!explorer.IsDirty())
			return true;

		int res = Far.Api.Message("JSON has been modified. Save?", Host.MyName, MessageOptions.YesNoCancel | MessageOptions.Warning);
		if (res == 0)
		{
			explorer.SaveData();
			return true;
		}

		if (res == 1)
			return true;

		return false;
	}

	public override bool SaveData()
	{
		var explorer = MyExplorer;

		// has no file? nothing to save
		var filePath = explorer.Args.FilePath;
		if (filePath is null)
		{
			Far.Api.Message("There is no source file.", Host.MyName);
			return false;
		}

		int res = Far.Api.Message($"Save JSON?\n{filePath}", Host.MyName, MessageOptions.YesNo);
		if (res != 0)
			return false;

		explorer.SaveData();
		return true;
	}
}
