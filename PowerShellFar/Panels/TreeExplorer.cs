
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using FarNet;

namespace PowerShellFar
{
	class TreeExplorerGetFilesParameter
	{
		public bool ShowHidden { get; set; }
	}
	/// <summary>
	/// Explorer of a tree.
	/// </summary>
	public class TreeExplorer : Explorer
	{
		const string TypeIdString = "7c3f54bc-721c-4bc0-8d05-afbd526561f6";
		readonly TreeFileCollection _RootFiles = new TreeFileCollection(null);
		readonly List<FarFile> _Files = new List<FarFile>();
		/// <summary>
		/// Root files.
		/// </summary>
		public TreeFileCollection RootFiles
		{
			get { return _RootFiles; }
		}
		/// <summary>
		/// New tree explorer.
		/// </summary>
		public TreeExplorer() : base(new Guid(TypeIdString)) { }
		/// <inheritdoc/>
		protected TreeExplorer(Guid typeId) : base(typeId) { }
		/// <inheritdoc/>
		public override IList<FarFile> GetFiles(GetFilesEventArgs args)
		{
			if (args == null) return null;

			_Files.Clear();

			var parameter = args.ParameterOrDefault<TreeExplorerGetFilesParameter>();

			foreach (TreeFile ti in _RootFiles)
				AddFileFromTreeItem(ti, parameter.ShowHidden);

			return _Files;
		}
		void AddFileFromTreeItem(TreeFile item, bool showHidden)
		{
			if (!showHidden && item.IsHidden)
				return;

			int level = item.Level;

			string nodePrefix = new string(' ', level * 2);

			if (item.IsNode)
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

			_Files.Add(item);

			if (item._State == 1)
			{
				foreach (TreeFile ti in item.ChildFiles)
					AddFileFromTreeItem(ti, showHidden);
			}
		}
	}
}
