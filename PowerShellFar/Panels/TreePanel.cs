/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Panel with <see cref="TreeFile"/> items.
	/// </summary>
	public class TreePanel : AnyPanel
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public TreePanel()
		{
			IgnoreDirectoryFlag = true; // _090810_180151

			Panel.Info.UseAttrHighlighting = false;
			Panel.Info.UseFilter = true;
			Panel.Info.StartSortMode = PanelSortMode.Unsorted;

			PanelModeInfo mode0 = new PanelModeInfo();
			mode0.ColumnTitles = new string[] { "Name", "Description" };
			mode0.ColumnTypes = "O,Z";
			mode0.ColumnWidths = "0,0";

			PanelModeInfo mode1 = (PanelModeInfo)mode0.Clone();
			mode1.IsFullScreen = true;

			Panel.Info.SetMode(PanelViewMode.AlternativeFull, mode0);
			Panel.Info.SetMode(PanelViewMode.Full, mode1);

			Panel.KeyPressed += OnKeyPressedTreePanel;
		}

		/// <summary>
		/// With a root item.
		/// </summary>
		/// <param name="root">Root item.</param>
		public TreePanel(TreeFile root)
			: this()
		{
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
			A.Far.ShowHelp(A.Psf.AppHome, "TreePanel", HelpOptions.Path);
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

			Panel.Files.Add(item);

			if (item._State == 1)
			{
				foreach (TreeFile ti in item.ChildFiles)
					AddFileFromTreeItem(ti, showHidden);
			}
		}

		internal override void OnGettingData(PanelEventArgs e)
		{
			bool showHidden = Panel.ShowHidden;

			Panel.Files.Clear();
			foreach (TreeFile ti in _RootFiles)
				AddFileFromTreeItem(ti, showHidden);
		}

		void OnKeyPressedTreePanel(object sender, PanelKeyEventArgs e)
		{
			switch (e.Code)
			{
				case VKeyCode.LeftArrow:
					{
						if (e.State != KeyStates.None && e.State != KeyStates.Alt || A.Far.CommandLine.Length > 0)
							return;

						FarFile f = Panel.CurrentFile;
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
							Panel.PostFile(ti.Parent);
							Panel.Redraw();
						}
						return;
					}
				case VKeyCode.RightArrow:
					{
						if (e.State != KeyStates.None && e.State != KeyStates.Alt || A.Far.CommandLine.Length > 0)
							return;

						FarFile f = Panel.CurrentFile;
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
							Panel.Redraw(Panel.CurrentIndex + 1, -1);
						}
						return;
					}
			}
		}

	}

}
