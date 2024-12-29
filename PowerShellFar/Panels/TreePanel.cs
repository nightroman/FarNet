using System;
using System.Collections.Generic;
using FarNet;

namespace PowerShellFar;

/// <summary>
/// Panel with <see cref="TreeFile"/> items.
/// </summary>
/// <remarks>
/// Available view modes:
/// <ul>
/// <li>[Ctrl0] - tree and description columns</li>
/// <li>[Ctrl1] - tree column and description status</li>
/// </ul>
/// </remarks>
public class TreePanel : AnyPanel
{
	/// <summary>
	/// Gets the panel explorer.
	/// </summary>
	public new TreeExplorer Explorer => (TreeExplorer)base.Explorer;

	/// <summary>
	/// New tree panel with the explorer.
	/// </summary>
	/// <param name="explorer">The panel explorer.</param>
	public TreePanel(TreeExplorer explorer) : base(explorer)
	{
		IgnoreDirectoryFlag = true; // _090810_180151

		SortMode = PanelSortMode.Unsorted;

		// columns
		var cO = new SetColumn { Kind = "O", Name = "Name" };
		var cZ = new SetColumn { Kind = "Z", Name = "Description" };

		// mode: tree column and description status
		var plan0 = new PanelPlan
		{
			Columns = [cO],
			StatusColumns = [cZ]
		};
		SetPlan(0, plan0);

		// mode: tree and description columns
		var plan1 = new PanelPlan
		{
			Columns = [cO, cZ]
		};
		SetPlan((PanelViewMode)1, plan1);
	}

	internal override void ShowHelpForPanel()
	{
		Entry.Instance.ShowHelpTopic(HelpTopic.TreePanel);
	}

	/// <summary>
	/// Opens/closes the node.
	/// </summary>
	/// <param name="file">The node to open/close.</param>
	public override void OpenFile(FarFile file)
	{
		ArgumentNullException.ThrowIfNull(file);

		var node = (TreeFile)file;
		if (node._State == 0)
		{
			if (!node.IsNode)
				return;
			node.FillNode();
			node._State = 1;
		}
		else
		{
			node._State = -node._State;
		}
		UpdateRedraw(false);
	}

	/// <inheritdoc/>
	public override bool UIKeyPressed(KeyInfo key)
	{
		switch (key.VirtualKeyCode)
		{
			case KeyCode.LeftArrow:
				{
					if (!key.Is() && !key.IsAlt() || Far.Api.CommandLine.Length > 0)
						break;

					var file = CurrentFile;
					if (file is null)
						break;

					TreeFile node = (TreeFile)file;
					if (node._State == 1)
					{
						// reset
						if (key.IsAlt())
						{
							node.ChildFiles.Clear();
							node._State = 0;
							UpdateRedraw(false);
							return true;
						}

						// collapse
						OpenFile(file);
					}
					else if (node.Parent != null)
					{
						PostFile(node.Parent);
						Redraw();
					}

					return true;
				}
			case KeyCode.RightArrow:
				{
					if (!key.Is() && !key.IsAlt() || Far.Api.CommandLine.Length > 0)
						break;

					var file = CurrentFile;
					if (file is null)
						break;

					TreeFile node = (TreeFile)file;
					if (node != null && node._State != 1 && node.IsNode)
					{
						// reset
						if (key.IsAlt())
						{
							node.ChildFiles.Clear();
							node._State = 0;
						}

						// open
						OpenFile(file);
					}
					else
					{
						// go to next
						Redraw(CurrentIndex + 1, -1);
					}

					return true;
				}
		}
		return base.UIKeyPressed(key);
	}

	/// <inheritdoc/>
	public override IEnumerable<FarFile> UIGetFiles(GetFilesEventArgs args)
	{
		ArgumentNullException.ThrowIfNull(args);

		args.Parameter = new TreeExplorerGetFilesParameter() { ShowHidden = ShowHidden };

		return base.UIGetFiles(args);
	}
}
