
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

/*
_090810_180151
 * File system: use and process Hidden attributes in the expected way.
 * Folders use a directory flag (to consume native Far highlighting of directories, not files!).
*/

using System;
using System.Management.Automation;
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
		/// New folder tree with a folder explorer.
		/// </summary>
		/// <param name="explorer">The panel explorer.</param>
		public FolderTree(FolderExplorer explorer) : base(explorer)
		{
			// _091015_190130 Use of UpdateInfo is problematic: it is called after Close()
			// and somehow Close() may not work. To watch this in future Far versions.
			// For now use redrawing, it looks working fine.
		}
		/// <summary>
		/// New folder tree at the given location path.
		/// </summary>
		/// <param name="path">The location path.</param>
		public FolderTree(string path)
			: this(new FolderExplorer(path))
		{
			// post name
			if (string.IsNullOrEmpty(path) || path == ".")
			{
				FarFile currentFile = Far.Api.Panel.CurrentFile;
				if (currentFile != null)
					PostName(currentFile.Name);
			}
		}
		/// <summary>
		/// New folder tree at the current location.
		/// </summary>
		public FolderTree()
			: this((string)null) { }
		/// <inheritdoc/>
		public override void UIRedrawing(PanelEventArgs e)
		{
			base.UIRedrawing(e);
			
			string dir = string.Empty;

			var file = CurrentFile;
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
				Title = "Tree: " + dir;
			}
			else
			{
				Title = "Tree";
				dir = "*"; // to avoid empty (Far closes on dots or CtrlPgUp); STOP: see _130117_234326
			}

			//! panel directory is not the same as the explorer location
			CurrentLocation = dir;
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
			MemberPanel r = new MemberPanel(new MemberExplorer(t.Data));
			r.OpenChild(null);
			return r;
		}
		/// <summary>
		/// Opens the path on another panel for the FileSystem provider or an item panel as a child of this panel for other providers.
		/// </summary>
		/// <param name="file">The file to open.</param>
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
				Far.Api.Panel2.CurrentDirectory = node.Path;
				Far.Api.Panel2.Update(false);
				Far.Api.Panel2.Redraw();
			}
			// open at the same panel as child
			else
			{
				ItemPanel panel = new ItemPanel(node.Path);
				panel.OpenChild(this);
			}
		}
		internal override void UIAttributes()
		{
			FarFile file = CurrentFile;
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
				A.Message(Res.NotSupportedByProvider);
				return;
			}

			// show property panel
			(new PropertyExplorer(node.Path)).OpenPanelChild(this);
		}
		/// <summary>
		/// Shows help.
		/// </summary>
		internal override void ShowHelpForPanel()
		{
			Far.Api.ShowHelpTopic("FolderTree");
		}
	}
}
