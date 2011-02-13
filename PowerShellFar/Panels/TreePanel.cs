
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
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
		/// Constructor.
		/// </summary>
		public TreePanel()
		{
			IgnoreDirectoryFlag = true; // _090810_180151

			UseFilter = true;
			SortMode = PanelSortMode.Unsorted;

			KeyPressed += OnKeyPressedTreePanel;

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

		/// <summary>
		/// With a root item.
		/// </summary>
		/// <param name="root">Root item.</param>
		public TreePanel(TreeFile root)
			: this()
		{
			if (root == null)
				throw new ArgumentNullException("root");

			RootFiles.Add(root);
			root.Expand();
		}

		/// <summary>
		/// Root files.
		/// </summary>
		public TreeFileCollection RootFiles
		{
			get { return _RootFiles; }
		}
		TreeFileCollection _RootFiles = new TreeFileCollection(null);

		internal override void ShowHelp()
		{
			Help.ShowTopic("TreePanel");
		}

		/// <summary>
		/// Opens/closes the node.
		/// </summary>
		public override void OpenFile(FarFile file)
		{
			if (file == null)
				throw new ArgumentNullException("file");

			TreeFile ti = (TreeFile)file;
			if (ti._State == 0)
			{
				if (ti.Fill == null)
					return;
				ti.Fill(ti, null);
				ti._State = 1;
			}
			else
			{
				ti._State = -ti._State;
			}
			UpdateRedraw(false);
		}

		void AddFileFromTreeItem(TreeFile item, bool showHidden)
		{
			if (!showHidden && item.IsHidden)
				return;

			int level = item.Level;

			string nodePrefix = new string(' ', level * 2);

			if (item.Fill != null)
			{
				if (item._State == 1)
					nodePrefix += "- ";
				else
					nodePrefix += "+ ";
			}
			else
			{
				nodePrefix += "  ";
			}

			if (string.IsNullOrEmpty(item.Name)) //???
				item.Name = string.Empty;

			item.Owner = nodePrefix + item.Name;

			Files.Add(item);

			if (item._State == 1)
			{
				foreach (TreeFile ti in item.ChildFiles)
					AddFileFromTreeItem(ti, showHidden);
			}
		}

		internal override void OnUpdateFiles(PanelEventArgs e)
		{
			bool showHidden = ShowHidden;

			Files.Clear();
			foreach (TreeFile ti in _RootFiles)
				AddFileFromTreeItem(ti, showHidden);
		}

		void OnKeyPressedTreePanel(object sender, PanelKeyEventArgs e)
		{
			switch (e.Code)
			{
				case VKeyCode.LeftArrow:
					{
						if (e.State != KeyStates.None && e.State != KeyStates.Alt || Far.Net.CommandLine.Length > 0)
							return;

						FarFile f = CurrentFile;
						if (f == null)
							return;
						TreeFile ti = (TreeFile)f;

						e.Ignore = true;
						if (ti._State == 1)
						{
							// reset
							if (e.State == KeyStates.Alt)
							{
								ti.ChildFiles.Clear();
								ti._State = 0;
								UpdateRedraw(false);
								return;
							}

							// collapse
							OpenFile(f);
						}
						else if (ti.Parent != null)
						{
							PostFile(ti.Parent);
							Redraw();
						}
						return;
					}
				case VKeyCode.RightArrow:
					{
						if (e.State != KeyStates.None && e.State != KeyStates.Alt || Far.Net.CommandLine.Length > 0)
							return;

						FarFile f = CurrentFile;
						if (f == null)
							return;

						TreeFile ti = (TreeFile)f;
						e.Ignore = true;
						if (ti != null && ti._State != 1 && ti.Fill != null)
						{
							// reset
							if (e.State == KeyStates.Alt)
							{
								ti.ChildFiles.Clear();
								ti._State = 0;
							}

							// open
							OpenFile(f);
						}
						else
						{
							// go to next
							Redraw(CurrentIndex + 1, -1);
						}
						return;
					}
			}
		}

	}

}
