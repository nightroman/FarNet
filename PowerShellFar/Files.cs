/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Item of <see cref="TreePanel"/>.
	/// </summary>
	public sealed class TreeFile : FarFile
	{
		/// <summary>
		/// File name.
		/// </summary>
		public override string Name { get; set; }
		/// <summary>
		/// Used internally.
		/// </summary>
		public override string Owner { get; set; }
		/// <summary>
		/// File description.
		/// </summary>
		public override string Description { get; set; }
		/// <summary>
		/// User data.
		/// </summary>
		public override object Data { get; set; }

		/// <summary>
		/// 0: not yet opened and not filled; +1: opened and filled; -1: closed and filled.
		/// </summary>
		internal int _State;

		/// <summary>
		/// Creates an tree file.
		/// </summary>
		/// <remarks>
		/// This method is normaly not recommended, use <see cref="TreeFileCollection.Add()"/>
		/// to create and add a new file to its parent in one shot.
		/// </remarks>
		public TreeFile()
		{
			ChildFiles = new TreeFileCollection(this);
		}

		/// <summary>
		/// Item path. It can be used by <see cref="Fill"/>, for example.
		/// </summary>
		public string Path
		{
			get
			{
				if (Parent == null)
					return Name;
				else
					return string.Concat(Parent.Path.TrimEnd('\\'), "\\", Name);
			}
		}

		/// <summary>
		/// Number of child files to be displayed.
		/// </summary>
		public override long Length
		{
			get
			{
				return Parent == null ? ChildFiles.Count + 1 : ChildFiles.Count;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Parent item.
		/// </summary>
		public TreeFile Parent
		{
			get;
			internal set;
		}

		/// <summary>
		/// Handler that fills child items <see cref="ChildFiles"/>.
		/// Normally it uses <see cref="Name"/> or <see cref="Path"/> to recognize children to add.
		/// If this info is not enough, use <see cref="FarFile.Data"/> property to store extra information.
		/// </summary>
		public EventHandler Fill { get; set; }

		/// <summary>
		/// Item level. Top items have level 0, their children have level 1 and so on.
		/// </summary>
		public int Level
		{
			get
			{
				int r = 0;
				for (TreeFile parent = Parent; parent != null; parent = parent.Parent)
					++r;
				return r;
			}
		}

		/// <summary>
		/// Item root.
		/// </summary>
		public TreeFile Root
		{
			get
			{
				for (TreeFile ti = this; ; ti = ti.Parent)
					if (ti.Parent == null)
						return ti;
			}
		}

		/// <summary>
		/// Expands the child files. <see cref="Fill"/> must be defined.
		/// </summary>
		public void Expand()
		{
			if (Fill == null)
				throw new InvalidOperationException("Fill handler is null.");

			Fill(this, null);
			_State = 1;
		}

		/// <summary>
		/// Child items. They are filled by <see cref="Fill"/>.
		/// </summary>
		public TreeFileCollection ChildFiles
		{
			get;
			private set;
		}

		/// <summary>
		/// For internal use.
		/// </summary>
		public override FileAttributes Attributes { get; set; } // _090810_180151
	}

	/// <summary>
	/// Collection of tree files, children of a parent file.
	/// </summary>
	public sealed class TreeFileCollection : Collection<TreeFile>
	{
		TreeFile Parent;

		internal TreeFileCollection(TreeFile parent)
		{
			Parent = parent;
		}

		/// <summary>
		/// Creates a new child file and adds it to the collection.
		/// </summary>
		public TreeFile Add()
		{
			TreeFile r = new TreeFile();
			Add(r);
			return r;
		}

		///
		protected override void ClearItems()
		{
			// detach all
			foreach (TreeFile item in this)
				item.Parent = null;

			// clear
			base.ClearItems();
		}

		///
		protected override void InsertItem(int index, TreeFile item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			if (item.Parent != null)
				throw new ArgumentException("item has a parent");

			// insert, it will check the index
			base.InsertItem(index, item);

			// attach
			item.Parent = Parent;
		}

		///
		protected override void RemoveItem(int index)
		{
			// get it, index is checked
			var it = this[index];

			// remove
			base.RemoveItem(index);

			// detach
			it.Parent = null;
		}

		///
		protected override void SetItem(int index, TreeFile item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			if (item.Parent != null)
				throw new ArgumentException("item has a parent");

			// get it, index is checked
			var it = this[index];

			// detach
			it.Parent = null;

			// set it, attach
			base.SetItem(index, item);
			item.Parent = Parent;
		}
	}

	/// <summary>
	/// File system file or directory.
	/// </summary>
	public sealed class FSFile : FarFile
	{
		PSObject Value;

		FileSystemInfo Item { get { return (FileSystemInfo)Value.BaseObject; } }

		///
		public FSFile(PSObject value) //! use of the original PSObject is important, PSObject are better pipelined to cmdlets
		{
			if (value == null)
				throw new ArgumentNullException("value");

			Value = value;
		}

		/// <summary>
		/// File name.
		/// </summary>
		public override string Name
		{
			get { return Item.Name; }
		}

		/// <summary>
		/// Description.
		/// </summary>
		public override string Description
		{
			get
			{
				PSPropertyInfo pi = PSObject.AsPSObject(Item).Properties["FarDescription"]; //??? use or kill
				if (pi == null)
					return null;
				
				object value = pi.Value;
				return value == null ? null : value.ToString();
			}
		}

		/// <summary>
		/// User data.
		/// </summary>
		public override object Data
		{
			get { return Value; }
		}

		/// <summary>
		/// Creation time.
		/// </summary>
		public override DateTime CreationTime
		{
			get { return Item.CreationTime; }
		}

		/// <summary>
		/// Last access time.
		/// </summary>
		public override DateTime LastAccessTime
		{
			get { return Item.LastAccessTime; }
		}

		/// <summary>
		/// Last access time.
		/// </summary>
		public override DateTime LastWriteTime
		{
			get { return Item.LastWriteTime; }
		}

		/// <summary>
		/// File length.
		/// </summary>
		public override long Length
		{
			get { return Item is FileInfo ? ((FileInfo)Item).Length : 0; }
		}

		/// <summary>
		/// File attributes.
		/// </summary>
		public override FileAttributes Attributes
		{
			get { return Item.Attributes; }
		}

		///
		public override string ToString()
		{
			return Item.ToString();
		}
	}
}
