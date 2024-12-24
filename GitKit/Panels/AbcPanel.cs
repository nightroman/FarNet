using FarNet;
using System;

namespace GitKit.Panels;

abstract class AbcPanel(Explorer explorer) : Panel(explorer)
{
	protected abstract string HelpTopic { get; }

	internal abstract void AddMenu(IMenu menu);

	protected void SetView(PanelPlan plan0)
	{
		ViewMode = Far.Api.Panel is AbcPanel panel && 9 == (int)panel.ViewMode ? (PanelViewMode)9 : 0;

		var plan9 = plan0.Clone();
		plan9.IsFullScreen = true;
		SetPlan((PanelViewMode)9, plan9);
	}

	protected (TData?, TData?) GetSelectedDataRange<TData>(Func<FarFile, TData?> getData)
	{
		var files = GetSelectedFiles();
		if (files.Length >= 2)
			return (getData(files[0]), getData(files[^1]));

		var file1 = files.Length > 0 ? files[0] : null;
		var file2 = CurrentFile;

		if (ReferenceEquals(file1, file2))
			file1 = null;

		return (file1 is null ? default : getData(file1), file2 is null ? default : getData(file2));
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
		}

		return base.UIKeyPressed(key);
	}
}
