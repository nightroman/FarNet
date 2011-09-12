
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
	/// Combined delegate or script action with one parameter.
	/// Script variables: <c>$this</c> is the target object (sender).
	/// </summary>
	public sealed class ScriptAction<T>
	{
		readonly Action<T> _action;
		readonly ScriptBlock _script;
		///
		public ScriptAction(Action<T> action)
		{
			if (action == null) throw new ArgumentNullException("action");
			_action = action;
		}
		///
		public ScriptAction(ScriptBlock script)
		{
			if (script == null) throw new ArgumentNullException("script");
			_script = script;
		}
		///
		public void Invoke(T sender)
		{
			if (_action != null)
				_action(sender);
			else if (_script != null)
				A.InvokeScriptReturnAsIs(_script, sender, null);
		}
	}

	/// <summary>
	/// Combined delegate or script with two parameters.
	/// Script variables: <c>$this</c> is the target object (sender), <c>$_</c> is the arguments.
	/// </summary>
	public sealed class ScriptHandler<T> where T : EventArgs
	{
		readonly EventHandler<T> _handler;
		readonly ScriptBlock _script;
		///
		public ScriptHandler(EventHandler<T> handler)
		{
			if (handler == null) throw new ArgumentNullException("handler");
			_handler = handler;
		}
		///
		public ScriptHandler(ScriptBlock handler)
		{
			if (handler == null) throw new ArgumentNullException("handler");
			_script = handler;
		}
		///
		public ScriptHandler(object handler)
		{
			if (handler == null) throw new ArgumentNullException("handler");
			EventHandler<T> asEventHandler;
			ScriptBlock asScriptBlock;
			if (null != (asEventHandler = handler as EventHandler<T>))
				_handler = asEventHandler;
			else if (null != (asScriptBlock = handler as ScriptBlock))
				_script = asScriptBlock;
			else
				throw new ArgumentException("Invalid handler type.", "handler");
		}
		///
		public void Invoke(object sender, T args)
		{
			if (_handler != null)
				_handler(sender, args);
			else if (_script != null)
				A.InvokeScriptReturnAsIs(_script, sender, args);
		}
	}

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
		/// Item path. It can be used for filling, for example.
		/// </summary>
		public string Path //????
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
		/// Action that fills child items <see cref="ChildFiles"/>.
		/// Normally it uses <see cref="Name"/> or <see cref="Path"/> to recognize children to add.
		/// If this info is not enough, use <see cref="FarFile.Data"/> property to store extra information.
		/// </summary>
		public ScriptAction<TreeFile> Fill { get; set; }
		/// <summary>
		/// Gets true if it is a node item, i.e. not a leaf item.
		/// </summary>
		public bool IsNode { get { return Fill != null; } }
		internal void FillNode()
		{
			Fill.Invoke(this);
		}
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
			if (!IsNode) throw new InvalidOperationException("Fill handler is null.");

			FillNode();
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
	/// Compares files by meta data of the attached file data.
	/// </summary>
	public sealed class FileMetaComparer : EqualityComparer<FarFile>
	{
		readonly Meta _meta;
		///
		public FileMetaComparer(Meta meta)
		{
			if (meta == null) throw new ArgumentNullException("meta");
			_meta = meta;
		}
		///
		public FileMetaComparer(string property)
		{
			if (property == null) throw new ArgumentNullException("property");
			_meta = new Meta(property);
		}
		///
		public override bool Equals(FarFile x, FarFile y)
		{
			if (x == null || y == null)
				return x == null && y == null;
			else
				return object.Equals(_meta.GetValue(x.Data), _meta.GetValue(y.Data));
		}
		///
		public override int GetHashCode(FarFile obj)
		{
			if (obj == null || obj.Data == null)
				return 0;

			var value = _meta.GetValue(obj.Data);
			return value == null ? 0 : value.GetHashCode();
		}
	}
}
