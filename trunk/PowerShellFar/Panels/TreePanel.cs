
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using FarNet;

namespace PowerShellFar
{
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
		public new TreeExplorer Explorer { get { return (TreeExplorer)base.Explorer; } }
		/// <summary>
		/// New tree panel with the explorer.
		/// </summary>
		/// <param name="explorer">The panel explorer.</param>
		public TreePanel(TreeExplorer explorer)
			: base(explorer)
		{
			IgnoreDirectoryFlag = true; // _090810_180151

			SortMode = PanelSortMode.Unsorted;

			// columns
			SetColumn cO = new SetColumn() { Kind = "O", Name = "Name" };
			SetColumn cZ = new SetColumn() { Kind = "Z", Name = "Description" };

			// mode: tree and description columns
			PanelPlan plan0 = new PanelPlan();
			plan0.Columns = new FarColumn[] { cO, cZ };
			SetPlan((PanelViewMode)0, plan0);

			// mode: tree column and description status
			PanelPlan plan1 = new PanelPlan();
			plan1.Columns = new FarColumn[] { cO };
			plan1.StatusColumns = new FarColumn[] { cZ };
			SetPlan((PanelViewMode)1, plan1);
		}
		internal override void ShowHelpForPanel()
		{
			Far.Api.ShowHelpTopic("TreePanel");
		}
		/// <summary>
		/// Opens/closes the node.
		/// </summary>
		/// <param name="file">The node to open/close.</param>
		public override void OpenFile(FarFile file)
		{
			if (file == null)
				throw new ArgumentNullException("file");

			TreeFile node = (TreeFile)file;
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
			if (key == null) throw new ArgumentNullException("key");
			
			switch (key.VirtualKeyCode)
			{
				case KeyCode.LeftArrow:
					{
						if (!key.Is() && !key.IsAlt() || Far.Api.CommandLine.Length > 0)
							break;

						FarFile file = CurrentFile;
						if (file == null)
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

						FarFile file = CurrentFile;
						if (file == null)
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

			// base
			return base.UIKeyPressed(key);
		}
		/// <inheritdoc/>
		public override IList<FarFile> UIGetFiles(GetFilesEventArgs args)
		{
			if (args == null) return null;

			args.Parameter = new TreeExplorerGetFilesParameter() { ShowHidden = ShowHidden };
			
			return base.UIGetFiles(args);
		}
	}
}
