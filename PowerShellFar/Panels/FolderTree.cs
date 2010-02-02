/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

/*
_090810_180151
 * File system: use and process Hidden attributes in the expected way.
 * Folders use a directory flag (to consume native Far highlighting of directories, not files!).
*/

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// <see cref="TreePanel"/> with provider container items.
	/// </summary>
	/// <remarks>
	/// See <see cref="TreePanel"/> for details.
	/// </remarks>
	public sealed class FolderTree : TreePanel
	{
		/// <summary>
		/// At the current location.
		/// </summary>
		public FolderTree() : this(null) { }

		/// <summary>
		/// At the given location.
		/// </summary>
		public FolderTree(string path)
		{
			// _090810_180151
			Panel.Info.UseAttributeHighlighting = false;
			Panel.Info.UseHighlighting = true;
			
			// _091015_190130 Use of GettingInfo is problematic: it is called after Close()
			// and somehow Close() may not work. To watch this in future Far versions.
			// For now use Redrawing event, it looks working fine.

			// For updating the panel path.
			Panel.Redrawing += Updating;

			if (string.IsNullOrEmpty(path) || path == ".")
			{
				FarFile f = A.Far.Panel.CurrentFile;
				if (f != null)
				{
					Reset(null, f.Name);
					return;
				}
			}

			Reset(path, null);
		}

		void Updating(object sender, EventArgs e)
		{
			string dir = string.Empty;

			FarFile file = Panel.CurrentFile;
			if (file != null)
			{
				TreeFile node = (TreeFile)file;
				TreeFile parent = node.Parent;
				if (parent != null)
					dir = parent.Path;
				else
					dir = node.Path;
			}

			if (dir.Length > 0)
			{
				Panel.Info.Title = "Tree: " + dir;
			}
			else
			{
				Panel.Info.Title = "Tree";
				dir = "."; // to avoid empty (Far closes on dots or CtrlPgUp)
			}

			Panel.Info.CurrentDirectory = dir;
		}

		void Reset(string path, string current)
		{
			// set location
			if (!string.IsNullOrEmpty(path) && path != ".")
				A.Psf.Engine.SessionState.Path.SetLocation(path);

			// get location
			PowerPath location = new PowerPath(A.Psf.Engine.SessionState.Path.CurrentLocation);
			if (!My.ProviderInfoEx.IsNavigation(location.Provider))
				throw new RuntimeException("Provider '" + location.Provider + "' does not support navigation.");

			// get root item
			Collection<PSObject> items = A.Psf.Engine.SessionState.InvokeProvider.Item.Get(new string[] { "." }, true, true);
			PSObject data = items[0];

			// reset roots
			RootFiles.Clear();
			TreeFile ti = new TreeFile();
			ti.Name = location.Path; // special case name for the root
			ti.Fill = Fill;
			ti.Data = data;
			RootFiles.Add(ti);
			ti.Expand();

			// set current
			if (current != null)
			{
				foreach (TreeFile t in ti.ChildFiles)
				{
					if (Kit.Compare(t.Name, current) == 0)
					{
						Panel.PostFile(t);
						break;
					}
				}
			}

			// panel info
			Panel.Info.CurrentDirectory = ti.Path;
		}

		void Fill(object sender, EventArgs e)
		{
			TreeFile ti = sender as TreeFile;

			// get
			Collection<PSObject> items = A.Psf.Engine.InvokeProvider.ChildItem.Get(new string[] { ti.Path }, false, true, true);

			foreach (PSObject item in items)
			{
				if (!(bool)item.Properties["PSIsContainer"].Value)
					continue;

				TreeFile t = ti.ChildFiles.Add();

				// name
				t.Data = item;
				t.Name = (string)item.Properties["PSChildName"].Value;
				t.Fill = Fill;

				// description
				PSPropertyInfo pi = item.Properties["FarDescription"];
				if (pi != null && pi.Value != null)
					t.Description = pi.Value.ToString();

				// attributes _090810_180151
				FileSystemInfo fsi = item.BaseObject as FileSystemInfo;
				if (fsi != null)
					t.Attributes = fsi.Attributes;
				else
					t.Attributes = FileAttributes.Directory;
			}
		}

		/// <summary>
		/// Navigation.
		/// </summary>
		internal override void OnSettingDirectory(SettingDirectoryEventArgs e)
		{
			// done
			e.Ignore = true;

			// e.g. [CtrlQ]
			if ((e.Mode & OperationModes.Silent) != 0)
				return;

			string newLocation = e.Name;
			string toSetCurrent = null;
			TreeFile root = RootFiles[0];
			if (newLocation == ".." && RootFiles.Count == 1) //???
			{
				//??? dupe from ItemPanel
				newLocation = root.Path;
				if (newLocation.EndsWith("\\", StringComparison.Ordinal))
				{
					newLocation = null;
				}
				else
				{
					int i = newLocation.LastIndexOf('\\');
					if (i < 0)
					{
						newLocation = null;
					}
					else
					{
						//! Issue with names like z:|z - Far doesn't set cursor on it
						if (newLocation.Length > i + 2 && newLocation[i + 2] == ':')
							Panel.PostName(newLocation.Substring(i + 1));

						newLocation = newLocation.Substring(0, i);
						if (newLocation.StartsWith("\\\\", StringComparison.Ordinal)) //HACK network path
						{
							i = newLocation.LastIndexOf('\\');
							if (i <= 1)
							{
								// show computer shares menu
								string computer = newLocation.Substring(2);
								string share = SelectShare(computer);
								if (share == null)
									newLocation = null;
								else
									newLocation += "\\" + share;
							}
						}

						// add \, else we can't step to the root from level 1
						if (newLocation != null)
							newLocation += "\\";
					}
				}

				toSetCurrent = My.PathEx.GetFileName(root.Name);
			}

			if (newLocation != null)
				Reset(newLocation, toSetCurrent);
		}

		/// <summary>
		/// Opens <see cref="MemberPanel"/> for a file.
		/// File <c>Data</c> must not be null.
		/// </summary>
		internal override MemberPanel OpenFileMembers(FarFile file)
		{
			// get data
			TreeFile t = (TreeFile)file;
			if (t.Data == null)
				return null;

			//! use null as parent: this panel can be not open now
			MemberPanel r = new MemberPanel(t.Data);
			r.ShowAsChild(null);
			return r;
		}

		/// <summary>
		/// Opens path on another panel (FileSystem) or ItemPanel for other providers.
		/// </summary>
		public override void OpenFile(FarFile file)
		{
			// base
			if (UserWants != UserAction.Enter)
			{
				base.OpenFile(file);
				return;
			}

			// get data
			TreeFile node = (TreeFile)file;
			PSObject data = node.Data as PSObject;
			ProviderInfo provider = (ProviderInfo)data.Properties["PSProvider"].Value;

			// open at the passive panel
			if (provider.Name == "FileSystem")
			{
				A.Far.Panel2.Path = node.Path;
				A.Far.Panel2.Update(false);
				A.Far.Panel2.Redraw();
			}
			// open at the same panel as child
			else
			{
				ItemPanel panel = new ItemPanel(node.Path);
				panel.ShowAsChild(this);
			}
		}

		internal override void UIAttributes()
		{
			FarFile file = Panel.CurrentFile;
			if (file == null)
				return;

			TreeFile node = (TreeFile)file;
			PSObject data = node.Data as PSObject;
			if (data == null)
				return;

			// validate provider
			ProviderInfo provider = (ProviderInfo)data.Properties["PSProvider"].Value;
			if (!My.ProviderInfoEx.HasProperty(provider))
			{
				A.Msg(Res.NotSupportedByProvider);
				return;
			}

			// show property panel
			(new PropertyPanel(node.Path)).ShowAsChild(this);
		}

		/// <summary>
		/// Shows help.
		/// </summary>
		internal override void ShowHelp()
		{
			A.Far.ShowHelp(A.Psf.AppHome, "FolderTree", HelpOptions.Path);
		}

	}
}
