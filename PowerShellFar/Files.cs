
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
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
	/// Combined delegate or script block action with one parameter.
	/// Script arguments: 0: the target object (sender).
	/// </summary>
	/// <typeparam name="T">Type of the sender.</typeparam>
	public sealed class ScriptAction<T>
	{
		readonly Action<T> _action;
		readonly ScriptBlock _script;
		/// <summary>
		/// New action with a delegate.
		/// </summary>
		/// <param name="action">The delegate.</param>
		public ScriptAction(Action<T> action)
		{
			if (action == null) throw new ArgumentNullException("action");
			_action = action;
		}
		/// <summary>
		/// New action with a script block.
		/// </summary>
		/// <param name="script">The script block.</param>
		public ScriptAction(ScriptBlock script)
		{
			if (script == null) throw new ArgumentNullException("script");
			_script = script;
		}
		/// <summary>
		/// Invokes the action.
		/// </summary>
		/// <param name="sender">The sender.</param>
		public void Invoke(T sender)
		{
			if (_action != null)
				_action(sender);
			else if (_script != null)
				A.InvokeScriptReturnAsIs(_script, sender);
		}
	}

	/// <summary>
	/// Combined delegate or script block with two parameters.
	/// Script arguments: 0: the target object (sender); 1: the arguments.
	/// </summary>
	/// <typeparam name="T">Event arguments type.</typeparam>
	public sealed class ScriptHandler<T> where T : EventArgs
	{
		readonly EventHandler<T> _handler;
		readonly ScriptBlock _script;
		/// <summary>
		/// New handler with a delegate.
		/// </summary>
		/// <param name="handler">The delegate.</param>
		public ScriptHandler(EventHandler<T> handler)
		{
			if (handler == null) throw new ArgumentNullException("handler");
			_handler = handler;
		}
		/// <summary>
		/// New handler with a script block. 
		/// </summary>
		/// <param name="handler">The script block.</param>
		public ScriptHandler(ScriptBlock handler)
		{
			if (handler == null) throw new ArgumentNullException("handler");
			_script = handler;
		}
		/// <summary>
		/// New handler with a delegate or a script block.
		/// </summary>
		/// <param name="handler">The delegate or script block.</param>
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
		/// <summary>
		/// Invokes the handler.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The arguments.</param>
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
		/// INTERNAL
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
		/// <inheritdoc/>
		protected override void ClearItems()
		{
			// detach all
			foreach (TreeFile item in this)
				item.Parent = null;

			// clear
			base.ClearItems();
		}
		/// <inheritdoc/>
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
		/// <inheritdoc/>
		protected override void RemoveItem(int index)
		{
			// get it, index is checked
			var it = this[index];

			// remove
			base.RemoveItem(index);

			// detach
			it.Parent = null;
		}
		/// <inheritdoc/>
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
		/// <summary>
		/// New comparer with a meta data for comparison.
		/// </summary>
		/// <param name="meta">The meta for comparison.</param>
		public FileMetaComparer(Meta meta)
		{
			if (meta == null) throw new ArgumentNullException("meta");
			_meta = meta;
		}
		/// <summary>
		/// New comparer with a property used on comparison.
		/// </summary>
		/// <param name="property">The property name.</param>
		public FileMetaComparer(string property)
		{
			if (property == null) throw new ArgumentNullException("property");
			_meta = new Meta(property);
		}
		/// <inheritdoc/>
		public override bool Equals(FarFile x, FarFile y)
		{
			if (x == null || y == null)
				return x == null && y == null;
			else
				return object.Equals(_meta.GetValue(x.Data), _meta.GetValue(y.Data));
		}
		/// <inheritdoc/>
		public override int GetHashCode(FarFile obj)
		{
			if (obj == null || obj.Data == null)
				return 0;

			var value = _meta.GetValue(obj.Data);
			return value == null ? 0 : value.GetHashCode();
		}
	}
}
