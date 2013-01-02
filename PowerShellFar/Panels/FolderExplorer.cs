
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Folder tree explorer.
	/// </summary>
	public sealed class FolderExplorer : TreeExplorer
	{
		const string TypeIdString = "c9b2ddc2-80d8-4074-9cf7-d009b4ef7ffb";
		/// <summary>
		/// New folder explorer.
		/// </summary>
		public FolderExplorer(string path)
			: base(new Guid(TypeIdString))
		{
			Reset(path);
		}
		///
		public override Panel CreatePanel()
		{
			return new FolderTree(this);
		}
		static readonly ScriptAction<TreeFile> TheFill = new ScriptAction<TreeFile>(Fill);
		static void Fill(TreeFile node)
		{
			// get
			Collection<PSObject> items = A.GetChildItems(node.Path);

			foreach (PSObject item in items)
			{
				if (!(bool)item.Properties["PSIsContainer"].Value)
					continue;

				TreeFile t = node.ChildFiles.Add();

				// name
				t.Data = item;
				t.Name = (string)item.Properties["PSChildName"].Value;
				t.Fill = TheFill;

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
		void Reset(string path)
		{
			// set location
			if (!string.IsNullOrEmpty(path) && path != ".")
				A.Psf.Engine.SessionState.Path.SetLocation(path);

			// get location
			PathInfoEx location = new PathInfoEx(A.Psf.Engine.SessionState.Path.CurrentLocation);
			if (!My.ProviderInfoEx.IsNavigation(location.Provider))
				throw new RuntimeException("Provider '" + location.Provider + "' does not support navigation.");

			// get root item
			Collection<PSObject> items = A.Psf.Engine.SessionState.InvokeProvider.Item.Get(new string[] { "." }, true, true);
			PSObject data = items[0];

			// reset roots
			RootFiles.Clear();
			TreeFile ti = new TreeFile();
			ti.Name = location.Path; // special case name for the root
			ti.Fill = TheFill;
			ti.Data = data;
			RootFiles.Add(ti);
			ti.Expand();

			// panel info
			Location = ti.Path;
		}
		///
		public override Explorer ExploreParent(ExploreParentEventArgs args)
		{
			if (args == null) return null;

			string newLocation = My.PathEx.GetDirectoryName(Location); //???? GetDirectoryName to add '\' for the root like IO.Path does
			if (newLocation.Length == 0)
				return null;
			
			if (newLocation.EndsWith(":", StringComparison.Ordinal))
				newLocation += "\\";
			if (newLocation.Equals(Location, StringComparison.OrdinalIgnoreCase))
				return null;

			args.PostName = My.PathEx.GetFileName(Location);
			
			return new FolderExplorer(newLocation);
		}
	}
}
